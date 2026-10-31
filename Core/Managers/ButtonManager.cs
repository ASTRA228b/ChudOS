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
using System.Linq;
using System.Reflection;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Core.Managers;

public static class ButtonManager
{
    public sealed class ButtonData
    {
        public MenuButtonAttribute Attribute = null!;
        public Action Run = null!;
        public Action? OnEnable;
        public Action? OnDisable;
        public bool Enabled;
        public int OptionIndex;
        public string Name => Attribute.Name ?? string.Empty;
        public ButtonType Type => Attribute.Type;
        public string Category => Attribute.Category;
        public string Option => Type == ButtonType.Option && Attribute.Options.Length > 0 ? Attribute.Options[OptionIndex] : "";
        public string DisplayName
        {
            get
            {
                string label = TranslationManager.T(Name);
                if (!string.IsNullOrEmpty(Attribute.Controls)) label += " " + Attribute.Controls;
                if (Type == ButtonType.Toggle) return $"{label} [{TranslationManager.T(Enabled ? "ON" : "OFF")}]";
                return Type == ButtonType.Option ? $"{label}: {TranslationManager.T(Option)}" : label;
            }
        }
    }

    private static readonly List<ButtonData> All = new();
    private static readonly Dictionary<string, ButtonData> ByName = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<ButtonData> UpdateSchedule = new(), FixedSchedule = new(), LateSchedule = new();
    private static ButtonData[] UpdateRuns = Array.Empty<ButtonData>(), FixedRuns = Array.Empty<ButtonData>(), LateRuns = Array.Empty<ButtonData>();
    public static IReadOnlyList<ButtonData> Buttons => All;
    public static bool Initialized { get; private set; }
    public static event Action<string>? OnNotification;
    public static event Action? OnButtonsChanged;

