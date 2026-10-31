// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaTag.Gravity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Movement;

public static class PlanetZones
{
    private static readonly Dictionary<PlanetZone, bool> Original = new();
    private static readonly CreativeTools.Session Session = new();
    private static float nextScan;
    [MenuButton("Disable Planet Zones", ButtonType.Toggle, Category = "Movement", Order = 503, runOption = RunOption.Update, Description = "Disable supported planet zones that rotate the player or change gravity")]
    public static void Tick()
    {
        if (Session.Changed()) Stop();
        if (!CreativeTools.Ready) { Stop(); return; }
        if (Time.unscaledTime < nextScan) return;
        nextScan = Time.unscaledTime + 1f;
        PortTools.Register(Stop);
        foreach (PlanetZone zone in Object.FindObjectsByType<PlanetZone>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (zone == null) continue;
            if (!Original.ContainsKey(zone)) Original.Add(zone, zone.enabled);
            if (zone.enabled) zone.enabled = false;
        }
    }
    [MenuDisable("Disable Planet Zones")]
    public static void Stop()
    {
        foreach (var entry in Original) if (entry.Key != null) entry.Key.enabled = entry.Value;
        Original.Clear();
        nextScan = 0f;
    }
}
