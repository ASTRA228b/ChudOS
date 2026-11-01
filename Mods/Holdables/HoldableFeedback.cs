// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Mods.Settings;
using ChudOS.Core.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace ChudOS.Mods.Holdables;

internal static class HoldableFeedback
{
    internal static bool AllowInput
    {
        get
        {
            if (ChudOS.Core.Main.IsMenuOpen || ChudOS.Core.AGUI.JarvisGUI.Editing) return false;
            var mouse = Mouse.current;
            if (mouse == null) return true;
            Vector2 point = mouse.position.ReadValue();
            return !GUIManager.ContainsPointer(new Vector2(point.x, Screen.height - point.y));
        }
    }
    internal static void Pulse(bool left)
    {
        if (ExpansionPreferences.Vibrations && XRSettings.isDeviceActive && GorillaTagger.Instance != null) GorillaTagger.Instance.StartVibration(left, 0.15f, 0.05f);
    }
}
