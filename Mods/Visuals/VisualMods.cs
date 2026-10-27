// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra
//
// This file is part of ChudOS.
// ChudOS is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 3,
// as published by the Free Software Foundation.
//
// ChudOS is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ChudOS. If not, see <https://www.gnu.org/licenses/>.

using ChudOS.Core.Managers;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChudOS.Mods.Visuals;

public static class VisualMods
{
    private static bool SavedFog, OldFog;
    private static bool SavedAmbient;
    private static Color OldAmbient;
    private static AmbientMode OldAmbientMode;

    [MenuButton("Hide Fog", ButtonType.Toggle, Category = "Visuals", Order = 0, runOption = RunOption.Once, Description = "Disable scene fog and zone ground fog, including Forest Halloween fog")]
    public static void HideFog()
    {
        if (!SavedFog) { OldFog = RenderSettings.fog; SavedFog = true; }
        try { GroundFogController.Enable(); RenderSettings.fog = false; }
        catch { RestoreFog(); throw; }
    }

    [MenuDisable("Hide Fog")]
    public static void RestoreFog()
    {
        try { GroundFogController.Stop(); }
        finally
        {
            if (SavedFog) RenderSettings.fog = OldFog;
            SavedFog = false;
        }
    }


    [MenuButton("Ambient Tint", ButtonType.Option, Category = "Visuals", Order = 1, Options = new[] { "Default", "Purple", "Warm" }, Description = "Change ambient scene lighting")]
    public static void AmbientTint()
    {
        int mode = ButtonManager.GetButton("Ambient Tint")!.OptionIndex;
        if (!SavedAmbient)
        {
            OldAmbient = RenderSettings.ambientLight;
            OldAmbientMode = RenderSettings.ambientMode;
            SavedAmbient = true;
        }
        if (mode == 0) RestoreAmbient();
        else
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = mode == 1 ? new Color(0.36f, 0.17f, 0.46f) : new Color(0.72f, 0.52f, 0.32f);
        }
    }

    private static void RestoreAmbient()
    {
        if (!SavedAmbient) return;
        RenderSettings.ambientMode = OldAmbientMode;
        RenderSettings.ambientLight = OldAmbient;
        SavedAmbient = false;
    }

    [MenuButton("Visual Info", ButtonType.Press, Category = "Visuals", Order = 2, Notify = false, Description = "Show current scene fog and ambient settings")]
    public static void VisualInfo() => NotifyLib.MessageToast($"Fog: {(RenderSettings.fog ? "ON" : "OFF")} | Ambient: {RenderSettings.ambientMode}", 3500);
}
