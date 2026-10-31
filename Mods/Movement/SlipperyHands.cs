// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Reflection;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using GorillaLocomotion;
using HarmonyLib;
using UnityEngine;

namespace ChudOS.Mods.Movement;

public static class SlipperyHands
{
    private static Harmony? patch;
    private static MethodInfo? target;
    private static bool active;
    [MenuButton("Slippery Hands", ButtonType.Toggle, Category = "Movement", Order = 504, runOption = RunOption.Once, Description = "Make contacted surfaces slippery using the selected slip strength")]
    public static void Enable()
    {
        Stop();
        try
        {
            target = AccessTools.Method(typeof(GTPlayer), nameof(GTPlayer.GetSlidePercentage), new[] { typeof(RaycastHit) }) ?? throw new MissingMethodException("GTPlayer.GetSlidePercentage");
            patch = new Harmony("astra.chudos.slipperyhands");
            patch.Patch(target, prefix: new HarmonyMethod(typeof(SlipperyHands), nameof(Prefix)));
            active = true;
            PortTools.Register(Stop);
        }
        catch
        {
            Stop();
            ButtonManager.SetEnabled("Slippery Hands", false);
            throw;
        }
    }
    private static bool Prefix(GTPlayer __instance, ref float __result)
    {
        if (!active || __instance != GTPlayer.Instance) return true;
        __result = PortSettings.SlipStrength;
        return false;
    }
    [MenuDisable("Slippery Hands")]
    public static void Stop()
    {
        active = false;
        if (patch != null && target != null) patch.Unpatch(target, HarmonyPatchType.Prefix, patch.Id);
        patch = null;
        target = null;
    }
}
