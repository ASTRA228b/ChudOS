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

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Movement;
using ChudOS.Stuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Fun;

public static class NewFunMods
{
    private static Camera? freeCamera, desktopCamera;
    private static Vector3 cameraVelocity;
    private static Quaternion freeLookOffset = Quaternion.identity;
    private static bool freeCameraLooking, previousCursorVisible;
    private static CursorLockMode previousCursorLock;
    private static float nextCameraScan, originalFov;
    private static bool fovSaved;
    private static GorillaTagger? tapOwner;
    private static float originalTapVolume;
    internal static bool TapsMuted;
    private static readonly float[] FovValues = { 0f, 60f, 75f, 90f, 110f };

    [MenuButton("Free Camera", ButtonType.Toggle, Category = "Fun", Order = 100, runOption = RunOption.LateUpdate, Description = "WASD or sticks move; Q/E change height; hold right mouse to look")]
    public static void FreeCamera()
    {
        if (!PortTools.Ready || !Application.isFocused)
        {
            StopFreeCameraLook();
            cameraVelocity = Vector3.zero;
            return;
        }
        PortTools.Register(Cleanup);
        Transform head = GorillaTagger.Instance.headCollider.transform;
        if (freeCamera == null)
        {
            GameObject root = new("ChudOS Free Camera");
            freeCamera = root.AddComponent<Camera>();
            Camera source = Camera.main;
            if (source != null) freeCamera.CopyFrom(source);
            freeCamera.stereoTargetEye = StereoTargetEyeMask.None;
            freeCamera.targetTexture = null;
            freeCamera.targetDisplay = 0;
            freeCamera.depth = 100f;
            freeCamera.nearClipPlane = 0.01f;
            freeCamera.enabled = true;
            root.transform.SetPositionAndRotation(head.position, head.rotation);
            freeLookOffset = Quaternion.identity;
        }
        bool headTracking = XRSettings.isDeviceActive;
        if (headTracking) freeCamera.transform.rotation = head.rotation * freeLookOffset;
        UpdateFreeCameraLook();
        if (headTracking) freeLookOffset = Quaternion.Inverse(head.rotation) * freeCamera.transform.rotation;
        Vector2 stick = InputLib.LeftJoyStickAxis;
        float up = InputLib.RightJoyStickAxis.y;
        Keyboard? keys = Keyboard.current;
        if (keys != null)
        {
            stick.x += (keys.dKey.isPressed ? 1f : 0f) - (keys.aKey.isPressed ? 1f : 0f);
            stick.y += (keys.wKey.isPressed ? 1f : 0f) - (keys.sKey.isPressed ? 1f : 0f);
            up += (keys.eKey.isPressed ? 1f : 0f) - (keys.qKey.isPressed ? 1f : 0f);
        }
        Vector3 forward = Vector3.ProjectOnPlane(freeCamera.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 desired = Vector3.ClampMagnitude(right * stick.x + forward * stick.y + Vector3.up * up, 1f) * 8f;
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
        cameraVelocity = Vector3.Lerp(cameraVelocity, desired, 1f - Mathf.Exp(-12.4f * dt));
        freeCamera.transform.position += cameraVelocity * dt;
        int mode = ButtonManager.GetButton("Camera FOV")?.OptionIndex ?? 0;
        if (mode > 0) freeCamera.fieldOfView = FovValues[mode];
    }
    [MenuDisable("Free Camera")]
    public static void StopFreeCamera()
    {
        StopFreeCameraLook();
        if (freeCamera != null) Object.Destroy(freeCamera.gameObject);
        freeCamera = null; cameraVelocity = Vector3.zero;
        freeLookOffset = Quaternion.identity;
    }

    private static void UpdateFreeCameraLook()
    {
        Mouse? mouse = Mouse.current;
        if (mouse == null || !mouse.rightButton.isPressed)
        {
            StopFreeCameraLook();
            return;
        }
        if (!freeCameraLooking)
        {
            freeCameraLooking = true;
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }
        if (freeCamera == null) return;
        Vector2 delta = mouse.delta.ReadValue();
        Vector3 angles = freeCamera.transform.eulerAngles;
        float pitch = angles.x > 180f ? angles.x - 360f : angles.x;
        float sensitivity = MovementMods.MouseSensitivity;
        pitch = Mathf.Clamp(pitch - delta.y * sensitivity, -80f, 80f);
        freeCamera.transform.rotation = Quaternion.Euler(pitch, angles.y + delta.x * sensitivity, 0f);
    }

    private static void StopFreeCameraLook()
    {
        if (!freeCameraLooking) return;
        freeCameraLooking = false;
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
    }

    [MenuButton("Camera FOV", ButtonType.Option, Category = "Fun", Order = 101, Options = new[] { "Default", "60", "75", "90", "110" }, Description = "Changes desktop camera FOV; Default restores the original")]
    public static void CameraFov()
    {
        PortTools.Register(Cleanup, MaintainFov);
        int mode = ButtonManager.GetButton("Camera FOV")?.OptionIndex ?? 0;
        if (mode == 0) { RestoreFov(); return; }
        MaintainFov();
    }

    private static void MaintainFov()
    {
        int mode = ButtonManager.GetButton("Camera FOV")?.OptionIndex ?? 0;
        if (mode == 0) return;
        FindDesktopCamera();
        if (desktopCamera == null) return;
        if (!fovSaved) { originalFov = desktopCamera.fieldOfView; fovSaved = true; }
        desktopCamera.fieldOfView = FovValues[mode];
        if (freeCamera != null) freeCamera.fieldOfView = FovValues[mode];
    }

    private static void FindDesktopCamera()
    {
        if (desktopCamera != null || Time.unscaledTime < nextCameraScan) return;
        nextCameraScan = Time.unscaledTime + 1f;
        if (GorillaTagger.Instance == null || GorillaTagger.Instance.thirdPersonCamera == null) return;
        GameObject cameraRoot = GorillaTagger.Instance.thirdPersonCamera;
        desktopCamera = cameraRoot.GetComponent<Camera>() ?? cameraRoot.GetComponentInChildren<Camera>(true);
        if (desktopCamera != null) fovSaved = false;
    }

    private static void RestoreFov()
    {
        if (desktopCamera != null && fovSaved) desktopCamera.fieldOfView = originalFov;
        if (freeCamera != null) freeCamera.fieldOfView = Camera.main != null ? Camera.main.fieldOfView : 60f;
        desktopCamera = null; fovSaved = false; nextCameraScan = 0f;
    }

    [MenuButton("Silent Hand Taps", ButtonType.Toggle, Category = "Fun", Order = 102, Description = "Mutes your hand tap volume while enabled")]
    public static void SilentHandTaps()
    {
        if (GorillaTagger.Instance == null) return;
        PortTools.Register(Cleanup);
        if (tapOwner != GorillaTagger.Instance)
        {
            StopSilentHandTaps();
            tapOwner = GorillaTagger.Instance;
            originalTapVolume = tapOwner.handTapVolume;
        }
        TapsMuted = true;
        tapOwner.handTapVolume = 0f;
    }
    [MenuDisable("Silent Hand Taps")]
    public static void StopSilentHandTaps()
    {
        TapsMuted = false;
        if (tapOwner != null) tapOwner.handTapVolume = originalTapVolume;
        tapOwner = null;
    }

    private static void Cleanup() { StopFreeCamera(); RestoreFov(); StopSilentHandTaps(); }
}

[HarmonyPatch(typeof(VRRig), "SetHandEffectData")]
internal static class ChudOSSilentTapPatch
{
    private static void Prefix(VRRig __instance, ref float handTapVolume)
    {
        if (NewFunMods.TapsMuted && __instance.isLocal) handTapVolume = 0f;
    }
}
