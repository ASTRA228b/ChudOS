// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;

namespace ChudOS.Mods.Movement;

public static class MacroWorkshopButtons
{
    [MenuButton("Macro Editor", ButtonType.Toggle, Category = "Macros", Order = 700, runOption = RunOption.Once, Controls = "", Description = "Inspect or edit the selected recording without replaying input or modifying the original file.")]
    public static void EnableMacroEditor() => BetaFeature.Enable<MacroWorkshop>("Macro Editor");
    [MenuDisable("Macro Editor")]
    public static void DisableMacroEditor() => BetaFeature.Disable("Macro Editor");

    [MenuButton("Macro Compare", ButtonType.Toggle, Category = "Macros", Order = 701, runOption = RunOption.Once, Controls = "", Description = "Inspect or edit the selected recording without replaying input or modifying the original file.")]
    public static void EnableMacroCompare() => BetaFeature.Enable<MacroWorkshop>("Macro Compare");
    [MenuDisable("Macro Compare")]
    public static void DisableMacroCompare() => BetaFeature.Disable("Macro Compare");

    [MenuButton("Route Deviation Coach", ButtonType.Toggle, Category = "Macros", Order = 702, runOption = RunOption.Once, Controls = "", Description = "Inspect or edit the selected recording without replaying input or modifying the original file.")]
    public static void EnableRouteDeviationCoach() => BetaFeature.Enable<MacroWorkshop>("Route Deviation Coach");
    [MenuDisable("Route Deviation Coach")]
    public static void DisableRouteDeviationCoach() => BetaFeature.Disable("Route Deviation Coach");

}
