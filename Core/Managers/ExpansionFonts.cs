// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Mods.Settings;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Core.Managers;

internal sealed class ExpansionFontBinding : MonoBehaviour
{
    internal float Size, Width;
}

internal static class ExpansionFonts
{
    private static readonly string[] Names = { "Arial", "Segoe UI", "Verdana", "Consolas" };
    private static readonly Dictionary<int, Font> fonts = new();
    private static readonly HashSet<Font> ownedFonts = new();
    private static readonly Dictionary<int, TMP_FontAsset> tmpFonts = new();
    private static readonly Dictionary<TMP_Text, TMP_FontAsset> boardFonts = new();
    private static GUISkin? guiSkin, sourceSkin;
    private static int guiChoice = -1;
    private static float guiScale;

    internal static Font GetFont(int index)
    {
        index = Mathf.Clamp(index, 0, Names.Length - 1);
        if (fonts.TryGetValue(index, out var cached) && cached != null) return cached;
        Font? font = index == 0 ? Resources.GetBuiltinResource<Font>("Arial.ttf") : Font.CreateDynamicFontFromOSFont(new[] { Names[index], "Segoe UI Symbol", "Arial" }, 48);
        if (font != null && index != 0) ownedFonts.Add(font);
        font ??= Resources.GetBuiltinResource<Font>("Arial.ttf");
        return fonts[index] = font;
    }
    internal static void Bind(TextMesh text, float size)
    {
        var binding = text.GetComponent<ExpansionFontBinding>() ?? text.gameObject.AddComponent<ExpansionFontBinding>();
        binding.Size = size; Apply(text);
    }
    internal static void Fit(TextMesh text, float width)
    {
        var binding = text.GetComponent<ExpansionFontBinding>() ?? text.gameObject.AddComponent<ExpansionFontBinding>();
        if (binding.Size <= 0f) binding.Size = text.characterSize;
        binding.Width = width; Apply(text);
    }
    internal static void Apply(TextMesh text)
    {
        if (text == null) return;
        Font font = GetFont(ExpansionPreferences.MenuFont);
        text.font = font; text.GetComponent<Renderer>().sharedMaterial = font.material;
        var binding = text.GetComponent<ExpansionFontBinding>();
        if (binding == null) return;
        float size = binding.Size * ExpansionPreferences.MenuTextSize;
        if (binding.Width > 0f && text.text.Length > 0)
        {
            font.RequestCharactersInTexture(text.text, text.fontSize, text.fontStyle);
            float line = 0f, widest = 0f;
            for (int i = 0; i < text.text.Length; i++)
            {
                char letter = text.text[i];
                if (letter == '\n') { widest = Mathf.Max(widest, line); line = 0f; continue; }
                if (text.richText && letter == '<') { int end = text.text.IndexOf('>', i + 1); if (end > i) { i = end; continue; } }
                if (font.GetCharacterInfo(letter, out var character, text.fontSize, text.fontStyle)) line += character.advance;
            }
            float units = Mathf.Max(widest, line) / 10f;
            if (units > 0f) size = Mathf.Min(size, binding.Width * 0.88f / units);
        }
        text.characterSize = Mathf.Max(0.001f, size);
    }
    internal static void ApplyBoard(TMP_Text text)
    {
        if (text == null) return;
        if (!boardFonts.ContainsKey(text)) boardFonts[text] = text.font;
        int choice = ExpansionPreferences.BoardFont;
        if (choice == 0) { text.font = boardFonts[text]; return; }
        if (!tmpFonts.TryGetValue(choice, out var font) || font == null)
        {
            font = TMP_FontAsset.CreateFontAsset(GetFont(choice));
            font.name = "ChudOS Board Font " + choice; font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            tmpFonts[choice] = font;
        }
        var original = boardFonts[text];
        font.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
        if (original != null && original != font && !font.fallbackFontAssetTable.Contains(original)) font.fallbackFontAssetTable.Add(original);
        if (text.font != font) text.font = font;
    }
    internal static void RestoreBoard(TMP_Text text)
    {
        if (text == null || !boardFonts.TryGetValue(text, out var original)) return;
        text.font = original; boardFonts.Remove(text);
    }
    internal static void RefreshBoards()
    {
        foreach (var text in boardFonts.Keys.ToArray())
        {
            if (text == null) { boardFonts.Remove(text!); continue; }
            ApplyBoard(text);
        }
    }
    internal readonly struct GUIScope : IDisposable
    {
        private readonly GUISkin previous;
        internal GUIScope(GUISkin replacement) { previous = GUI.skin; GUI.skin = replacement; }
        public void Dispose() => GUI.skin = previous;
    }
    internal static GUIScope BeginGUI()
    {
        if (guiSkin == null || sourceSkin != GUI.skin || guiChoice != ExpansionPreferences.GUIFont || guiScale != ExpansionPreferences.GUITextSize)
        {
            if (guiSkin != null) Object.Destroy(guiSkin);
            sourceSkin = GUI.skin; guiSkin = Object.Instantiate(sourceSkin);
            guiChoice = ExpansionPreferences.GUIFont; guiScale = ExpansionPreferences.GUITextSize;
            Font? font = guiChoice == 0 ? sourceSkin.font : GetFont(guiChoice);
            guiSkin.font = font;
            foreach (GUIStyle style in new[] { guiSkin.label, guiSkin.button, guiSkin.toggle, guiSkin.textField, guiSkin.textArea, guiSkin.window })
            {
                style.font = font;
                style.fontSize = guiScale == 1f ? style.fontSize : Mathf.RoundToInt((style.fontSize > 0 ? style.fontSize : 13) * guiScale);
            }
        }
        var styles = ChudOS.Core.AGUI.GUIHelpers.GlobalStyles.Buttonss;
        if (styles != null) { styles.font = guiSkin.font; styles.fontSize = guiSkin.button.fontSize; }
        var window = ChudOS.Core.AGUI.GUIHelpers.GlobalStyles.WindowStyle;
        if (window != null) { window.font = guiSkin.font; window.fontSize = guiSkin.window.fontSize; }
        var label = ChudOS.Core.AGUI.GUIHelpers.GlobalStyles.LabelStyle;
        if (label != null) { label.font = guiSkin.font; label.fontSize = guiSkin.label.fontSize; }
        return new GUIScope(guiSkin);
    }
    internal static void Cleanup()
    {
        foreach (var pair in boardFonts) if (pair.Key != null) pair.Key.font = pair.Value;
        boardFonts.Clear();
        foreach (var font in tmpFonts.Values)
        {
            if (font == null) continue;
            if (font.material != null) Object.Destroy(font.material);
            foreach (var atlas in font.atlasTextures) if (atlas != null) Object.Destroy(atlas);
            Object.Destroy(font);
        }
        tmpFonts.Clear();
        foreach (var font in ownedFonts) if (font != null) Object.Destroy(font);
        ownedFonts.Clear(); fonts.Clear();
        if (guiSkin != null) Object.Destroy(guiSkin);
        guiSkin = sourceSkin = null; guiChoice = -1;
    }
}
