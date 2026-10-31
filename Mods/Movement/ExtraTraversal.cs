// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Movement;

public static class ExtraTraversal
{
    private static readonly CreativeTools.Session Session = new();
    private static readonly List<Transform> Checkpoints = new();
    private static readonly Vector3[] Anchors = new Vector3[2];
    private static readonly bool[] Attached = new bool[2];
    private static bool placeInput, jumpInput, deleteInput;
    private static float nextHop;
    private static bool Ready()
    {
        if (Session.Changed()) Clear();
        if (!CreativeTools.Ready) { Clear(); return false; }
        PortTools.Register(Clear);
        return true;
    }
    [MenuButton("Advanced Checkpoints", ButtonType.Toggle, Category = "Movement", Order = 410, Controls = "(X)(A)(B)(RG)", runOption = RunOption.FixedUpdate, Description = "X places a checkpoint; A returns to the nearest one; B removes it; hold RG near one to move it")]
    public static void AdvancedCheckpoints()
    {
        if (!Ready()) return;
        Vector3 hand = CreativeTools.Right.position;
        bool place = CreativeTools.Pressed(InputLib.LeftControllerXButton, ref placeInput), jump = CreativeTools.Pressed(InputLib.RightControllerAButton, ref jumpInput), delete = CreativeTools.Pressed(InputLib.RightControllerBButton, ref deleteInput);
        if (place && Checkpoints.Count < 16)
        {
            Transform marker = CreativeTools.Marker("Checkpoint", 0.12f * CreativeTools.Scale);
            marker.position = CreativeTools.Position;
            Checkpoints.Add(marker);
        }
        int nearest = -1;
        float distance = float.PositiveInfinity;
        for (int i = Checkpoints.Count - 1; i >= 0; i--)
        {
            if (Checkpoints[i] == null) { Checkpoints.RemoveAt(i); continue; }
            float current = (Checkpoints[i].position - hand).sqrMagnitude;
            if (current >= distance) continue;
            nearest = i; distance = current;
        }
        if (nearest < 0) return;
        Transform selected = Checkpoints[nearest];
        if (delete) { Object.Destroy(selected.gameObject); Checkpoints.RemoveAt(nearest); return; }
        if (jump) PortTools.Teleport(selected.position);
        else if (InputLib.RightGrab && distance < 0.09f * CreativeTools.Scale * CreativeTools.Scale) selected.position = hand;
    }
    [MenuDisable("Advanced Checkpoints")]
    public static void StopCheckpoints()
    {
        foreach (Transform marker in Checkpoints) if (marker != null) Object.Destroy(marker.gameObject);
        Checkpoints.Clear(); placeInput = jumpInput = deleteInput = false;
    }
    [MenuButton("Climby Hands", ButtonType.Toggle, Category = "Movement", Order = 411, Controls = "(LG)/(RG)", runOption = RunOption.FixedUpdate, Description = "Grip a surface with either hand, then pull yourself using that hand")]
    public static void ClimbyHands()
    {
        if (!Ready()) return;
        Vector3 correction = Vector3.zero;
        int count = 0;
        for (int i = 0; i < 2; i++)
        {
            bool held = i == 0 ? InputLib.LeftGrab : InputLib.RightGrab;
            Vector3 hand = i == 0 ? CreativeTools.Left.position : CreativeTools.Right.position;
            if (!held) { Attached[i] = false; continue; }
            if (!Attached[i] && GTPlayer.Instance.IsHandTouching(i == 0)) { Anchors[i] = hand; Attached[i] = true; }
            if (!Attached[i]) continue;
            Vector3 error = Anchors[i] - hand;
            if (error.magnitude > 0.75f * CreativeTools.Scale) { Attached[i] = false; continue; }
            correction += error; count++;
        }
        if (count == 0) return;
        Rigidbody body = CreativeTools.Body;
        body.linearVelocity = Vector3.ClampMagnitude(correction / (count * Mathf.Max(Time.fixedDeltaTime, 0.001f)), 10f * CreativeTools.Scale);
        if (body.useGravity) body.AddForce(-Physics.gravity, ForceMode.Acceleration);
    }
    [MenuDisable("Climby Hands")]
    public static void StopClimbing() => Attached[0] = Attached[1] = false;
    [MenuButton("Auto Climb", ButtonType.Toggle, Category = "Movement", Order = 412, Controls = "(RG)(LG)", runOption = RunOption.FixedUpdate, Description = "Hold both grips while touching a wall; the selected hand-contact mode controls climbing assistance")]
    public static void AutoClimb()
    {
        if (!Ready() || !InputLib.LeftGrab || !InputLib.RightGrab) return;
        bool left = GTPlayer.Instance.IsHandTouching(true), right = GTPlayer.Instance.IsHandTouching(false);
        if (CreativeSettings.ClimbMode == 0 ? !left || !right : !left && !right) return;
        Vector3 velocity = CreativeTools.Body.linearVelocity;
        velocity.y = Mathf.MoveTowards(velocity.y, CreativeSettings.ClimbPower * CreativeTools.Scale, 20f * CreativeTools.Scale * Time.fixedDeltaTime);
        CreativeTools.Body.linearVelocity = velocity;
    }
    [MenuButton("Bunny Hop", ButtonType.Toggle, Category = "Movement", Order = 413, Controls = "(A)", runOption = RunOption.FixedUpdate, Description = "Hold A to jump again when you land")]
    public static void BunnyHop()
    {
        if (!Ready() || !InputLib.RightControllerAButton || Time.unscaledTime < nextHop) return;
        if (CreativeTools.Body.linearVelocity.y > 0.5f * CreativeTools.Scale || !CreativeTools.Ground(out _)) return;
        Vector3 velocity = CreativeTools.Body.linearVelocity;
        velocity.y = CreativeSettings.HopPower * CreativeTools.Scale;
        CreativeTools.Body.linearVelocity = velocity;
        nextHop = Time.unscaledTime + 0.25f;
    }
    [MenuDisable("Bunny Hop")]
    public static void StopHop() => nextHop = 0f;
    internal static void Clear() { StopCheckpoints(); StopClimbing(); StopHop(); }
}
