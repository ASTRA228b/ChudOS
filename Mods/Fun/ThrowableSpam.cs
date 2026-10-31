// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using GorillaNetworking;
using UnityEngine;

namespace ChudOS.Mods.Fun;

public static class ThrowableSpam
{
    private static SnowballThrowable? selected, owned;
    private static SnowballMaker? maker;
    private static string kind = "", selectedId = "", resolutionMessage = "Throwable data is not ready yet.";
    private static float nextThrow, nextResolve, nextNotice, interval = 0.25f;

    [MenuButton("Snowball Spam", ButtonType.Toggle, Category = "Fun", Order = 160, Description = "Hold right trigger to repeatedly throw available snowballs using normal hand velocity")]
    public static void Snowballs() => Tick("Snowball");
    [MenuEnable("Snowball Spam")]
    public static void StartSnowballs() { ButtonManager.SetEnabled("Water Balloon Spam", false); Reset(); }
    [MenuDisable("Snowball Spam")]
    public static void StopSnowballs() => Reset();
    [MenuButton("Water Balloon Spam", ButtonType.Toggle, Category = "Fun", Order = 161, Description = "Hold right trigger to throw available water balloons using normal hand velocity")]
    public static void Balloons() => Tick("Water Balloon");
    [MenuEnable("Water Balloon Spam")]
    public static void StartBalloons() { ButtonManager.SetEnabled("Snowball Spam", false); Reset(); }
    [MenuDisable("Water Balloon Spam")]
    public static void StopBalloons() => Reset();
    [MenuButton("Throwable Interval", ButtonType.Option, Category = "Settings", Order = 247, Options = new[] { "0.25s", "0.5s", "1s" }, Notify = false)]
    public static void SetInterval() => interval = new[] { 0.25f, 0.5f, 1f }[ButtonManager.GetButton("Throwable Interval")!.OptionIndex];

    private static void Tick(string name)
    {
        if (!PortTools.Ready || VRRig.LocalRig == null || !InputLib.RightTrigger) { nextThrow = 0f; return; }
        if (Time.unscaledTime < nextThrow) return;
        if (maker != SnowballMaker.rightHandInstance || kind != name) { Reset(); kind = name; maker = SnowballMaker.rightHandInstance; }
        if (selected == null && Time.unscaledTime >= nextResolve) Resolve(name);
        nextThrow = Time.unscaledTime + interval;
        if (selected == null || maker == null || selected.matDataIndexes == null || selected.matDataIndexes.Count == 0)
        {
            if (Time.unscaledTime >= nextNotice)
            {
                nextNotice = Time.unscaledTime + 10f;
                NotifyLib.SendNotificationALL(resolutionMessage, 4000);
            }
            return;
        }
        if (maker.velocityEstimator == null || !selected.IsMine())
        {
            selected = null;
            selectedId = "";
            resolutionMessage = "Waiting for local hand tracking and throwable ownership.";
            return;
        }
        var equipment = EquipmentInteractor.instance;
        if (equipment == null || equipment.rightHandHeldEquipment != null) return;
        if (!TryGetMaterial(selected, out int material))
        {
            selected = null;
            selectedId = "";
            resolutionMessage = "The throwable material mapping is not ready yet.";
            return;
        }
        if (maker.TryCreateSnowball(material, out var throwable) && throwable != null)
        {
            owned = throwable;
            bool released = false;
            try
            {
                if (throwable.throwableMakerIndex == selected.throwableMakerIndex)
                    released = throwable.OnRelease(null!, equipment.rightHand);
            }
            finally
            {
                if (!released) throwable.SetSnowballActiveLocal(false);
                owned = null;
            }
        }
    }

    private static void Resolve(string name)
    {
        nextResolve = Time.unscaledTime + 2f;
        var controller = CosmeticsController.instance;
        var rig = VRRig.LocalRig;
        if (maker == null || controller == null || rig == null || !controller.v2_allCosmeticsInfoAssetRef_isLoaded)
        { resolutionMessage = "Waiting for the game's throwable catalog."; return; }
        var cosmetics = controller.v2_allCosmetics;
        if (cosmetics == null) return;
        string wanted = name.Replace(" ", "");
        string? loadId = null;
        foreach (var item in cosmetics)
        {
            if (!item.isThrowable || string.IsNullOrEmpty(item.displayName) || item.displayName.Replace(" ", "").IndexOf(wanted, StringComparison.OrdinalIgnoreCase) < 0) continue;
            string id = CosmeticsV2Spawner_Dirty.GetPlayfabIdFromThrowableIndex(false, item.throwableIndex, out string mapped) ? mapped : item.playFabID;
            if (string.IsNullOrEmpty(id)) continue;
            if (maker.snowballs != null)
                foreach (var candidate in maker.snowballs)
                {
                    if (candidate == null || candidate.throwableMakerIndex != item.throwableIndex) continue;
                    selected = candidate;
                    selectedId = id;
                    selected.velocityEstimator = maker.velocityEstimator;
                    return;
                }
            loadId ??= id;
        }
        if (loadId != null && rig.cosmeticsObjectRegistry != null)
        {
            rig.cosmeticsObjectRegistry.Cosmetic(loadId);
            resolutionMessage = "Loading " + name.ToLowerInvariant() + "...";
        }
        else resolutionMessage = "No available " + name.ToLowerInvariant() + " was found in the loaded catalog.";
    }
    private static bool TryGetMaterial(SnowballThrowable throwable, out int material)
    {
        material = -1;
        if (throwable.matDataIndexes == null || selectedId.Length == 0) return false;
        foreach (int candidate in throwable.matDataIndexes)
            if (CosmeticsV2Spawner_Dirty.GetThrowableIDFromMaterialIndex(false, candidate, out string id) && string.Equals(id, selectedId, StringComparison.Ordinal))
            { material = candidate; return true; }
        return false;
    }

    internal static void Reset()
    {
        if (owned != null) owned.SetSnowballActiveLocal(false);
        owned = selected = null;
        maker = null;
        kind = selectedId = "";
        resolutionMessage = "Throwable data is not ready yet.";
        nextThrow = nextResolve = 0f;
    }
}
