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

using UnityEngine;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace ChudOS.Libraries;

public static class GunLib
{
    public static GameObject? Pointer;
    private static LineRenderer? Line;
    public static RaycastHit LastHit;
    public static bool HasHit;
    public static bool Enabled;
    private static RaycastHit[] Hits = new RaycastHit[64];
    private static Camera? DesktopCamera;
    private static float NextCameraScan;
    private static bool RayComplete;
    private static bool leftHanded;
    public static Transform AimHand => GorillaTagger.Instance == null ? null! : leftHanded ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;
    [MenuButton("Gun Hand", ButtonType.Option, Category = "Settings", Order = 152, Options = new[] { "Right", "Left" }, Notify = false)]
    public static void SetHand() { leftHanded = (ChudOS.Core.Managers.ButtonManager.GetButton("Gun Hand")?.OptionIndex ?? 0) == 1; Hide(); PortBindings.Refresh(); }
    public static bool AimPressed => XRSettings.isDeviceActive ? ControllerInputPoller.instance != null && (leftHanded ? InputLib.LeftGrab : InputLib.RightGrab) : Mouse.current?.rightButton.isPressed == true;
    public static bool SelectPressed => XRSettings.isDeviceActive ? ControllerInputPoller.instance != null && (leftHanded ? InputLib.LeftTrigger : InputLib.RightTrigger) : Mouse.current?.leftButton.isPressed == true;

    private static Ray AimRay(Transform hand)
    {
        if (XRSettings.isDeviceActive || Mouse.current == null) return new Ray(hand.position, hand.forward);
        if (DesktopCamera == null || !DesktopCamera.isActiveAndEnabled || Time.unscaledTime >= NextCameraScan)
        {
            NextCameraScan = Time.unscaledTime + 1f;
            DesktopCamera = null;
            foreach (Camera camera in Camera.allCameras)
                if (camera.isActiveAndEnabled && camera.stereoTargetEye == StereoTargetEyeMask.None && camera.targetTexture == null && (DesktopCamera == null || camera.depth > DesktopCamera.depth)) DesktopCamera = camera;
            DesktopCamera ??= Camera.main;
        }
        return DesktopCamera != null ? DesktopCamera.ScreenPointToRay(Mouse.current.position.ReadValue()) : new Ray(hand.position, hand.forward);
    }

    public static bool UpdateGun(Transform hand, bool active, bool select, out Vector3 point, float distance = 80f)
        => UpdateGunInternal(hand, active, select, out point, distance, false);

    private static bool UpdateGunInternal(Transform hand, bool active, bool select, out Vector3 point, float distance, bool playerTargets)
    {
        RayComplete = false;
        point = Vector3.zero;
        if (!active || hand == null || !Finite(hand.position) || !Finite(hand.forward) || float.IsNaN(distance) || float.IsInfinity(distance) || distance <= 0f) { Hide(); return false; }
        distance = Mathf.Min(distance, 500f);
        if (Line == null) Create();
        Ray ray = AimRay(hand);
        Vector3 origin = ray.origin;
        Vector3 direction = ray.direction;
        if (!Finite(origin) || !Finite(direction) || direction.sqrMagnitude < 0.0001f) { Hide(); return false; }
        HasHit = RaycastNearest(origin, direction, distance, playerTargets, out LastHit);
        point = HasHit ? LastHit.point : origin + direction * distance;
        if (Pointer != null) { Pointer.SetActive(true); Pointer.transform.position = point; }
        if (Line != null) { Line.enabled = true; Line.SetPosition(0, origin); Line.SetPosition(1, point); }
        Enabled = true;
        return select && HasHit;
    }

    public static bool UpdateRigGun(Transform hand, bool active, bool select, ref VRRig? target, float distance = 80f)
    {
        if (NetworkSystem.Instance == null || !NetworkSystem.Instance.InRoom)
        {
            target = null;
            Hide();
            return false;
        }

        bool selected = UpdateGunInternal(hand, active, select, out _, distance, true);
        if (!RayComplete) { target = null; Hide(); return false; }
        if (!active || !select || hand == null || !Finite(hand.position) || !Finite(hand.forward) || float.IsNaN(distance) || float.IsInfinity(distance) || distance <= 0f)
        {
            target = null;
            return false;
        }

        if (!IsRemoteRig(target) || (target!.transform.position - hand.position).sqrMagnitude > distance * distance)
        {
            target = null;
            if (selected && LastHit.collider != null)
            {
                VRRig candidate = LastHit.collider.GetComponentInParent<VRRig>();
                if (IsRemoteRig(candidate)) target = candidate;
            }
            if (target == null) target = FindRigOnRay(AimRay(hand), HasHit ? Mathf.Min(distance, LastHit.distance) : distance);
        }

        if (target == null) return false;
        Vector3 point = target.head != null && target.head.rigTarget != null ? target.head.rigTarget.position : target.transform.position;
        if (Pointer != null) Pointer.transform.position = point;
        if (Line != null) Line.SetPosition(1, point);
        return true;
    }

