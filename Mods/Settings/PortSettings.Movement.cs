// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;

namespace ChudOS.Mods.Settings;

public static partial class PortSettings
{
    internal static float PortalRadius = 0.55f, PortalCooldown = 0.35f;
    internal static float C4Power = 25f, C4Radius = 8f;
    internal static float WalkSpeed = 4f, SprintMultiplier = 2f, LookSensitivity = 0.12f;
    internal static float SlipStrength = 0.98f;
    internal static int PlayerCollisionParts;
    private static int Index(string name) => ButtonManager.GetButton(name)?.OptionIndex ?? 0;

    [MenuButton("Portal Size", ButtonType.Option, Category = "Settings", Order = 500, Options = new[] { "Normal", "Small", "Large", "Extra Large" }, Notify = false)]
    public static void SetPortalSize() => PortalRadius = new[] { 0.55f, 0.4f, 0.7f, 0.9f }[Index("Portal Size")];
    [MenuButton("Portal Cooldown", ButtonType.Option, Category = "Settings", Order = 501, Options = new[] { "0.35s", "0.2s", "0.5s", "1s" }, Notify = false)]
    public static void SetPortalCooldown() => PortalCooldown = new[] { 0.35f, 0.2f, 0.5f, 1f }[Index("Portal Cooldown")];
    [MenuButton("C4 Power", ButtonType.Option, Category = "Settings", Order = 502, Options = new[] { "25", "10", "15", "35", "45" }, Notify = false)]
    public static void SetC4Power() => C4Power = new[] { 25f, 10f, 15f, 35f, 45f }[Index("C4 Power")];
    [MenuButton("C4 Radius", ButtonType.Option, Category = "Settings", Order = 503, Options = new[] { "8m", "3m", "5m", "12m" }, Notify = false)]
    public static void SetC4Radius() => C4Radius = new[] { 8f, 3f, 5f, 12f }[Index("C4 Radius")];
    [MenuButton("PC Walking Speed", ButtonType.Option, Category = "Settings", Order = 504, Options = new[] { "4", "2", "6", "8" }, Notify = false)]
    public static void SetWalkSpeed() => WalkSpeed = new[] { 4f, 2f, 6f, 8f }[Index("PC Walking Speed")];
    [MenuButton("PC Sprint Multiplier", ButtonType.Option, Category = "Settings", Order = 505, Options = new[] { "2x", "1.5x", "2.5x", "3x" }, Notify = false)]
    public static void SetSprint() => SprintMultiplier = new[] { 2f, 1.5f, 2.5f, 3f }[Index("PC Sprint Multiplier")];
    [MenuButton("PC Look Sensitivity", ButtonType.Option, Category = "Settings", Order = 506, Options = new[] { "Normal", "Low", "High", "Very High" }, Notify = false)]
    public static void SetLookSensitivity() => LookSensitivity = new[] { 0.12f, 0.06f, 0.2f, 0.3f }[Index("PC Look Sensitivity")];
    [MenuButton("Solid Player Collision", ButtonType.Option, Category = "Settings", Order = 507, Options = new[] { "Body", "Hands", "Body And Hands" }, Notify = false)]
    public static void SetPlayerCollision() => PlayerCollisionParts = Index("Solid Player Collision");
    [MenuButton("Slippery Hands Strength", ButtonType.Option, Category = "Settings", Order = 508, Options = new[] { "Ice", "Light", "Strong" }, Notify = false)]
    public static void SetSlip() => SlipStrength = new[] { 0.98f, 0.8f, 0.995f }[Index("Slippery Hands Strength")];
}
