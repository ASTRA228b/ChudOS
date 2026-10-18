// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra
//
// This file is part of ChudOS.
// ChudOS is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 3,
// as published by the Free Software Foundation.
//
// ChudOS is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ChudOS. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Reflection;
using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using GorillaNetworking;
using HarmonyLib;
using UnityEngine;

namespace ChudOS.Mods.Rig;

public sealed class EvolvingCosmeticManager : MonoBehaviour
{
    private const string WindowName = "Evolving Cosmetics";
    private readonly List<CosmeticInfo> cosmetics = new();
    private readonly Dictionary<EvolvingCosmetic, OriginalData> changed = new();
    private readonly HashSet<string> discovered = new(StringComparer.Ordinal);
    private CosmeticsController? subscribed;
    private bool visible;
    private Rect window = new(210f, 150f, 560f, 540f);
    private Vector2 scroll;
    private string selectedId = "";
    private float nextRefresh;

    private sealed class CosmeticInfo
    {
        internal string ItemId = "", DisplayName = "", AgeText = "";
        internal EvolvingCosmetic Cosmetic = null!;
        internal int[] MinimumDays = Array.Empty<int>();
        internal bool Cycling;
        internal float NextCycle;
    }
    private readonly struct OriginalData
    {
        internal readonly int? Days;
        internal readonly int Stage;
        internal OriginalData(EvolvingCosmetic cosmetic) { Days = Access.Days(cosmetic); Stage = cosmetic.SelectedObjectIndex; }
    }

    private static class Access
    {
        internal static readonly AccessTools.FieldRef<EvolvingCosmetic, int?> Days = AccessTools.FieldRefAccess<EvolvingCosmetic, int?>("_daysAccrued");
        internal static readonly FieldInfo Stages = AccessTools.Field(typeof(EvolvingCosmetic), "ageAwareGameObjects");
        internal static readonly Action<EvolvingCosmetic> Activate = AccessTools.MethodDelegate<Action<EvolvingCosmetic>>(AccessTools.Method(typeof(EvolvingCosmetic), "ActivateSelectedIndex"));
        internal static readonly Action<EvolvingCosmetic> Recalculate = AccessTools.MethodDelegate<Action<EvolvingCosmetic>>(AccessTools.Method(typeof(EvolvingCosmetic), "UpdateDaysAccrued"));
        internal static readonly Action<EvolvingCosmetic, int> Select = AccessTools.MethodDelegate<Action<EvolvingCosmetic, int>>(AccessTools.PropertySetter(typeof(EvolvingCosmetic), "SelectedObjectIndex"));
        private static FieldInfo? minimumDays;

        internal static int[] ReadStages(EvolvingCosmetic cosmetic)
        {
            if (Stages.GetValue(cosmetic) is not Array stages) return Array.Empty<int>();
            int[] result = new int[stages.Length];
            for (int i = 0; i < result.Length; i++)
            {
                object? definition = stages.GetValue(i);
                if (definition == null) continue;
                minimumDays ??= AccessTools.Field(definition.GetType(), "minActiveDays");
                if (minimumDays == null) throw new MissingFieldException("Evolving cosmetic stage has no minimum age.");
                result[i] = Convert.ToInt32(minimumDays.GetValue(definition));
            }
            return result;
        }
    }

    private void Awake() => GUIManager.Register(WindowName, SetVisible, () => visible);
    private void SetVisible(bool value) { visible = value; if (value) RefreshCosmetics(); }
    public void Open() => SetVisible(true);

    private void Update()
    {
        if (subscribed != CosmeticsController.instance)
        {
            if (subscribed != null) subscribed.OnCosmeticsUpdated -= RefreshCosmetics;
            subscribed = CosmeticsController.instance;
            if (subscribed != null) subscribed.OnCosmeticsUpdated += RefreshCosmetics;
            RefreshCosmetics();
        }
        if (visible && Time.unscaledTime >= nextRefresh) RefreshCosmetics();
        foreach (CosmeticInfo info in cosmetics)
        {
            if (!info.Cycling || info.Cosmetic == null || info.MinimumDays.Length == 0 || Time.unscaledTime < info.NextCycle) continue;
            SetStage(info, (info.Cosmetic.SelectedObjectIndex + 1) % info.MinimumDays.Length);
            info.NextCycle = Time.unscaledTime + 1f;
        }
    }

