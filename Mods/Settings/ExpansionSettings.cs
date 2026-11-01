// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;

namespace ChudOS.Mods.Settings;

public static class ExpansionSettings
{
    internal static float MovementPower = 8f, MovementRange = 5f, PropMass = 0.2f, ToyPower = 10f, EmitterInterval = 0.25f, VisualRange = 60f, AudioVolume = 0.35f;
    internal static int GeometryLimit = 32;
    [MenuButton("Expansion Movement Power", ButtonType.Option, Category = "Settings", Order = 600, Options = new[] { "8", "4", "6", "12", "16" }, Notify = false)]
    public static void Power() => MovementPower = new[] { 8f, 4f, 6f, 12f, 16f }[Index("Expansion Movement Power")];
    [MenuButton("Expansion Movement Range", ButtonType.Option, Category = "Settings", Order = 601, Options = new[] { "5m", "2m", "8m", "12m" }, Notify = false)]
    public static void Range() => MovementRange = new[] { 5f, 2f, 8f, 12f }[Index("Expansion Movement Range")];
    [MenuButton("Expansion Object Limit", ButtonType.Option, Category = "Settings", Order = 602, Options = new[] { "32", "16", "48", "64" }, Notify = false)]
    public static void Limit() => GeometryLimit = new[] { 32, 16, 48, 64 }[Index("Expansion Object Limit")];
    [MenuButton("Sandbox Prop Mass", ButtonType.Option, Category = "Settings", Order = 603, Options = new[] { "0.2kg", "0.05kg", "0.5kg", "1kg" }, Notify = false)]
    public static void Mass() => PropMass = new[] { 0.2f, 0.05f, 0.5f, 1f }[Index("Sandbox Prop Mass")];
    [MenuButton("Toy Launch Power", ButtonType.Option, Category = "Settings", Order = 604, Options = new[] { "10", "4", "7", "14", "20" }, Notify = false)]
    public static void Toy() => ToyPower = new[] { 10f, 4f, 7f, 14f, 20f }[Index("Toy Launch Power")];
    [MenuButton("Local Emitter Interval", ButtonType.Option, Category = "Settings", Order = 605, Options = new[] { "0.25s", "0.5s", "1s" }, Notify = false)]
    public static void Rate() => EmitterInterval = new[] { 0.25f, 0.5f, 1f }[Index("Local Emitter Interval")];
    [MenuButton("Expansion Visual Range", ButtonType.Option, Category = "Settings", Order = 606, Options = new[] { "60m", "20m", "40m", "100m" }, Notify = false)]
    public static void Visual() => VisualRange = new[] { 60f, 20f, 40f, 100f }[Index("Expansion Visual Range")];
    [MenuButton("Practice Audio Volume", ButtonType.Option, Category = "Settings", Order = 607, Options = new[] { "35%", "0%", "15%", "60%" }, Notify = false)]
    public static void Audio() => AudioVolume = new[] { 0.35f, 0f, 0.15f, 0.6f }[Index("Practice Audio Volume")];
    private static int Index(string name) => ButtonManager.GetButton(name)!.OptionIndex;
}
