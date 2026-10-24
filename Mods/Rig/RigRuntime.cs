// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra
//
// This file is part of ChudOS.
// ChudOS is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 3,
// as published by the Free Software Foundation.
//
// ChudOS is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ChudOS. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Rig;

internal abstract class RigBehaviour
{
    internal bool Enabled { get; private set; }
    internal void Begin() { Enabled = true; Start(); OnEnable(); }
    internal void End() { Enabled = false; OnDisable(); }
    internal void TickUpdate() => Update();
    internal void TickLateUpdate() => LateUpdate();
    protected virtual void Start() { }
    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
    protected virtual void Update() { }
    protected virtual void LateUpdate() { }
}

internal sealed class RigRuntime : MonoBehaviour
{
    private static RigRuntime? instance;
    private RigBehaviour? active;
    private string owner = "";
    private Snapshot? snapshot;
    private bool changing;
    private static readonly string[] ExternalPoses = { "Tag Gun", "Tag Self", "Tag All", "Fake Power Off", "Rewind" };
    private static readonly string[] ExternalGuns = { "Tag Gun", "Teleport Gun", "Draw Gun", "Platform Gun" };

    internal static bool Ready => PortTools.Ready && VRRig.LocalRig != null &&
        VRRig.LocalRig.head.rigTarget != null && VRRig.LocalRig.leftHand.rigTarget != null && VRRig.LocalRig.rightHand.rigTarget != null &&
        GTPlayer.Instance.LeftHand.controllerTransform != null && GTPlayer.Instance.RightHand.controllerTransform != null;
    internal static bool InVR => XRSettings.isDeviceActive;
    internal static bool IsRigEnabled => VRRig.LocalRig != null && VRRig.LocalRig.enabled;
    internal static Vector3 RigPosition
    {
        get => VRRig.LocalRig.transform.position;
        set => VRRig.LocalRig.transform.position = value;
    }
    internal static Quaternion RigRotation
    {
        get => VRRig.LocalRig.transform.rotation;
        set => VRRig.LocalRig.transform.rotation = value;
    }

    private static RigRuntime Get()
    {
        if (instance != null) return instance;
        ChudOS.Core.Main main = Object.FindFirstObjectByType<ChudOS.Core.Main>();
        if (main == null) throw new InvalidOperationException("ChudOS runtime is unavailable.");
        instance = main.GetComponent<RigRuntime>() ?? main.gameObject.AddComponent<RigRuntime>();
        return instance;
    }

    internal static void Enable<T>(string name) where T : RigBehaviour, new()
    {
        RigRuntime manager = Get();
        manager.changing = true;
        try
        {
            foreach (var button in ButtonManager.Buttons)
                if (button.Category == "Rig" && button.Type == ChudOS.Stuff.ButtonType.Toggle &&
                    button.Name != name && button.Name != "Max Quest Score") ButtonManager.SetEnabled(button, false);
            foreach (string other in ExternalPoses) ButtonManager.SetEnabled(other, false);
            if (IsGun(name)) foreach (string other in ExternalGuns) ButtonManager.SetEnabled(other, false);
            manager.Stop();
            manager.owner = name;
            manager.active = new T();
            manager.enabled = true;
            manager.BeginWhenReady();
        }
        catch
        {
            manager.Stop();
            throw;
        }
        finally { manager.changing = false; }
    }

