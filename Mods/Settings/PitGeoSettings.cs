// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Movement;
using ChudOS.Stuff;

namespace ChudOS.Mods.Settings;

public static class PitGeoSettings
{
    private static readonly float[] Multipliers = { 1f, 1.25f, 1.5f, 2f, 3f, 5f, 10f, 15f, 20f, 25f };
    private static readonly float[] Maximums = { 1f, 1.5f, 2f, 3f, 5f, 10f, 15f, 20f, 25f, 30f };
    public static int Walls { get; private set; }
    public static int Ground { get; private set; }
    public static float WallMultiplier { get; private set; } = 1f;
    public static float WallMaximum { get; private set; } = 1f;
    public static float GroundMultiplier { get; private set; } = 1f;
    public static float GroundMaximum { get; private set; } = 1f;
    private static int input, activation;
    private static int Index(string name, int count) => Math.Clamp(ButtonManager.GetButton(name)?.OptionIndex ?? 0, 0, count - 1);
    internal static bool Pressed => activation == 1 || input switch
    {
        0 => InputLib.RightGrab, 1 => InputLib.LeftGrab, 2 => InputLib.RightTrigger, 3 => InputLib.LeftTrigger,
        4 => InputLib.RightJoystickClick, 5 => InputLib.LeftJoystickClick, 6 => InputLib.RightControllerAButton,
        7 => InputLib.RightControllerBButton, 8 => InputLib.LeftControllerXButton, _ => InputLib.LeftControllerYButton
    };

    [MenuButton("PitGeo Walls", ButtonType.Option, Category = "Settings", Order = 200, Options = new[] { "None", "Upper", "Lower", "Both" }, Notify = false)]
    public static void SetWalls() { Walls = Index("PitGeo Walls", 4); PitGeo.Refresh(); }
    [MenuButton("PitGeo Ground", ButtonType.Option, Category = "Settings", Order = 201, Options = new[] { "None", "Top", "Bottom" }, Notify = false)]
    public static void SetGround() { Ground = Index("PitGeo Ground", 3); PitGeo.Refresh(); }
    [MenuButton("PitGeo Wall Multiplier", ButtonType.Option, Category = "Settings", Order = 202, Options = new[] { "1", "1.25", "1.5", "2", "3", "5", "10", "15", "20", "25" }, Notify = false)]
    public static void SetWallMultiplier() { WallMultiplier = Multipliers[Index("PitGeo Wall Multiplier", Multipliers.Length)]; PitGeo.Refresh(); }
    [MenuButton("PitGeo Wall Max Multiplier", ButtonType.Option, Category = "Settings", Order = 203, Options = new[] { "1", "1.5", "2", "3", "5", "10", "15", "20", "25", "30" }, Notify = false)]
    public static void SetWallMaximum() { WallMaximum = Maximums[Index("PitGeo Wall Max Multiplier", Maximums.Length)]; PitGeo.Refresh(); }
    [MenuButton("PitGeo Ground Multiplier", ButtonType.Option, Category = "Settings", Order = 204, Options = new[] { "1", "1.25", "1.5", "2", "3", "5", "10", "15", "20", "25" }, Notify = false)]
    public static void SetGroundMultiplier() { GroundMultiplier = Multipliers[Index("PitGeo Ground Multiplier", Multipliers.Length)]; PitGeo.Refresh(); }
    [MenuButton("PitGeo Ground Max Multiplier", ButtonType.Option, Category = "Settings", Order = 205, Options = new[] { "1", "1.5", "2", "3", "5", "10", "15", "20", "25", "30" }, Notify = false)]
    public static void SetGroundMaximum() { GroundMaximum = Maximums[Index("PitGeo Ground Max Multiplier", Maximums.Length)]; PitGeo.Refresh(); }
    [MenuButton("PitGeo Input", ButtonType.Option, Category = "Settings", Order = 206, Options = new[] { "Right Grip", "Left Grip", "Right Trigger", "Left Trigger", "Right Joystick Click", "Left Joystick Click", "A", "B", "X", "Y" }, Notify = false)]
    public static void SetInput() { input = Index("PitGeo Input", 10); PitGeo.Refresh(); }
    [MenuButton("PitGeo Activation", ButtonType.Option, Category = "Settings", Order = 207, Options = new[] { "Hold Input", "Always On" }, Notify = false)]
    public static void SetActivation() { activation = Index("PitGeo Activation", 2); PitGeo.Refresh(); }
    [MenuButton("Reset PitGeo Settings", ButtonType.Press, Category = "Settings", Order = 208)]
    public static void Reset()
    {
        string[] names = { "PitGeo Walls", "PitGeo Ground", "PitGeo Wall Multiplier", "PitGeo Wall Max Multiplier", "PitGeo Ground Multiplier", "PitGeo Ground Max Multiplier", "PitGeo Input", "PitGeo Activation" };
        foreach (string name in names)
        {
            var button = ButtonManager.GetButton(name);
            if (button == null) continue;
            button.OptionIndex = 0;
            button.Run();
        }
    }
}
