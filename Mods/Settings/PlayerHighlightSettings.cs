// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Settings;

public static class PlayerHighlightSettings
{
    private static readonly float[] Opacities = { 0.6f, 0.2f, 0.4f, 0.8f, 1f };
    private static readonly float[] Heights = { 10f, 5f, 20f, 40f };
    private static readonly float[] Widths = { 0.025f, 0.01f, 0.05f, 0.1f };
    internal static bool ChamsTheme, BeaconTheme;
    internal static float ChamsOpacity = 0.6f;
    internal static float BeaconHeight = 10f, BeaconWidth = 0.025f;

    private static int Index(string name) => ButtonManager.GetButton(name)?.OptionIndex ?? 0;

    [MenuButton("Chams Color", ButtonType.Option, Category = "Settings", Order = 233, Options = new[] { "Player", "Theme" }, Notify = false)]
    public static void SetChamsColor() => ChamsTheme = Index("Chams Color") == 1;
    [MenuButton("Chams Opacity", ButtonType.Option, Category = "Settings", Order = 234, Options = new[] { "60%", "20%", "40%", "80%", "100%" }, Notify = false)]
    public static void SetChamsOpacity() => ChamsOpacity = Opacities[Mathf.Clamp(Index("Chams Opacity"), 0, Opacities.Length - 1)];
    [MenuButton("Beacon Height", ButtonType.Option, Category = "Settings", Order = 235, Options = new[] { "10", "5", "20", "40" }, Notify = false)]
    public static void SetBeaconHeight() => BeaconHeight = Heights[Mathf.Clamp(Index("Beacon Height"), 0, Heights.Length - 1)];
    [MenuButton("Beacon Width", ButtonType.Option, Category = "Settings", Order = 236, Options = new[] { "0.025", "0.01", "0.05", "0.1" }, Notify = false)]
    public static void SetBeaconWidth() => BeaconWidth = Widths[Mathf.Clamp(Index("Beacon Width"), 0, Widths.Length - 1)];
    [MenuButton("Beacon Color", ButtonType.Option, Category = "Settings", Order = 237, Options = new[] { "Player", "Theme" }, Notify = false)]
    public static void SetBeaconColor() => BeaconTheme = Index("Beacon Color") == 1;
}