    internal static void Disable(string name)
    {
        if (instance != null && instance.owner == name) instance.Stop();
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(this); return; }
        instance = this;
        enabled = false;
        PortTools.Register(Cleanup);
        ButtonManager.OnButtonsChanged += CheckConflicts;
    }

    private static void Cleanup() { if (instance != null) instance.Stop(); }

    private void CheckConflicts()
    {
        if (changing || active == null) return;
        foreach (string name in ExternalPoses)
            if (ButtonManager.GetButton(name)?.Enabled == true) { ButtonManager.SetEnabled(owner, false); return; }
        if (!IsGun(owner)) return;
        foreach (string name in ExternalGuns)
            if (ButtonManager.GetButton(name)?.Enabled == true) { ButtonManager.SetEnabled(owner, false); return; }
    }

    private static bool IsGun(string name) => name.EndsWith(" Gun", StringComparison.Ordinal);

    private void BeginWhenReady()
    {
        if (!Ready || active == null || active.Enabled) return;
        snapshot = new Snapshot(VRRig.LocalRig);
        active.Begin();
    }

    private void Update() => RunPhase(false);
    private void LateUpdate() => RunPhase(true);

    private void RunPhase(bool late)
    {
        if (active == null) return;
        if (snapshot != null && snapshot.Rig != VRRig.LocalRig)
        {
            ButtonManager.SetEnabled(owner, false);
            return;
        }
        if (!Ready) return;
        try
        {
            BeginWhenReady();
            if (late) active?.TickLateUpdate();
            else active?.TickUpdate();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            ButtonManager.SetEnabled(owner, false);
        }
    }

    private void Stop()
    {
        RigBehaviour? previous = active;
        active = null;
        enabled = false;
        owner = "";
        try { if (previous?.Enabled == true) previous.End(); }
        finally { snapshot?.Restore(); snapshot = null; }
    }

    internal static void ToggleRig(bool enabled)
    {
        if (VRRig.LocalRig == null) return;
        VRRig.LocalRig.enabled = enabled ? instance?.snapshot?.WasEnabled ?? true : false;
    }
    internal static void RestorePose()
    {
        instance?.snapshot?.Restore();
        ToggleRig(true);
    }
    internal static void ToggleRig(bool enabled, Vector3 position)
    {
        ToggleRig(enabled);
        if (VRRig.LocalRig != null) VRRig.LocalRig.transform.position = position;
    }

    private void OnDestroy()
    {
        ButtonManager.OnButtonsChanged -= CheckConflicts;
        Stop();
        if (instance == this) instance = null;
    }

    private sealed class Snapshot
    {
        internal readonly VRRig Rig;
        internal readonly bool WasEnabled;
        private readonly LocalPose[] poses;
        private readonly float leftT, rightT, li, lm, lt, ri, rm, rt;

        internal Snapshot(VRRig rig)
        {
            Rig = rig;
            WasEnabled = rig.enabled;
            poses = new[]
            {
                new LocalPose(rig.transform), new LocalPose(rig.head.rigTarget),
                new LocalPose(rig.leftHand.rigTarget), new LocalPose(rig.rightHand.rigTarget),
                new LocalPose(GTPlayer.Instance.LeftHand.controllerTransform),
                new LocalPose(GTPlayer.Instance.RightHand.controllerTransform)
            };
            leftT = rig.leftHand.calcT; rightT = rig.rightHand.calcT;
            li = rig.leftIndex.calcT; lm = rig.leftMiddle.calcT; lt = rig.leftThumb.calcT;
            ri = rig.rightIndex.calcT; rm = rig.rightMiddle.calcT; rt = rig.rightThumb.calcT;
        }

        internal void Restore()
        {
            foreach (LocalPose pose in poses) pose.Restore();
            if (Rig == null) return;
            Rig.leftHand.calcT = leftT; Rig.rightHand.calcT = rightT;
            Rig.leftIndex.calcT = li; Rig.leftMiddle.calcT = lm; Rig.leftThumb.calcT = lt;
            Rig.rightIndex.calcT = ri; Rig.rightMiddle.calcT = rm; Rig.rightThumb.calcT = rt;
            Rig.enabled = WasEnabled;
        }
    }

    private readonly struct LocalPose
    {
        private readonly Transform target;
        private readonly Vector3 position;
        private readonly Quaternion rotation;
        internal LocalPose(Transform value) { target = value; position = value.localPosition; rotation = value.localRotation; }
        internal void Restore() { if (target != null) { target.localPosition = position; target.localRotation = rotation; } }
    }
}

internal static class RigExtensions
{
    internal static bool IsLocalRig(this VRRig rig) => rig != null && (rig == VRRig.LocalRig || rig.isLocal);
}

internal sealed class RigInput
{
    private static readonly RigInput instance = new();
    internal static RigInput Instance { get { instance.Refresh(); return instance; } }
    internal readonly RigButton RightPrimary = new(), RightSecondary = new(), LeftPrimary = new(), LeftSecondary = new(), LeftGrip = new(), RightGrip = new();
    internal readonly RigStick LeftJoystick = new(), RightJoystick = new();
    private int frame = -1;
    private void Refresh()
    {
        if (frame == Time.frameCount) return;
        frame = Time.frameCount;
        if (ControllerInputPoller.instance == null)
        {
            RightPrimary.Set(false); RightSecondary.Set(false); LeftPrimary.Set(false); LeftSecondary.Set(false);
            LeftGrip.Set(false); RightGrip.Set(false);
            LeftJoystick.Axis = RightJoystick.Axis = Vector2.zero;
            return;
        }
        RightPrimary.Set(InputLib.RightControllerAButton); RightSecondary.Set(InputLib.RightControllerBButton);
        LeftPrimary.Set(InputLib.LeftControllerXButton); LeftSecondary.Set(InputLib.LeftControllerYButton);
        LeftGrip.Set(InputLib.LeftGrab); RightGrip.Set(InputLib.RightGrab);
        LeftJoystick.Axis = InputLib.LeftJoyStickAxis; RightJoystick.Axis = InputLib.RightJoyStickAxis;
    }
}
internal sealed class RigButton
{
    internal bool IsPressed, WasPressed;
    internal void Set(bool current) { WasPressed = current && !IsPressed; IsPressed = current; }
}
internal sealed class RigStick { internal Vector2 Axis; }

internal static class RigKeys
{
    private static UnityEngine.InputSystem.Controls.KeyControl? Get(KeyCode key)
    {
        Keyboard? keyboard = Keyboard.current;
        if (keyboard == null) return null;
        return key switch
        {
            KeyCode.UpArrow => keyboard.upArrowKey, KeyCode.DownArrow => keyboard.downArrowKey,
            KeyCode.LeftArrow => keyboard.leftArrowKey, KeyCode.RightArrow => keyboard.rightArrowKey,
            KeyCode.RightControl => keyboard.rightCtrlKey, KeyCode.RightShift => keyboard.rightShiftKey,
            KeyCode.R => keyboard.rKey, _ => null
        };
    }
    internal static bool Held(KeyCode key) => Get(key)?.isPressed == true;
    internal static bool Pressed(KeyCode key) => Get(key)?.wasPressedThisFrame == true;
}
