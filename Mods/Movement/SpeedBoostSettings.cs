// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;
using GorillaLocomotion;

namespace ChudOS.Mods.Movement;

public static class SpeedBoostSettings
{
    internal static float JumpSpeed = 8.5f, Multiplier = 1.5f;
    private static bool applyingPreset;

    [MenuButton("Speed Boost Preset", ButtonType.Option, Category = "Settings", Order = 169, Options = new[] { "Boost", "Normal", "Strong", "Custom" }, Notify = false)]
    public static void SetPreset()
    {
        int preset = ButtonManager.GetButton("Speed Boost Preset")?.OptionIndex ?? 0;
        if (preset == 3 || applyingPreset) return;
        applyingPreset = true;
        try
        {
            SetOption("Speed Boost Jump Speed", preset == 1 ? 1 : preset == 2 ? 2 : 0);
            SetOption("Speed Boost Multiplier", preset == 1 ? 1 : preset == 2 ? 6 : 0);
        }
        finally { applyingPreset = false; }
    }

    private static void SetOption(string name, int index)
    {
        var button = ButtonManager.GetButton(name);
        if (button == null) return;
        if (button.OptionIndex != index) ButtonManager.ChangeOption(button, index - button.OptionIndex);
        else button.Run();
    }

    private static void RefreshPreset()
    {
        if (applyingPreset) return;
        int preset = JumpSpeed == 8.5f && Multiplier == 1.5f ? 0 : JumpSpeed == 6.5f && Multiplier == 1.1f ? 1 : JumpSpeed == 10f && Multiplier == 1.8f ? 2 : 3;
        applyingPreset = true;
        try { SetOption("Speed Boost Preset", preset); }
        finally { applyingPreset = false; }
    }

    [MenuButton("Speed Boost Jump Speed", ButtonType.Option, Category = "Settings", Order = 170, Options = new[] { "8.5", "6.5", "10", "12", "15", "20" }, Notify = false)]
    public static void SetSpeed() { JumpSpeed = new[] { 8.5f, 6.5f, 10f, 12f, 15f, 20f }[ButtonManager.GetButton("Speed Boost Jump Speed")?.OptionIndex ?? 0]; RefreshPreset(); }
    [MenuButton("Speed Boost Multiplier", ButtonType.Option, Category = "Settings", Order = 171, Options = new[] { "1.5", "1.1", "1.25", "1.75", "2", "2.2", "1.8" }, Notify = false)]
    public static void SetMultiplier() { Multiplier = new[] { 1.5f, 1.1f, 1.25f, 1.75f, 2f, 2.2f, 1.8f }[ButtonManager.GetButton("Speed Boost Multiplier")?.OptionIndex ?? 0]; RefreshPreset(); }

    [MenuButton("Mosa Speed Boost", ButtonType.Toggle, Category = "Movement", Order = 1, runOption = RunOption.FixedUpdate, Description = "Sets maxJumpSpeed to 999 and jumpMultiplier to 2.2")]
    public static void Mosa()
    {
        var player = GTPlayer.Instance;
        if (player == null) return;
        player.maxJumpSpeed = 999f;
        player.jumpMultiplier = 2.2f;
    }
    [MenuEnable("Mosa Speed Boost")]
    public static void StartMosa() => ButtonManager.SetEnabled("Speed Boost", false);
    [MenuEnable("Speed Boost")]
    public static void StartNormal() => ButtonManager.SetEnabled("Mosa Speed Boost", false);
    [MenuDisable("Mosa Speed Boost")]
    public static void StopMosa()
    {
        var player = GTPlayer.Instance;
        if (player == null) return;
        bool boost = ButtonManager.GetButton("Speed Boost")?.Enabled == true;
        player.maxJumpSpeed = boost ? JumpSpeed : 6.5f;
        player.jumpMultiplier = boost ? Multiplier : 1.1f;
    }
}
