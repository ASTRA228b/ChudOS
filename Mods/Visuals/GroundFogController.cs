// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Reflection;
using ChudOS.Libraries;
using GorillaTag.Rendering;
using HarmonyLib;
using UnityEngine;

namespace ChudOS.Mods.Visuals;

internal static class GroundFogController
{
    private static readonly string[] FieldNames = { "groundFogColor", "_groundFogDepthFadeSize", "groundFogHeight", "_groundFogHeightFadeSize", "groundFogColor_overrideMode", "groundFogDepthFade_overrideMode", "groundFogHeight_overrideMode", "groundFogHeightFade_overrideMode" };
    private static readonly FieldInfo[] Fields = FieldNames.Select(name => AccessTools.Field(typeof(ZoneShaderSettings), name) ?? throw new MissingFieldException(typeof(ZoneShaderSettings).FullName, name)).ToArray();
    private static readonly AccessTools.FieldRef<ZoneShaderSettings, Color> FogColor = AccessTools.FieldRefAccess<ZoneShaderSettings, Color>("groundFogColor");
    private static readonly AccessTools.FieldRef<ZoneShaderSettings, float> Depth = AccessTools.FieldRefAccess<ZoneShaderSettings, float>("_groundFogDepthFadeSize");
    private static readonly AccessTools.FieldRef<ZoneShaderSettings, float> Height = AccessTools.FieldRefAccess<ZoneShaderSettings, float>("groundFogHeight");
    private static readonly AccessTools.FieldRef<ZoneShaderSettings, float> Fade = AccessTools.FieldRefAccess<ZoneShaderSettings, float>("_groundFogHeightFadeSize");
    private static readonly Action<ZoneShaderSettings, bool> Apply = AccessTools.MethodDelegate<Action<ZoneShaderSettings, bool>>(AccessTools.Method(typeof(ZoneShaderSettings), "BecomeActiveInstance"));
    private static readonly Dictionary<ZoneShaderSettings, object?[]> Saved = new();
    private static float nextCheck;

    internal static void Enable()
    {
        nextCheck = 0f;
        Tick();
        PortTools.Register(VisualMods.RestoreFog, Tick);
    }

    private static void Tick()
    {
        if (Time.unscaledTime < nextCheck) return;
        nextCheck = Time.unscaledTime + 0.25f;
        if (RenderSettings.fog) RenderSettings.fog = false;
        var zone = ZoneShaderSettings.activeInstance;
        if (zone == null) return;
        if (!Saved.ContainsKey(zone)) Saved.Add(zone, Fields.Select(field => field.GetValue(zone)).ToArray());
        if (FogColor(zone) != Color.clear || Depth(zone) != 0f || Height(zone) != 0f || Fade(zone) != 0f)
            zone.SetGroundFogValue(Color.clear, 0f, 0f, 0f);
    }

    internal static void Stop()
    {
        PortTools.RemoveTick(Tick);
        var active = ZoneShaderSettings.activeInstance;
        try
        {
            foreach (var pair in Saved)
            {
                if (pair.Key == null) continue;
                for (int i = 0; i < Fields.Length; i++) Fields[i].SetValue(pair.Key, pair.Value[i]);
            }
            if (active != null && Saved.ContainsKey(active)) Apply(active, true);
        }
        finally { Saved.Clear(); nextCheck = 0f; }
    }
}