    public static void Initialize()
    {
        if (Initialized) return;
        var enables = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);
        var disables = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);
        foreach (Type type in typeof(ButtonManager).Assembly.GetTypes())
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                MenuButtonAttribute? attr = method.GetCustomAttribute<MenuButtonAttribute>();
                MenuEnableAttribute? enable = method.GetCustomAttribute<MenuEnableAttribute>();
                MenuDisableAttribute? disable = method.GetCustomAttribute<MenuDisableAttribute>();
                if (attr == null && enable == null && disable == null) continue;
                if (method.ReturnType != typeof(void) || method.GetParameters().Length != 0 || method.ContainsGenericParameters)
                {
                    Debug.LogWarning($"[{Constantss.GUID}] Invalid attributed method: {type.Name}.{method.Name}");
                    continue;
                }
                Action action;
                try { action = (Action)Delegate.CreateDelegate(typeof(Action), method); }
                catch (Exception ex) { Debug.LogWarning($"[{Constantss.GUID}] Invalid button delegate: {ex.Message}"); continue; }
                if (attr != null)
                {
                    if (string.IsNullOrWhiteSpace(attr.Name) || (attr.Name != null && ByName.ContainsKey(attr.Name)) || (attr.Type == ButtonType.Option && (attr.Options == null || attr.Options.Length == 0)))
                    {
                        Debug.LogWarning($"[{Constantss.GUID}] Duplicate/invalid button: {attr.Name}");
                        continue;
                    }
                    ButtonData data = new() { Attribute = attr, Run = action };
                    All.Add(data);
                    ByName.Add(data.Name, data);
                }
                if (enable != null) enables[enable.Name] = action;
                if (disable != null) disables[disable.Name] = action;
            }
        }
        All.Sort((a, b) =>
        {
            int c = string.Compare(a.Category, b.Category, StringComparison.OrdinalIgnoreCase);
            if (c != 0) return c;
            c = a.Attribute.Order.CompareTo(b.Attribute.Order);
            return c != 0 ? c : string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        });
        foreach (ButtonData button in All)
        {
            enables.TryGetValue(button.Name, out button.OnEnable);
            disables.TryGetValue(button.Name, out button.OnDisable);
        }
        Initialized = true;
        foreach (ButtonData button in All)
            if (button.Type == ButtonType.Toggle && button.Attribute.DefaultEnabled) SetEnabled(button, true);
        Debug.Log($"[{Constantss.GUID}] Registered {All.Count} menu buttons.");
        OnButtonsChanged?.Invoke();
    }

    public static ButtonData? GetButton(string name) => ByName.TryGetValue(name, out ButtonData value) ? value : null;
    public static List<ButtonData> GetCategory(string name) => All.Where(b => string.Equals(b.Category, name, StringComparison.OrdinalIgnoreCase)).ToList();
    public static IEnumerable<string> GetCategories() => All.Select(b => b.Category).Distinct(StringComparer.OrdinalIgnoreCase);
    public static IEnumerable<string> GetEnabledNames() => All.Where(b => b.Type == ButtonType.Toggle && b.Enabled && b.Category != "Settings").Select(b => b.Name);
    public static void Press(string name) { ButtonData? button = GetButton(name); if (button != null) Press(button); }
    public static void Press(ButtonData button)
    {
        if (!All.Contains(button)) return;
        if (button.Type == ButtonType.Toggle) SetEnabled(button, !button.Enabled);
        else if (button.Type == ButtonType.Option) ChangeOption(button, 1);
        else
        {
            if (button.Category == "Detected") ChudOS.Mods.Safety.ModGuards.WarnDetected();
            if (Execute(button.Run, button.Name)) Notify(button, button.Attribute.messagePress, $"{button.Name} pressed");
        }
    }
    public static void SetEnabled(string name, bool state) { ButtonData? button = GetButton(name); if (button != null) SetEnabled(button, state); }
    public static void SetEnabled(ButtonData button, bool state)
    {
        if (!All.Contains(button) || button.Type != ButtonType.Toggle || button.Enabled == state) return;
        button.Enabled = state;
        if (state)
        {
            ChudOS.Mods.Safety.ModGuards.OnEnable(button);
            Execute(button.OnEnable, button.Name);
            if (button.Attribute.runOption == RunOption.Once && !Execute(button.Run, button.Name)) button.Enabled = false;
            Notify(button, button.Attribute.messageEnable, $"{button.Name} enabled");
        }
        else
        {
            Execute(button.OnDisable, button.Name);
            Notify(button, button.Attribute.messageDisable, $"{button.Name} disabled");
        }
        RebuildSchedules();
        OnButtonsChanged?.Invoke();
    }
    public static void ChangeOption(ButtonData button, int direction)
    {
        if (!All.Contains(button) || button.Type != ButtonType.Option) return;
        int count = button.Attribute.Options.Length;
        int next = button.OptionIndex + direction;
        next = button.Attribute.LoopOptions ? (next % count + count) % count : Mathf.Clamp(next, 0, count - 1);
        if (next == button.OptionIndex) return;
        button.OptionIndex = next;
        Execute(button.Run, button.Name);
        Notify(button, button.Attribute.messagePress, $"{button.Name}: {button.Option}");
        OnButtonsChanged?.Invoke();
    }
    public static void Update() => RunPhase(RunOption.Update);
    public static void FixedUpdate() => RunPhase(RunOption.FixedUpdate);
    public static void LateUpdate() => RunPhase(RunOption.LateUpdate);
    private static void RunPhase(RunOption phase)
    {
        if (!Initialized) return;
        ButtonData[] scheduled = phase == RunOption.Update ? UpdateRuns : phase == RunOption.FixedUpdate ? FixedRuns : LateRuns;
        foreach (ButtonData button in scheduled)
            if (button.Enabled && !Execute(button.Run, button.Name)) SetEnabled(button, false);
    }
    private static void RebuildSchedules()
    {
        UpdateSchedule.Clear(); FixedSchedule.Clear(); LateSchedule.Clear();
        foreach (ButtonData button in All)
        {
            if (!button.Enabled || button.Type != ButtonType.Toggle) continue;
            switch (button.Attribute.runOption)
            {
                case RunOption.Update: UpdateSchedule.Add(button); break;
                case RunOption.FixedUpdate: FixedSchedule.Add(button); break;
                case RunOption.LateUpdate: LateSchedule.Add(button); break;
            }
        }
        UpdateRuns = UpdateSchedule.ToArray();
        FixedRuns = FixedSchedule.ToArray();
        LateRuns = LateSchedule.ToArray();
    }
    private static bool Execute(Action? action, string name)
    {
        if (action == null) return true;
        try { action(); return true; }
        catch (Exception ex) { if (ChudOS.Mods.Safety.ModGuards.ShouldLogError(name)) Debug.LogError($"[{Constantss.GUID}] {name}: {ex}"); return false; }
    }
    private static void Notify(ButtonData button, string message, string fallback)
    {
        if (button.Attribute.Notify) OnNotification?.Invoke(string.IsNullOrWhiteSpace(message) ? fallback : message);
    }
}
