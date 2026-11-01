// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Holdables;

internal sealed class PremadeHoldable : BetaFeature
{
    private static int selection;
    private readonly BuiltinHoldables builtin = new();
    private GameObject? item;
    private Renderer[] renderers = Array.Empty<Renderer>();
    private int model = -1, appearance = -1;
    private Color tint;

    [MenuButton("Equip Premade Holdable", ButtonType.Toggle, Category = "Holdables", Order = 4, runOption = RunOption.Once, Description = "Equip the selected premade model in your chosen hand. No bundle required.")]
    public static void EnablePremade() => Enable<PremadeHoldable>("Equip Premade Holdable");
    [MenuDisable("Equip Premade Holdable")]
    public static void DisablePremade() => Disable("Equip Premade Holdable");
    [MenuButton("Premade Holdable", ButtonType.Option, Category = "Holdables", Order = 5, Options = new[] { "Toy Sword", "Toy Hammer", "Round Shield", "Magic Wand", "Banana", "Flower", "Toy Microphone", "ChudOS Flag", "Wooden Cross", "Bisexual Flag", "Gay Pride Flag", "Trans Flag" }, Notify = false)]
    public static void SelectModel() => selection = Mathf.Clamp(ButtonManager.GetButton("Premade Holdable")?.OptionIndex ?? 0, 0, BuiltinHoldables.Models.Length - 1);

    protected override void Tick()
    {
        int selected = selection;
        if (model != selected)
        {
            if (item != null) { item.SetActive(false); Objects.Remove(item); Object.Destroy(item); }
            model = selected;
            item = Object.Instantiate(builtin.Get(BuiltinHoldables.Models[model].Id, transform), transform);
            item.name = "ChudOS Equipped " + BuiltinHoldables.Models[model].Name; item.SetActive(true); Objects.Add(item);
            renderers = item.GetComponentsInChildren<Renderer>(true); appearance = -1;
            HoldableFeedback.Pulse(ExpansionPreferences.LeftHand);
        }
        if (item == null) return;
        Transform hand = ExpansionPreferences.LeftHand ? CreativeTools.Left : CreativeTools.Right;
        item.transform.SetPositionAndRotation(hand.TransformPoint(ExpansionPreferences.HoldableOffset), hand.rotation * Quaternion.Euler(ExpansionPreferences.HoldableRotation));
        item.transform.localScale = Vector3.one * ExpansionPreferences.HoldableSize;
        Color color = ExpansionPreferences.HoldableColor;
        if (appearance == ExpansionPreferences.Revision && tint == color) return;
        appearance = ExpansionPreferences.Revision; tint = color;
        foreach (var renderer in renderers) { if (ExpansionPreferences.OriginalHoldableColors) renderer.SetPropertyBlock(null); else CreativeTools.Paint(renderer, color); }
    }
    protected override void Cleanup()
    {
        if (item != null) item.SetActive(false);
        builtin.Dispose();
    }
}
