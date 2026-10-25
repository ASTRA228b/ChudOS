// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Libraries.Notifications;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Movement;

public static class PitGeo
{
    private const string Root = "Environment Objects/LocalObjects_Prefab/Forest/Terrain/pitgeo/";
    private static readonly string[] Paths = { Root + "pit upper slippery wall", Root + "pit lower slippery wall", Root + "pit ground top", Root + "pit ground bottom" };
    private static readonly GorillaSurfaceOverride?[] surfaces = new GorillaSurfaceOverride?[4];
    private static readonly float[] originalVelocity = new float[4], originalMaximum = new float[4];
    private static readonly bool[] captured = new bool[4];
    private static float nextSearch;
    private static bool warned;

    [MenuButton("PitGeo", ButtonType.Toggle, Category = "Movement", Order = 155, runOption = RunOption.FixedUpdate, Description = "Configure Forest pit walls, ground, multipliers and input in Settings")]
    public static void Tick()
    {
        if (!PortTools.Ready) { Stop(); return; }
        PortTools.Register(Stop);
        if (PitGeoSettings.Walls == 0 && PitGeoSettings.Ground == 0) { Restore(); return; }
        if (Time.unscaledTime >= nextSearch)
        {
            nextSearch = Time.unscaledTime + 1f;
            for (int i = 0; i < surfaces.Length; i++)
            {
                if (surfaces[i] != null) continue;
                captured[i] = false;
                var obj = GameObject.Find(Paths[i]);
                if (obj != null) surfaces[i] = obj.GetComponent<GorillaSurfaceOverride>();
            }
        }
        for (int i = 0; i < surfaces.Length; i++)
        {
            var surface = surfaces[i];
            bool selected = i < 2 ? PitGeoSettings.Walls == 3 || PitGeoSettings.Walls == i + 1 : PitGeoSettings.Ground == i - 1;
            if (!selected || !PitGeoSettings.Pressed) { Restore(i); continue; }
            if (surface == null)
            {
                if (!warned) { warned = true; NotifyLib.SendNotificationALL("PitGeo: selected Forest pit surfaces are not loaded.", 4000); }
                continue;
            }
            if (!captured[i])
            {
                originalVelocity[i] = surface.extraVelMultiplier;
                originalMaximum[i] = surface.extraVelMaxMultiplier;
                captured[i] = true;
            }
            surface.extraVelMultiplier = i < 2 ? PitGeoSettings.WallMultiplier : PitGeoSettings.GroundMultiplier;
            surface.extraVelMaxMultiplier = i < 2 ? PitGeoSettings.WallMaximum : PitGeoSettings.GroundMaximum;
        }
    }
    [MenuEnable("PitGeo")]
    public static void Enable()
    {
        PortTools.Register(Stop);
        if (PitGeoSettings.Walls == 0 && PitGeoSettings.Ground == 0) NotifyLib.SendNotificationALL("Choose PitGeo Walls or PitGeo Ground in Settings.", 4000);
    }
    public static void Refresh()
    {
        Restore();
        if (ButtonManager.GetButton("PitGeo")?.Enabled == true) Tick();
    }
    private static void Restore(int index)
    {
        if (!captured[index]) return;
        var surface = surfaces[index];
        if (surface != null) { surface.extraVelMultiplier = originalVelocity[index]; surface.extraVelMaxMultiplier = originalMaximum[index]; }
        captured[index] = false;
    }
    private static void Restore() { for (int i = 0; i < surfaces.Length; i++) Restore(i); }
    [MenuDisable("PitGeo")]
    public static void Stop()
    {
        Restore();
        Array.Clear(surfaces, 0, surfaces.Length);
        nextSearch = 0f;
        warned = false;
    }
}
