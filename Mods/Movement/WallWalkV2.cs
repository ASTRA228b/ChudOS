// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;

namespace ChudOS.Mods.Movement;

public static class WallWalkV2
{
    private static float strength = 15f;
    private static int input;

    internal static void Tick()
    {
        if (!PortTools.Ready || !MovementInput.Both(input)) return;
        var body = GTPlayer.Instance.bodyCollider;
        if (body == null || body.attachedRigidbody == null) return;
        body.attachedRigidbody.AddForce(body.transform.forward * strength, ForceMode.Acceleration);
    }

    [MenuButton("Wall Walk Strength", ButtonType.Option, Category = "Settings", Order = 100, Options = new[] { "15", "55", "1", "5", "10", "25", "40", "75", "100", "0" }, Notify = false)]
    public static void SetStrength() => strength = new[] { 15f, 55f, 1f, 5f, 10f, 25f, 40f, 75f, 100f, 0f }[ButtonManager.GetButton("Wall Walk Strength")?.OptionIndex ?? 0];

    [MenuButton("Wall Walk Input", ButtonType.Option, Category = "Settings", Order = 101, Options = new[] { "Both Triggers", "Both Grips", "Both Joystick Clicks", "A + X", "B + Y" }, Notify = false)]
    public static void SetInput() => input = ButtonManager.GetButton("Wall Walk Input")?.OptionIndex ?? 0;
}

internal static class MovementInput
{
    internal static bool Left(int input) => input switch
    {
        0 => InputLib.LeftGrab,
        1 => InputLib.LeftTrigger,
        2 => InputLib.LeftJoystickClick,
        5 => InputLib.LeftControllerXButton,
        6 => InputLib.LeftControllerYButton,
        _ => false
    };

    internal static bool Right(int input) => input switch
    {
        0 => InputLib.RightGrab,
        1 => InputLib.RightTrigger,
        2 => InputLib.RightJoystickClick,
        3 => InputLib.RightControllerAButton,
        4 => InputLib.RightControllerBButton,
        _ => false
    };

    internal static bool Both(int input) => input switch
    {
        0 => Left(1) && Right(1),
        1 => Left(0) && Right(0),
        2 => Left(2) && Right(2),
        3 => Left(5) && Right(3),
        4 => Left(6) && Right(4),
        _ => false
    };
}
