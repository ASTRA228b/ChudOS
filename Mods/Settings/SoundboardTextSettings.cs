// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;

namespace ChudOS.Mods.Settings;

public static class SoundboardTextSettings
{
    private static readonly float[] Speeds = { 35f, 20f, 55f, 80f };
    public static bool Scrolling { get; private set; } = true;
    public static float Speed { get; private set; } = 35f;
    public static event Action? Changed;

    [MenuButton("Soundboard Text Scrolling", ButtonType.Toggle, Category = "Settings", Order = 180, DefaultEnabled = true, runOption = RunOption.Once, Notify = false, Description = "Scroll long titles and artists; OFF fits both as stationary text")]
    public static void Enable() => SetScrolling(true);
    [MenuDisable("Soundboard Text Scrolling")]
    public static void Disable() => SetScrolling(false);

    private static void SetScrolling(bool value)
    {
        if (Scrolling == value) return;
        Scrolling = value;
        Changed?.Invoke();
    }

    [MenuButton("Soundboard Scroll Speed", ButtonType.Option, Category = "Settings", Order = 181, Options = new[] { "Normal", "Slow", "Fast", "Very Fast" }, Notify = false, Description = "Change title and artist scrolling speed")]
    public static void SetSpeed()
    {
        int index = ButtonManager.GetButton("Soundboard Scroll Speed")?.OptionIndex ?? 0;
        Speed = Speeds[Math.Clamp(index, 0, Speeds.Length - 1)];
        Changed?.Invoke();
    }
}
