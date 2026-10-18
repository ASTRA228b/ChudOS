using HarmonyLib;
using UnityEngine;

namespace ChudOS.Mods.Rig;

internal sealed class SpinBot : RigBehaviour
{
    public static bool IsEnabled;

    protected override void OnEnable() => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public static class SpinBotPatch
{
    private static float yRot;

    private static void Postfix(VRRig __instance)
    {
        if (!SpinBot.IsEnabled || !__instance.IsLocalRig())
            return;

        yRot = (yRot + 1350 * Time.deltaTime) % 360f;

        __instance.transform.rotation = Quaternion.Euler(0, yRot, 0);
    }
}