// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;

namespace ChudOS.Mods.Safety;

public static class HealthExpansionButtons
{
    [MenuButton("Mod Health Monitor", ButtonType.Toggle, Category = "Safety", Order = 700, runOption = RunOption.Once, Controls = "", Description = "Displays runtime prerequisites and a bounded list of recent ChudOS errors.")]
    public static void EnableModHealthMonitor() => BetaFeature.Enable<ModHealthMonitor>("Mod Health Monitor");
    [MenuDisable("Mod Health Monitor")]
    public static void DisableModHealthMonitor() => BetaFeature.Disable("Mod Health Monitor");

}
