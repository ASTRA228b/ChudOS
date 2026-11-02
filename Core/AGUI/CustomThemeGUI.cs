// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using ChudOS.Mods.Settings;
using UnityEngine;

namespace ChudOS.Core.AGUI;

internal static class CustomThemeGUI
{
    internal static bool Visible;
    internal static Rect Window = new(220f, 80f, 560f, 720f);
    private static readonly string[] Directions = { "Vertical", "Horizontal", "Diagonal" };
    private static CustomThemeDocument draft = new(), previous = new();
    private static string json = "", status = "", hex = "";
    private static int previousTheme, colorIndex;
    private static Color color;
    private static bool preview, pendingPreview, editing, restoring;
    private static float nextPreview;
    private static Vector2 scroll;
    internal static bool Editing => Visible && editing;

    internal static void SetVisible(bool value)
    {
        if (Visible == value || restoring) return;
        if (value)
        {
            previous = CustomThemeManager.Snapshot(); draft = CustomThemeManager.Copy(previous);
            previousTheme = ButtonManager.GetButton("Menu Theme")?.OptionIndex ?? 0;
            json = CustomThemeManager.Serialize(draft); status = "";
            preview = pendingPreview = false;
            SelectColor(0);
        }
        else RestorePrevious();
        Visible = value; editing = false; pendingPreview = false;
    }
    private static void RestorePrevious()
    {
        if (!preview) return;
        restoring = true;
        try
        {
            CustomThemeManager.Apply(previous, out _);
            var button = ButtonManager.GetButton("Menu Theme");
            if (button != null && button.OptionIndex != previousTheme) ButtonManager.ChangeOption(button, previousTheme - button.OptionIndex);
        }
        finally { restoring = false; }
    }
    internal static void CreateGUI()
    {
        if (Visible) Window = MenuHelper.MakeWindow(847216, Window, Contents, "ChudOS Custom Themes");
    }
    private static void SelectColor(int index)
    {
        colorIndex = (index + CustomThemeManager.ColorNames.Length) % CustomThemeManager.ColorNames.Length;
        hex = draft.Colors[CustomThemeManager.ColorNames[colorIndex]];
        CustomThemeManager.TryColor(hex, out color);
    }
    private static void Changed() => pendingPreview = true;
    private static void ApplyDraft(bool accept)
    {
        pendingPreview = false;
        if (!CustomThemeManager.Apply(draft, out string error)) { status = error; return; }
        status = accept ? "Theme applied. Save Menu State to keep it after restarting." : "Preview applied.";
        if (!accept) return;
        previous = CustomThemeManager.Snapshot();
        previousTheme = ButtonManager.GetButton("Menu Theme")?.OptionIndex ?? 0;
    }
    private static void Contents(int id)
    {
        scroll = GUILayout.BeginScrollView(scroll);
        MenuHelper.Label("Theme name");
        string name = Text("Name", draft.Name, 48);
        if (name != draft.Name) { draft.Name = name; Changed(); }
        bool live = MenuHelper.Toggle("Live Preview", preview);
        if (live != preview)
        {
            if (!live) { RestorePrevious(); pendingPreview = false; }
            preview = live;
            if (preview) Changed();
        }
        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("<")) SelectColor(colorIndex - 1);
        MenuHelper.Label(CustomThemeManager.ColorNames[colorIndex]);
        if (MenuHelper.Button(">")) SelectColor(colorIndex + 1);
        GUILayout.EndHorizontal();
        string editedHex = Text("Color", hex, 7);
        if (editedHex != hex)
        {
            hex = editedHex;
            draft.Colors[CustomThemeManager.ColorNames[colorIndex]] = hex;
            if (CustomThemeManager.TryColor(hex, out Color parsed)) color = parsed;
            Changed();
        }
        float red = Channel("Red", color.r), green = Channel("Green", color.g), blue = Channel("Blue", color.b);
        if (red != color.r || green != color.g || blue != color.b)
        {
            color = new Color(red, green, blue, 1f);
            hex = "#" + ColorUtility.ToHtmlStringRGB(color);
            draft.Colors[CustomThemeManager.ColorNames[colorIndex]] = hex;
            Changed();
        }
        Rect swatch = GUILayoutUtility.GetRect(40f, 28f, GUILayout.ExpandWidth(true));
        Color old = GUI.color; GUI.color = color; GUI.DrawTexture(swatch, Texture2D.whiteTexture); GUI.color = old;
        var gradient = draft.Gradient;
        bool enabled = MenuHelper.Toggle("Enable Gradient", gradient.Enabled), animated = MenuHelper.Toggle("Animate Gradient", gradient.Animated);
        if (enabled != gradient.Enabled || animated != gradient.Animated) { gradient.Enabled = enabled; gradient.Animated = animated; Changed(); }
        GradientColor("Gradient Start", ref gradient.Start);
        GradientColor("Gradient End", ref gradient.End);
        GradientColor("Pressed Gradient Start", ref gradient.PressedStart);
        GradientColor("Pressed Gradient End", ref gradient.PressedEnd);
        int direction = Array.IndexOf(Directions, gradient.Direction);
        int nextDirection = MenuHelper.Toolbar(Mathf.Max(0, direction), Directions);
        if (nextDirection != direction) { gradient.Direction = Directions[nextDirection]; Changed(); }
        MenuHelper.Label("Gradient speed");
        float speed = MenuHelper.Slider(gradient.Speed, 0.01f, 1f);
        if (speed != gradient.Speed) { gradient.Speed = speed; Changed(); }
        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("Apply")) ApplyDraft(true);
        if (MenuHelper.Button("Save Theme")) status = CustomThemeManager.Save(draft, out string saveError) ? "Saved " + draft.Name + ".json" : saveError;
        if (MenuHelper.Button("Reset Editor")) { draft = new(); SelectColor(0); Changed(); }
        GUILayout.EndHorizontal();
        MenuHelper.Label("Save Theme replaces a file with the same theme name.");
        MenuHelper.Space(); MenuHelper.Label("Paste or edit theme JSON");
        GUI.SetNextControlName("ChudOS.Theme.JSON");
        json = GUILayout.TextArea(json, CustomThemeManager.MaxBytes, GUILayout.MinHeight(120f));
        GUILayout.BeginHorizontal();
        if (MenuHelper.Button("Import JSON"))
        {
            if (CustomThemeManager.TryParse(json, out var imported, out string error))
            {
                draft = imported; SelectColor(0); Changed(); status = "Imported into the editor. Press Apply or enable Live Preview.";
            }
            else status = error;
        }
        if (MenuHelper.Button("Copy JSON")) { json = CustomThemeManager.Serialize(draft); GUIUtility.systemCopyBuffer = json; status = "Theme JSON copied."; }
        if (MenuHelper.Button("Paste Clipboard"))
        {
            string clipboard = GUIUtility.systemCopyBuffer;
            if (clipboard.Length <= CustomThemeManager.MaxBytes) json = clipboard;
            else status = "Clipboard JSON is too large.";
        }
        GUILayout.EndHorizontal();
        if (MenuHelper.Button("Reload Saved Themes")) status = CustomThemeManager.Reload();
        foreach (string filename in CustomThemeManager.Files)
        {
            if (!MenuHelper.Button(filename)) continue;
            if (CustomThemeManager.Read(filename, out var saved, out string error))
            {
                draft = saved; json = CustomThemeManager.Serialize(draft); SelectColor(0); Changed(); status = "Loaded into the editor: " + filename;
            }
            else status = error;
        }
        MenuHelper.Label(status);
        if (MenuHelper.Button("Back / Close")) SetVisible(false);
        GUILayout.EndScrollView();
        editing = Visible && GUI.GetNameOfFocusedControl().StartsWith("ChudOS.Theme.", StringComparison.Ordinal);
        if (Visible && preview && pendingPreview && Event.current.type == EventType.Repaint && Time.unscaledTime >= nextPreview)
        {
            nextPreview = Time.unscaledTime + 0.15f;
            ApplyDraft(false);
        }
        GUI.DragWindow();
    }
    private static string Text(string name, string value, int limit)
    {
        GUI.SetNextControlName("ChudOS.Theme." + name);
        return GUILayout.TextField(value, limit);
    }
    private static float Channel(string name, float value) { MenuHelper.Label(name); return MenuHelper.Slider(value, 0f, 1f); }
    private static void GradientColor(string label, ref string value)
    {
        MenuHelper.Label(label);
        string edited = Text(label, value, 7);
        if (edited == value) return;
        value = edited; Changed();
    }
}
