// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using GorillaNetworking;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using ChudOS.Mods.Safety;

namespace ChudOS.Mods.Spammers;

public static class ThrowableSpam
{
    private static SnowballThrowable? selected, owned;
    private static SnowballMaker? maker;
    private static string kind = "", selectedId = "", resolutionMessage = "Throwable data is not ready yet.";
    private static float nextThrow, nextResolve, nextNotice, nextLoad, interval = 0.25f;
    private static int input;
    private static bool leftHand, inputUnavailable;
    private static VRRig? owner;
    public static string Status => resolutionMessage;

    [MenuButton("Snowball Spam", ButtonType.Toggle, Category = "Spammers", Order = 0, Controls = "(RT)/(LMB)", Description = "Repeated native releases. Close the menu and hold the selected input; requires a loaded throwable.")]
    public static void Snowballs() => Tick("Snowball");
    [MenuEnable("Snowball Spam")]
    public static void StartSnowballs() { ButtonManager.SetEnabled("Water Balloon Spam", false); Reset(); }
    [MenuDisable("Snowball Spam")]
    public static void StopSnowballs() => Reset();
    [MenuButton("Water Balloon Spam", ButtonType.Toggle, Category = "Spammers", Order = 1, Controls = "(RT)/(LMB)", Description = "Repeated native releases using the selected hand and normal hand velocity")]
    public static void Balloons() => Tick("Water Balloon");
    [MenuEnable("Water Balloon Spam")]
    public static void StartBalloons() { ButtonManager.SetEnabled("Snowball Spam", false); Reset(); }
    [MenuDisable("Water Balloon Spam")]
    public static void StopBalloons() => Reset();
    [MenuButton("Throwable Interval", ButtonType.Option, Category = "Settings", Order = 247, Options = new[] { "0.25s", "0.5s", "1s" }, Notify = false)]
    public static void SetInterval() => interval = new[] { 0.25f, 0.5f, 1f }[ButtonManager.GetButton("Throwable Interval")!.OptionIndex];
    [MenuButton("Throwable Hand", ButtonType.Option, Category = "Settings", Order = 328, Options = new[] { "Right", "Left" }, Notify = false)]
    public static void SetHand() { leftHand = ButtonManager.GetButton("Throwable Hand")!.OptionIndex == 1; Reset(); }
    [MenuButton("Throwable Input", ButtonType.Option, Category = "Settings", Order = 329, Options = new[] { "RT", "LT", "RG", "LG", "A", "B", "X", "Y", "RJ", "LJ" }, Notify = false)]
    public static void SetInput()
    {
        input = ButtonManager.GetButton("Throwable Input")!.OptionIndex;
        inputUnavailable = false;
        string controls = "(" + new[] { "RT", "LT", "RG", "LG", "A", "B", "X", "Y", "RJ", "LJ" }[input] + ")/(LMB)";
        foreach (string name in new[] { "Snowball Spam", "Water Balloon Spam" })
            if (ButtonManager.GetButton(name) is { } button) button.Attribute.Controls = controls;
    }
    [MenuButton("Throwable Status", ButtonType.Press, Category = "Spammers", Order = 10, Notify = false)]
    public static void ShowStatus() => NotifyLib.SendNotificationALL(Status, 4500);
    private static bool Held()
    {
        if (ChudOS.Core.Main.IsMenuOpen) return false;
        var mouse = Mouse.current;
        if (mouse?.leftButton.isPressed == true && !GUIManager.ContainsPointer(new Vector2(mouse.position.ReadValue().x, Screen.height - mouse.position.ReadValue().y))) return true;
        if (inputUnavailable) return false;
        try { return ModInput.Held(input); }
        catch (Exception) { inputUnavailable = true; resolutionMessage = "Controller input unavailable. Choose another input or use LMB."; return false; }
    }

    private static void Tick(string name)
    {
        if (GorillaTagger.Instance == null || VRRig.LocalRig == null || !Held()) return;
        if (Time.unscaledTime < nextThrow) return;
        var handMaker = leftHand ? SnowballMaker.leftHandInstance : SnowballMaker.rightHandInstance;
        if (maker != handMaker || kind != name || owner != VRRig.LocalRig) { Reset(); kind = name; maker = handMaker; owner = VRRig.LocalRig; }
        PortTools.Register(Reset);
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
        if (maker.velocityEstimator == null || owner.Creator == null || !owner.Creator.IsLocal)
        {
            selected = null;
            selectedId = "";
            resolutionMessage = "Waiting for local hand tracking and throwable ownership.";
            return;
        }
        var equipment = EquipmentInteractor.instance;
        if (equipment == null) return;
        if ((leftHand ? equipment.disableLeftGrab : equipment.disableRightGrab) || (leftHand ? equipment.leftHandHeldEquipment : equipment.rightHandHeldEquipment) != null)
        { resolutionMessage = "Selected hand is occupied or grabbing is disabled."; return; }
        if (!TryGetMaterial(selected, out int material))
        {
            selected = null;
            selectedId = "";
            resolutionMessage = "The throwable material mapping is not ready yet.";
            return;
        }
        if (PhotonNetwork.InRoom && !NetworkSafety.TryRPC()) { resolutionMessage = "Waiting for the shared network request budget."; return; }
        if (maker.TryCreateSnowball(material, out var throwable) && throwable != null)
        {
            owned = throwable;
            bool released = false;
            try
            {
                if (throwable.throwableMakerIndex == selected.throwableMakerIndex && throwable.IsMine())
                {
                    throwable.velocityEstimator = maker.velocityEstimator;
                    GameObject hand = leftHand ? equipment.leftHand : equipment.rightHand;
                    if (hand != null) released = throwable.OnRelease(null!, hand);
                }
                resolutionMessage = released ? name + " native release requested." : "The game rejected the throwable ownership or release.";
            }
            finally
            {
                if (throwable != null && (!released || throwable.gameObject.activeSelf)) throwable.SetSnowballActiveLocal(false);
                owned = null;
            }
        }
        else resolutionMessage = "The game could not create this throwable yet.";
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
            string id = CosmeticsV2Spawner_Dirty.GetPlayfabIdFromThrowableIndex(leftHand, item.throwableIndex, out string mapped) && !string.IsNullOrEmpty(mapped) ? mapped : item.playFabID;
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
            if (Time.unscaledTime >= nextLoad) { nextLoad = Time.unscaledTime + 5f; rig.cosmeticsObjectRegistry.Cosmetic(loadId); }
            resolutionMessage = "Loading " + name.ToLowerInvariant() + "...";
        }
        else resolutionMessage = "No available " + name.ToLowerInvariant() + " was found in the loaded catalog.";
    }
    private static bool TryGetMaterial(SnowballThrowable throwable, out int material)
    {
        material = -1;
        if (throwable.matDataIndexes == null || selectedId.Length == 0) return false;
        foreach (int candidate in throwable.matDataIndexes)
            if (CosmeticsV2Spawner_Dirty.GetThrowableIDFromMaterialIndex(leftHand, candidate, out string id) && string.Equals(id, selectedId, StringComparison.Ordinal))
            { material = candidate; return true; }
        return false;
    }

    internal static void Reset()
    {
        if (owned != null) owned.SetSnowballActiveLocal(false);
        owned = selected = null;
        maker = null;
        owner = null;
        inputUnavailable = false;
        kind = selectedId = "";
        resolutionMessage = "Throwable data is not ready yet.";
        nextThrow = nextResolve = nextLoad = 0f;
    }
}
