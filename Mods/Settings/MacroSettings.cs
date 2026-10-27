// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;

namespace ChudOS.Mods.Settings;

public static class MacroSettings
{
    internal static bool Legacy, Loop;
    internal static float Range = 1f, Speed = 1f, Height;
    internal static int Position;
    private static int Index(string name) => ButtonManager.GetButton(name)?.OptionIndex ?? 0;

    [MenuButton("Legacy Macro Playback", ButtonType.Toggle, Category = "Settings", Order = 250, runOption = RunOption.Once, Description = "Use the previous macro timing and positioning, including its original rig restoration behavior")]
    public static void LegacyOn() { Legacy = true; MacroManager.SettingsChanged(); }
    [MenuDisable("Legacy Macro Playback")]
    public static void LegacyOff() { Legacy = false; MacroManager.SettingsChanged(); }
    [MenuButton("Macro Playback Range", ButtonType.Option, Category = "Settings", Order = 251, Options = new[] { "Normal (1m)", "Small (0.5m)", "Large (2m)", "Extra Large (3m)", "Extra Small (0.25m)" }, Notify = false)]
    public static void SetRange() { Range = new[] { 1f, 0.5f, 2f, 3f, 0.25f }[Index("Macro Playback Range")]; MacroManager.SettingsChanged(); }
    [MenuButton("Loop Macros", ButtonType.Toggle, Category = "Settings", Order = 252, runOption = RunOption.Once)]
    public static void LoopOn() { Loop = true; MacroManager.SettingsChanged(); }
    [MenuDisable("Loop Macros")]
    public static void LoopOff() { Loop = false; MacroManager.SettingsChanged(); }
    [MenuButton("Macro Playback Speed", ButtonType.Option, Category = "Settings", Order = 253, Options = new[] { "1x", "0.5x", "0.75x", "1.25x", "1.5x", "2x" }, Notify = false)]
    public static void SetSpeed() { Speed = new[] { 1f, 0.5f, 0.75f, 1.25f, 1.5f, 2f }[Index("Macro Playback Speed")]; MacroManager.SettingsChanged(); }
    [MenuButton("Macro Position", ButtonType.Option, Category = "Settings", Order = 254, Options = new[] { "Recorded", "Relative" }, Notify = false, Description = "Recorded follows the original world route; Relative translates the entire route to where playback starts")]
    public static void SetPosition() { Position = Index("Macro Position"); MacroManager.SettingsChanged(); }
    [MenuButton("Macro Height Offset", ButtonType.Option, Category = "Settings", Order = 255, Options = new[] { "0m", "+0.05m", "+0.1m", "+0.25m", "-0.05m", "-0.1m", "-0.25m" }, Notify = false)]
    public static void SetHeight() { Height = new[] { 0f, 0.05f, 0.1f, 0.25f, -0.05f, -0.1f, -0.25f }[Index("Macro Height Offset")]; MacroManager.SettingsChanged(); }
}
