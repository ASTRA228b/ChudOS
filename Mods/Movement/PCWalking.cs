// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace ChudOS.Mods.Movement;

public static class PCWalking
{
    private static readonly CreativeTools.Session Session = new();
    private static bool looking, originalCursorVisible, jumpInput;
    private static CursorLockMode originalCursorLock;
    private static float yaw, pitch, jumpAfter;
    private static bool anglesReady;
    [MenuButton("PC Walking", ButtonType.Toggle, Category = "Movement", Order = 507, Controls = "(WASD)(Space)(Shift)(Ctrl)(RMB)", runOption = RunOption.FixedUpdate, Description = "Grounded desktop movement; Space jumps, Shift sprints, Ctrl slows movement, RMB looks")]
    public static void Tick()
    {
        if (Session.Changed()) StopState();
        if (XRSettings.isDeviceActive) { ButtonManager.SetEnabled("PC Walking", false); return; }
        if (!CreativeTools.Ready) { StopState(); return; }
        PortTools.Register(Stop, Look);
        Keyboard? keyboard = Keyboard.current;
        if (keyboard == null || PortBindings.DesktopBlocked) { jumpInput = false; return; }
        Transform head = CreativeTools.Head;
        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        float x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
        float z = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
        float speed = PortSettings.WalkSpeed * CreativeTools.Scale;
        if (keyboard.leftShiftKey.isPressed) speed *= PortSettings.SprintMultiplier;
        if (keyboard.leftCtrlKey.isPressed) speed *= 0.55f;
        Vector3 movement = Vector3.ClampMagnitude(right * x + forward * z, 1f) * speed;
        Vector3 velocity = CreativeTools.Body.linearVelocity;
        velocity.x = movement.x; velocity.z = movement.z;
        bool jump = CreativeTools.Pressed(keyboard.spaceKey.isPressed, ref jumpInput);
        if (CreativeTools.Ground(out _, 0.15f) && jump && Time.unscaledTime >= jumpAfter)
        {
            velocity.y = 5f * CreativeTools.Scale;
            jumpAfter = Time.unscaledTime + 0.25f;
        }
        CreativeTools.Body.linearVelocity = velocity;
    }
    [MenuEnable("PC Walking")]
    public static void Enable() { Stop(); PortBindings.Refresh(); PortTools.Register(Stop, Look); }
    private static void Look()
    {
        if (!CreativeTools.Ready || XRSettings.isDeviceActive || PortBindings.DesktopBlocked || Mouse.current?.rightButton.isPressed != true) { ReleaseCursor(); return; }
        if (!looking)
        {
            originalCursorVisible = Cursor.visible; originalCursorLock = Cursor.lockState;
            looking = true; anglesReady = false;
        }
        if (!anglesReady)
        {
            Vector3 angles = CreativeTools.Head.eulerAngles;
            yaw = angles.y; pitch = Mathf.DeltaAngle(0f, angles.x); anglesReady = true;
        }
        Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked;
        Vector2 delta = Mouse.current!.delta.ReadValue();
        yaw += delta.x * PortSettings.LookSensitivity;
        pitch = Mathf.Clamp(pitch - delta.y * PortSettings.LookSensitivity, -85f, 85f);
        CreativeTools.Head.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
    private static void ReleaseCursor()
    {
        if (!looking) return;
        Cursor.visible = originalCursorVisible; Cursor.lockState = originalCursorLock;
        looking = anglesReady = false;
    }
    private static void StopState() { ReleaseCursor(); jumpInput = false; jumpAfter = 0f; }
    [MenuDisable("PC Walking")]
    public static void Stop() { StopState(); PortTools.RemoveTick(Look); }
}
