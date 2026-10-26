// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;

namespace ChudOS.Mods.Settings;

public static class ExtraMovementSettings
{
    private static readonly float[] Speeds = { 8f, 2f, 4f, 6f, 10f, 15f, 20f };
    private static readonly float[] Responses = { 5f, 2f, 8f, 12f };
    private static readonly float[] Accelerations = { 25f, 10f, 15f, 40f, 60f };
    private static readonly float[] Powers = { 1.5f, 0.5f, 1f, 2f, 3f };
    private static readonly float[] Jumps = { 6f, 3f, 4f, 5f, 8f, 10f };
    private static readonly float[] Punches = { 10f, 2f, 5f, 15f, 20f };

    internal static float DroneSpeed = 8f, DroneResponse = 5f;
    internal static float VelocitySpeed = 8f, VelocityAcceleration = 25f;
    internal static float ThrowPower = 1.5f, JumpPower = 6f, PunchPower = 10f;
    internal static int VelocityInput, VelocityDirection, VelocityRelease, JumpInput;

    private static int Index(string name) => ButtonManager.GetButton(name)?.OptionIndex ?? 0;

    [MenuButton("Drone Speed", ButtonType.Option, Category = "Settings", Order = 220, Options = new[] { "8", "2", "4", "6", "10", "15", "20" }, Notify = false)]
    public static void SetDroneSpeed() => DroneSpeed = Speeds[Index("Drone Speed")];
    [MenuButton("Drone Response", ButtonType.Option, Category = "Settings", Order = 221, Options = new[] { "5", "2", "8", "12" }, Notify = false)]
    public static void SetDroneResponse() => DroneResponse = Responses[Index("Drone Response")];
    [MenuButton("Velocity Speed", ButtonType.Option, Category = "Settings", Order = 222, Options = new[] { "8", "2", "4", "6", "10", "15", "20" }, Notify = false)]
    public static void SetVelocitySpeed() => VelocitySpeed = Speeds[Index("Velocity Speed")];
    [MenuButton("Velocity Acceleration", ButtonType.Option, Category = "Settings", Order = 223, Options = new[] { "25", "10", "15", "40", "60" }, Notify = false)]
    public static void SetVelocityAcceleration() => VelocityAcceleration = Accelerations[Index("Velocity Acceleration")];
    [MenuButton("Velocity Input", ButtonType.Option, Category = "Settings", Order = 224, Options = new[] { "Right Trigger", "Left Trigger", "Right Grip", "Left Grip", "A", "B", "X", "Y", "Right Joystick Click", "Left Joystick Click" }, Notify = false)]
    public static void SetVelocityInput() => VelocityInput = Index("Velocity Input");
    [MenuButton("Velocity Direction", ButtonType.Option, Category = "Settings", Order = 225, Options = new[] { "Hand", "Head", "Joystick" }, Notify = false)]
    public static void SetVelocityDirection() => VelocityDirection = Index("Velocity Direction");
    [MenuButton("Velocity Release", ButtonType.Option, Category = "Settings", Order = 226, Options = new[] { "Stop", "Keep Momentum" }, Notify = false)]
    public static void SetVelocityRelease() => VelocityRelease = Index("Velocity Release");
    [MenuButton("Throw Power", ButtonType.Option, Category = "Settings", Order = 227, Options = new[] { "1.5", "0.5", "1", "2", "3" }, Notify = false)]
    public static void SetThrowPower() => ThrowPower = Powers[Index("Throw Power")];
    [MenuButton("Double Jump Power", ButtonType.Option, Category = "Settings", Order = 228, Options = new[] { "6", "3", "4", "5", "8", "10" }, Notify = false)]
    public static void SetJumpPower() => JumpPower = Jumps[Index("Double Jump Power")];
    [MenuButton("Double Jump Input", ButtonType.Option, Category = "Settings", Order = 229, Options = new[] { "A", "B", "X", "Y" }, Notify = false)]
    public static void SetJumpInput() => JumpInput = Index("Double Jump Input");
    [MenuButton("Punch Power", ButtonType.Option, Category = "Settings", Order = 230, Options = new[] { "10", "2", "5", "15", "20" }, Notify = false)]
    public static void SetPunchPower() => PunchPower = Punches[Index("Punch Power")];
}