    public void RefreshCosmetics()
    {
        nextRefresh = Time.unscaledTime + 1f;
        if (CosmeticsController.instance == null || VRRig.LocalRig == null || VRRig.LocalRig.cosmeticsObjectRegistry == null)
        {
            foreach (CosmeticInfo info in cosmetics) Restore(info.Cosmetic);
            cosmetics.Clear();
            return;
        }
        discovered.Clear();
        CosmeticsController controller = CosmeticsController.instance;
        var registry = VRRig.LocalRig.cosmeticsObjectRegistry;
        var worn = controller.currentWornSet.items;
        if (worn == null) return;
        foreach (var item in worn)
        {
            if (item.isNullItem || string.IsNullOrEmpty(item.itemName) || item.itemName == "null") continue;
            CosmeticItemInstance instance = registry.Cosmetic(item.itemName);
            if (instance == null) continue;
            EvolvingCosmetic? cosmetic = FindIn(instance.objects) ?? FindIn(instance.leftObjects) ?? FindIn(instance.rightObjects) ?? FindIn(instance.holdableObjects);
            if (cosmetic == null || !discovered.Add(item.itemName)) continue;
            CosmeticInfo? info = cosmetics.Find(value => value.ItemId == item.itemName);
            if (info != null && info.Cosmetic != cosmetic)
            {
                Restore(info.Cosmetic);
                cosmetics.Remove(info);
                info = null;
            }
            if (info == null)
            {
                string name = string.IsNullOrEmpty(item.overrideDisplayName) ? item.displayName : item.overrideDisplayName;
                info = new CosmeticInfo
                {
                    ItemId = item.itemName, DisplayName = string.IsNullOrEmpty(name) ? item.itemName : name,
                    Cosmetic = cosmetic, MinimumDays = Access.ReadStages(cosmetic),
                    AgeText = Math.Max(0, Access.Days(cosmetic).GetValueOrDefault()).ToString()
                };
                cosmetics.Add(info);
            }
        }
        for (int i = cosmetics.Count - 1; i >= 0; i--)
            if (cosmetics[i].Cosmetic == null || !discovered.Contains(cosmetics[i].ItemId))
            {
                Restore(cosmetics[i].Cosmetic);
                cosmetics.RemoveAt(i);
            }
        cosmetics.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.DisplayName, b.DisplayName));
        if (!cosmetics.Exists(info => info.ItemId == selectedId)) selectedId = cosmetics.Count > 0 ? cosmetics[0].ItemId : "";
    }

    private static EvolvingCosmetic? FindIn(List<GameObject>? objects)
    {
        if (objects == null) return null;
        foreach (GameObject obj in objects)
        {
            if (obj == null) continue;
            EvolvingCosmetic found = obj.GetComponentInChildren<EvolvingCosmetic>(true);
            if (found != null) return found;
        }
        return null;
    }

    private void Capture(EvolvingCosmetic cosmetic)
    {
        if (!changed.ContainsKey(cosmetic)) changed.Add(cosmetic, new OriginalData(cosmetic));
    }

    private void SetStage(CosmeticInfo info, int stage)
    {
        if (info.Cosmetic == null || stage < 0 || stage >= info.MinimumDays.Length) return;
        Capture(info.Cosmetic);
        Access.Days(info.Cosmetic) = Math.Max(Access.Days(info.Cosmetic).GetValueOrDefault(), info.MinimumDays[stage]);
        Access.Select(info.Cosmetic, stage);
        Access.Activate(info.Cosmetic);
        VRRig.LocalRig?.reliableState?.SetIsDirty();
        info.AgeText = Access.Days(info.Cosmetic).GetValueOrDefault().ToString();
    }

    private void SetAge(CosmeticInfo info, int days)
    {
        if (info.Cosmetic == null) return;
        Capture(info.Cosmetic);
        Access.Days(info.Cosmetic) = Math.Max(0, days);
        Access.Activate(info.Cosmetic);
        info.AgeText = Math.Max(0, days).ToString();
    }

    private void ResetLocalData(CosmeticInfo info)
    {
        if (info.Cosmetic == null) return;
        Capture(info.Cosmetic);
        info.Cycling = false;
        Access.Recalculate(info.Cosmetic);
        VRRig.LocalRig?.reliableState?.SetIsDirty();
        info.AgeText = Access.Days(info.Cosmetic).GetValueOrDefault().ToString();
    }

    private void Restore(EvolvingCosmetic cosmetic)
    {
        if (!changed.TryGetValue(cosmetic, out OriginalData original)) return;
        changed.Remove(cosmetic);
        if (cosmetic == null) return;
        Access.Days(cosmetic) = original.Days;
        Access.Select(cosmetic, original.Stage);
        Access.Activate(cosmetic);
        VRRig.LocalRig?.reliableState?.SetIsDirty();
    }

    private void OnGUI()
    {
        if (!visible) return;
        GlobalStyles.INIT();
        window = GUILayout.Window(18573491, window, UIM, WindowName, GlobalStyles.WindowStyle);
    }

    private void UIM(int id)
    {
        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("Refresh Cosmetics")) RefreshCosmetics();
        if (MenuHelper.Button("Close")) visible = false;
        GUILayout.EndHorizontal();
        scroll = GUILayout.BeginScrollView(scroll);
        if (cosmetics.Count == 0) MenuHelper.Label("No evolving cosmetics found in your currently worn set.");
        foreach (CosmeticInfo info in cosmetics)
        {
            if (MenuHelper.Button(info.DisplayName + (info.ItemId == selectedId ? " [Selected]" : ""))) selectedId = info.ItemId;
        }
        CosmeticInfo? selected = cosmetics.Find(info => info.ItemId == selectedId);
        if (selected != null && selected.Cosmetic != null)
        {
            MenuHelper.Space();
            MenuHelper.Label(selected.DisplayName);
            MenuHelper.Label("Local Availability Age");
            GUILayout.BeginHorizontal();
            selected.AgeText = GUILayout.TextField(selected.AgeText, 10);
            bool valid = int.TryParse(selected.AgeText, out int days) && days >= 0;
            bool previousEnabled = GUI.enabled;
            GUI.enabled = valid;
            if (MenuHelper.Button("Apply Age")) SetAge(selected, days);
            GUI.enabled = previousEnabled;
            if (MenuHelper.Button("- Day")) SetAge(selected, Math.Max(0, Access.Days(selected.Cosmetic).GetValueOrDefault() - 1));
            if (MenuHelper.Button("+ Day")) SetAge(selected, (int)Math.Min(int.MaxValue, (long)Access.Days(selected.Cosmetic).GetValueOrDefault() + 1L));
            GUILayout.EndHorizontal();
            if (MenuHelper.Button("Reset Local Data")) ResetLocalData(selected);
            for (int stage = 0; stage < selected.MinimumDays.Length; stage++)
            {
                string label = $"Stage {stage + 1} ({selected.MinimumDays[stage]} days)" + (selected.Cosmetic.SelectedObjectIndex == stage ? " [Selected]" : "");
                if (MenuHelper.Button(label)) SetStage(selected, stage);
            }
            bool cycle = MenuHelper.Toggle("Cycle Stages", selected.Cycling);
            if (cycle != selected.Cycling)
            {
                selected.Cycling = cycle;
                selected.NextCycle = Time.unscaledTime + 1f;
            }
            MenuHelper.Label("Remote clients may reject stages that fail their age checks.");
        }
        GUILayout.EndScrollView();
        GUI.DragWindow(new Rect(0f, 0f, window.width, 25f));
    }

    private void OnDestroy()
    {
        if (subscribed != null) subscribed.OnCosmeticsUpdated -= RefreshCosmetics;
        GUIManager.Register(WindowName, _ => { }, () => false);
        var originals = new List<EvolvingCosmetic>(changed.Keys);
        foreach (EvolvingCosmetic cosmetic in originals) Restore(cosmetic);
        cosmetics.Clear();
        visible = false;
    }
}
