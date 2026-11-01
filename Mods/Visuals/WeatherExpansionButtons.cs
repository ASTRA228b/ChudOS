// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;

namespace ChudOS.Mods.Visuals;

public static class WeatherExpansionButtons
{
    [MenuButton("Snow Control", ButtonType.Toggle, Category = "Weather", Order = 700, runOption = RunOption.Once, Controls = "", Description = "Temporarily disables loaded ambient snowfall particle emission.")]
    public static void EnableSnowControl() => BetaFeature.Enable<ExpansionVisuals>("Snow Control");
    [MenuDisable("Snow Control")]
    public static void DisableSnowControl() => BetaFeature.Disable("Snow Control");

}
