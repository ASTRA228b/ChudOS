// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Collections.Generic;
using System.Reflection;
using ChudOS.Libraries;
using HarmonyLib;

namespace ChudOS.Mods.Safety;

internal static class NoFingerPose
{
    internal static bool Enabled;

    internal static void Enable()
    {
        Enabled = true;
        PortTools.Register(Disable);
    }

    internal static void Disable() => Enabled = false;

    internal static bool IsLocal(VRMap map)
    {
        VRRig rig = VRRig.LocalRig;
        return Enabled && rig != null &&
            (ReferenceEquals(map, rig.leftHand) || ReferenceEquals(map, rig.rightHand) ||
             ReferenceEquals(map, rig.leftIndex) || ReferenceEquals(map, rig.leftMiddle) ||
             ReferenceEquals(map, rig.leftThumb) || ReferenceEquals(map, rig.rightIndex) ||
             ReferenceEquals(map, rig.rightMiddle) || ReferenceEquals(map, rig.rightThumb));
    }

    private static IEnumerable<MethodBase> Methods(string name)
    {
        yield return AccessTools.DeclaredMethod(typeof(VRMap), name);
        yield return AccessTools.DeclaredMethod(typeof(VRMapThumb), name);
        yield return AccessTools.DeclaredMethod(typeof(VRMapIndex), name);
        yield return AccessTools.DeclaredMethod(typeof(VRMapMiddle), name);
    }

    [HarmonyPatch]
    private static class MapPatch
    {
        private static IEnumerable<MethodBase> TargetMethods() => Methods("MapMyFinger");

        [HarmonyPriority(Priority.First)]
        private static bool Prefix(VRMap __instance)
        {
            if (!IsLocal(__instance)) return true;
            __instance.calcT = 0f;
            __instance.LerpFinger(0f, false);
            return false;
        }
    }

    [HarmonyPatch]
    private static class LerpPatch
    {
        private static IEnumerable<MethodBase> TargetMethods() => Methods("LerpFinger");

        [HarmonyPriority(Priority.Last)]
        private static void Prefix(VRMap __instance, ref float __0)
        {
            if (!IsLocal(__instance)) return;
            __0 = 0f;
            __instance.calcT = 0f;
        }
    }
}
