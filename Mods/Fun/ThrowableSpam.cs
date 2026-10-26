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
    private static string kind = "";
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
                NotifyLib.SendNotificationALL(name + " is not available yet. Equip it or visit a map where it is available.", 4000);
            }
            return;
        }
        var equipment = EquipmentInteractor.instance;
        if (equipment == null || equipment.rightHandHeldEquipment != null) return;
        if (maker.TryCreateSnowball(selected.matDataIndexes[0], out var throwable) && throwable != null)
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
        if (maker == null || maker.snowballs == null) return;
        int index = -1;
        var cosmetics = CosmeticsController.instance?.v2_allCosmetics;
        if (cosmetics != null)
            foreach (var item in cosmetics)
                if (item.isThrowable && item.displayName != null && item.displayName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    foreach (var candidate in maker.snowballs)
                        if (candidate != null && candidate.throwableMakerIndex == item.throwableIndex) { index = item.throwableIndex; break; }
        foreach (var candidate in maker.snowballs)
            if (candidate != null && ((index >= 0 && candidate.throwableMakerIndex == index) || candidate.name.IndexOf(name.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) >= 0))
            {
                if (candidate is GrowingSnowballThrowable) continue;
                selected = candidate;
                return;
            }
    }

    internal static void Reset()
    {
        if (owned != null) owned.SetSnowballActiveLocal(false);
        owned = selected = null;
        maker = null;
        kind = "";
        nextThrow = nextResolve = 0f;
    }
}
