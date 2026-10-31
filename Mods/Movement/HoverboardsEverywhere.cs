// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Core.Managers;
using ChudOS.Stuff;
using GorillaLocomotion;
using HarmonyLib;
using System.Reflection;

namespace ChudOS.Mods.Movement;

public static class HoverboardsEverywhere
{
    private static GTPlayer? owner;
    private static bool original;
    private static Harmony? patch;
    private static readonly AccessTools.FieldRef<GTPlayer, bool> Allowed = AccessTools.FieldRefAccess<GTPlayer, bool>("<isHoverAllowed>k__BackingField");
    private static readonly MethodInfo RefreshAllowed = AccessTools.Method(typeof(GTPlayer), "RefreshHoverAllowed") ?? throw new MissingMethodException("GTPlayer.RefreshHoverAllowed");
    [MenuButton("Hoverboards Everywhere", ButtonType.Toggle, Category = "Movement", Order = 502, runOption = RunOption.Update, Description = "Enable native hoverboard mode outside normal hoverboard areas")]
    public static void Tick()
    {
        if (!PortTools.Ready) { Stop(); return; }
        GTPlayer player = GTPlayer.Instance;
        if (owner != player)
        {
            Stop();
            try
            {
                owner = player;
                original = player.enableHoverMode;
                PortTools.Register(Stop);
                patch = new Harmony("astra.chudos.hoverboards-everywhere");
                patch.Patch(RefreshAllowed, prefix: new HarmonyMethod(typeof(HoverboardsEverywhere), nameof(RefreshPrefix)));
            }
            catch
            {
                Stop();
                ButtonManager.SetEnabled("Hoverboards Everywhere", false);
                throw;
            }
        }
        Allowed(player) = true;
        if (!player.enableHoverMode) player.SetHoverActive(true);
    }
    private static bool RefreshPrefix(GTPlayer __instance)
    {
        if (__instance != owner) return true;
        Allowed(__instance) = true;
        return false;
    }
    [MenuDisable("Hoverboards Everywhere")]
    public static void Stop()
    {
        if (patch != null) patch.Unpatch(RefreshAllowed, HarmonyPatchType.Prefix, patch.Id);
        patch = null;
        GTPlayer? previous = owner;
        owner = null;
        if (previous == null) return;
        try { if (previous.enableHoverMode != original) previous.SetHoverActive(original); }
        finally { RefreshAllowed.Invoke(previous, null); }
    }
}