    private static bool IsRemoteRig(VRRig? rig) => rig != null && rig != VRRig.LocalRig && rig.gameObject.activeInHierarchy &&
        !rig.isLocal && !rig.isOfflineVRRig && Finite(rig.transform.position) &&
        !float.IsNaN(rig.scaleFactor) && !float.IsInfinity(rig.scaleFactor) && rig.scaleFactor > 0f &&
        rig.head.rigTarget != null && rig.leftHand.rigTarget != null && rig.rightHand.rigTarget != null &&
        VRRigCache.ActiveRigs != null && VRRigCache.ActiveRigs.Contains(rig);

    private static bool Finite(Vector3 value) => !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
        !float.IsNaN(value.y) && !float.IsInfinity(value.y) && !float.IsNaN(value.z) && !float.IsInfinity(value.z);

    private static VRRig? FindRigOnRay(Ray ray, float distance)
    {
        if (VRRigCache.ActiveRigs == null) return null;
        VRRig? result = null;
        float nearest = Mathf.Min(distance, 500f);
        foreach (VRRig rig in VRRigCache.ActiveRigs)
        {
            if (!IsRemoteRig(rig)) continue;
            float radius = 0.3f * Mathf.Clamp(rig.scaleFactor, 0.5f, 2f);
            Vector3 body = rig.bodyRenderer != null ? rig.bodyRenderer.transform.position : rig.transform.position;
            for (int i = 0; i < 2; i++)
            {
                Vector3 point = i == 0 ? body : rig.head.rigTarget.position;
                float along = Vector3.Dot(point - ray.origin, ray.direction);
                if (along <= 0f || along > nearest) continue;
                if ((point - ray.GetPoint(along)).sqrMagnitude > radius * radius) continue;
                nearest = along;
                result = rig;
            }
        }
        return result;
    }

    private static bool RaycastNearest(Vector3 origin, Vector3 direction, float distance, bool playerTargets, out RaycastHit hit)
    {
        RayComplete = true;
        hit = default;
        int count;
        while (true)
        {
            count = Physics.RaycastNonAlloc(origin, direction, Hits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
            if (count < Hits.Length) break;
            if (Hits.Length >= 512) { RayComplete = false; return false; }
            Hits = new RaycastHit[Hits.Length * 2];
        }
        float closest = distance;
        bool found = false;
        for (int i = 0; i < count; i++)
        {
            Collider collider = Hits[i].collider;
            if (collider == null || collider.gameObject == Pointer) continue;
            VRRig rig = collider.GetComponentInParent<VRRig>();
            if (rig != null && !IsRemoteRig(rig)) continue;
            if (GTPlayer.Instance != null && collider.transform.IsChildOf(GTPlayer.Instance.transform)) continue;
            if (collider.isTrigger && (!playerTargets || rig == null)) continue;
            if (Hits[i].distance > closest) continue;
            closest = Hits[i].distance;
            hit = Hits[i];
            found = true;
        }
        return found;
    }

    private static void Create()
    {
        GameObject root = new GameObject(Constantss.GUID + ".GunLine");
        Line = root.AddComponent<LineRenderer>();
        Line.positionCount = 2;
        Line.startWidth = 0.008f;
        Line.endWidth = 0.004f;
        Line.useWorldSpace = true;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) Line.material = new Material(shader);
        Line.startColor = Color.magenta;
        Line.endColor = Color.white;
        Pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Pointer.name = Constantss.GUID + ".GunPointer";
        Pointer.layer = 2;
        Pointer.transform.localScale = Vector3.one * 0.035f;
        Collider collider = Pointer.GetComponent<Collider>();
        if (collider != null) { collider.enabled = false; UnityEngine.Object.Destroy(collider); }
    }
    public static void Hide() { Enabled = false; HasHit = false; LastHit = default; if (Line != null) Line.enabled = false; if (Pointer != null) Pointer.SetActive(false); }
    public static void Cleanup()
    {
        if (Line != null) { if (Line.material != null) UnityEngine.Object.Destroy(Line.material); UnityEngine.Object.Destroy(Line.gameObject); }
        if (Pointer != null) UnityEngine.Object.Destroy(Pointer);
        Line = null; Pointer = null; HasHit = false; Enabled = false;
        DesktopCamera = null;
        Array.Clear(Hits, 0, Hits.Length);
    }
}
