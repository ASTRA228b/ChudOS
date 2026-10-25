// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChudOS.Mods.Movement;

public static class NoClipMods
{
    private static int input;
    private static Rigidbody? flyBody;
    private static bool originalGravity;

    [MenuButton("NoClip", ButtonType.Toggle, Category = "Movement", Order = 152, runOption = RunOption.FixedUpdate, Description = "Use the configured input to pass through surfaces; PC: hold N")]
    public static void NoClip()
    {
        if (!PortTools.Ready) { StopNoClip(); return; }
        PortTools.Register(Clear);
        bool pressed = input switch { 1 => InputLib.RightGrab, 2 => true, _ => InputLib.RightTrigger };
        if (Application.isFocused && Keyboard.current?.nKey.isPressed == true) pressed = true;
        NoClipState.Set(NoClipState.Owner.NoClip, pressed);
    }
    [MenuDisable("NoClip")]
    public static void StopNoClip() => NoClipState.Set(NoClipState.Owner.NoClip, false);

    [MenuButton("NoClip Fly", ButtonType.Toggle, Category = "Movement", Order = 153, runOption = RunOption.FixedUpdate, Description = "Hold X to fly through surfaces; PC: hold F")]
    public static void NoClipFly()
    {
        if (!PortTools.Ready || GTPlayer.Instance.headCollider == null) { StopFly(); return; }
        PortTools.Register(Clear);
        bool pressed = InputLib.LeftControllerXButton || Application.isFocused && Keyboard.current?.fKey.isPressed == true;
        if (!pressed) { StopFly(); return; }
        var body = GorillaTagger.Instance.rigidbody;
        if (flyBody != body)
        {
            RestoreFlyBody();
            flyBody = body;
            originalGravity = body.useGravity;
        }
        NoClipState.Set(NoClipState.Owner.Fly, true);
        body.useGravity = false;
        body.linearVelocity = GTPlayer.Instance.headCollider.transform.forward.normalized * MovementMods.FlySpeed;
    }
    [MenuDisable("NoClip Fly")]
    public static void StopFly() { RestoreFlyBody(); NoClipState.Set(NoClipState.Owner.Fly, false); }
    private static void RestoreFlyBody()
    {
        if (flyBody != null) { flyBody.useGravity = originalGravity; flyBody.linearVelocity = Vector3.zero; }
        flyBody = null;
    }

    [MenuButton("NoClip Input", ButtonType.Option, Category = "Settings", Order = 182, Options = new[] { "Right Trigger", "Right Grip", "Always On" }, Description = "Activation input for NoClip")]
    public static void SetInput() => input = Mathf.Clamp(ButtonManager.GetButton("NoClip Input")?.OptionIndex ?? 0, 0, 2);

    public static void Clear()
    {
        ButtonManager.SetEnabled("NoClip", false);
        ButtonManager.SetEnabled("NoClip Fly", false);
        RestoreFlyBody();
        NoClipState.Clear();
    }
}
