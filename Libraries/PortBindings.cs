// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using UnityEngine;
using UnityEngine.XR;

namespace ChudOS.Libraries;

internal static class PortBindings
{
    internal static bool DesktopBlocked => !XRSettings.isDeviceActive && (!Application.isFocused || ChudOS.Core.Main.IsMenuOpen);
    internal static void Refresh()
    {
        bool desktop = !XRSettings.isDeviceActive;
        bool left = ButtonManager.GetButton("Gun Hand")?.OptionIndex == 1;
        string gun = desktop ? "(RMB)(LMB)" : left ? "(LG)(LT)" : "(RG)(RT)";
        Set("Portal Gun", gun + (desktop ? "(Q)" : "(A)"));
        Set("C4", desktop ? "(G)(Space)" : "(RG)(A)");
        Set("PC Walking", "(WASD)(Space)(Shift)(Ctrl)(RMB)");
    }
    private static void Set(string name, string controls)
    {
        var button = ButtonManager.GetButton(name);
        if (button != null) button.Attribute.Controls = controls;
    }
}
