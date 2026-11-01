// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;

namespace ChudOS.Mods.Player;

public static class PlayerExpansionButtons
{
    [MenuButton("Cosmetic Inspector", ButtonType.Toggle, Category = "Player", Order = 700, runOption = RunOption.Once, Controls = "(RG)(RT)", Description = "Select a player to inspect their locally loaded cosmetic objects.")]
    public static void EnableCosmeticInspector() => BetaFeature.Enable<ExpansionPlayer>("Cosmetic Inspector");
    [MenuDisable("Cosmetic Inspector")]
    public static void DisableCosmeticInspector() => BetaFeature.Disable("Cosmetic Inspector");

    [MenuButton("Player Audio Panel", ButtonType.Toggle, Category = "Player", Order = 701, runOption = RunOption.Once, Controls = "", Description = "Adjust individual loaded remote voice-source volumes.")]
    public static void EnablePlayerAudioPanel() => BetaFeature.Enable<ExpansionPlayer>("Player Audio Panel");
    [MenuDisable("Player Audio Panel")]
    public static void DisablePlayerAudioPanel() => BetaFeature.Disable("Player Audio Panel");

    [MenuButton("Outline ESP", ButtonType.Toggle, Category = "Player", Order = 702, runOption = RunOption.Once, Controls = "", Description = "Shows a camera-space silhouette contour sampled from avatar meshes.")]
    public static void EnableOutlineESP() => BetaFeature.Enable<ExpansionPlayer>("Outline ESP");
    [MenuDisable("Outline ESP")]
    public static void DisableOutlineESP() => BetaFeature.Disable("Outline ESP");

    [MenuButton("Voice Radar", ButtonType.Toggle, Category = "Player", Order = 703, runOption = RunOption.Once, Controls = "", Description = "Draws direction lines to players while they speak.")]
    public static void EnableVoiceRadar() => BetaFeature.Enable<ExpansionPlayer>("Voice Radar");
    [MenuDisable("Voice Radar")]
    public static void DisableVoiceRadar() => BetaFeature.Disable("Voice Radar");

    [MenuButton("Nearby Player Alert", ButtonType.Toggle, Category = "Player", Order = 704, runOption = RunOption.Once, Controls = "", Description = "Notifies when another player enters a four-metre radius.")]
    public static void EnableNearbyPlayerAlert() => BetaFeature.Enable<ExpansionPlayer>("Nearby Player Alert");
    [MenuDisable("Nearby Player Alert")]
    public static void DisableNearbyPlayerAlert() => BetaFeature.Disable("Nearby Player Alert");

    [MenuButton("FOV Cone Viewer", ButtonType.Toggle, Category = "Player", Order = 705, runOption = RunOption.Once, Controls = "", Description = "Shows other avatars head-forward view cones.")]
    public static void EnableFOVConeViewer() => BetaFeature.Enable<ExpansionPlayer>("FOV Cone Viewer");
    [MenuDisable("FOV Cone Viewer")]
    public static void DisableFOVConeViewer() => BetaFeature.Disable("FOV Cone Viewer");

}
