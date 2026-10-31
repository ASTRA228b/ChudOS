// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Mods.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChudOS.Core;

public sealed partial class Main
{
    private static readonly string[][] LanguageRows =
    {
        new[] { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" },
        new[] { "QWERTYUIOP", "ASDFGHJKLÇ", "ZXCVBNM", "ÁÀÂÃÉÊÍÓÔÕÚÜ" },
        new[] { "QWERTYUIOP", "ASDFGHJKLÑ", "ZXCVBNM", "ÁÉÍÓÚÜ¡¿" },
        new[] { "AZERTYUIOP", "QSDFGHJKLM", "WXCVBN", "ÀÂÇÉÈÊËÎÏ", "ÔŒÙÛÜŸÆ" },
        new[] { "QWERTZUIOPÜ", "ASDFGHJKLÖÄ", "YXCVBNMß" },
        new[] { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM", "ÀÈÉÌÒÙ" }
    };
    internal static string[] KeyboardRows(int language) => LanguageRows[Mathf.Clamp(language, 0, LanguageRows.Length - 1)];
    private static TextMesh? KeyboardQueryText;
    private Keyboard? searchKeyboard;
    private static float KeyboardY, KeyboardZ, KeyboardHeight, KeyboardWidth;
    private static int keyboardScreenWidth = -1, keyboardScreenHeight = -1;

    public static void RefreshKeyboardLayout() { ClearKeyboard(); RequestRefresh(); }
    private static void CreateKeyboard()
    {
        if (KeyboardBuilt || MenuMain == null) return;
        KeyboardBuilt = true;
        float size = KeyboardSettings.Size;
        bool panel = KeyboardSettings.Style == 1 || KeyboardSettings.Style == 3;
        bool numbers = KeyboardSettings.Numbers == 1 || KeyboardSettings.Numbers == 0 && panel;
        string[] rows = KeyboardRows(KeyboardSettings.Language);
        int columns = numbers ? 10 : 0;
        foreach (string row in rows) columns = Mathf.Max(columns, row.Length);
        float pitch = (KeyboardSettings.Style == 2 ? 0.085f : KeyboardSettings.Style == 3 ? 0.115f : 0.098f) * size;
        float gap = new[] { 0.012f, 0.006f, 0.021f }[KeyboardSettings.Spacing] * size;
        float spacing = (KeyboardSettings.Style == 2 ? 0.17f : 0.20f) * size;
        float width = Mathf.Max(1.11f * size, columns * pitch + 0.08f * size);
        int count = rows.Length + (numbers ? 1 : 0);
        float height = count * spacing + (panel ? 0.80f : 0.34f) * size;
        float centerY = 0f, centerZ = 0f;
        if (KeyboardSettings.Placement == 2) centerY = -1.14f - 0.08f - height / 2f;
        else centerZ = (KeyboardSettings.Placement == 0 ? 1f : -1f) * (0.526f + 0.08f + width / 2f);
        KeyboardY = centerY; KeyboardZ = centerZ; KeyboardHeight = height; KeyboardWidth = width;
        CubeKeyboard("Keyboard Back", 0.55f, centerY, centerZ, height, width, MenuMat);
        if (panel)
        {
            KeyboardObjects.Add(Text("Search", centerY + height / 2f - 0.12f * size, centerZ, 0.016f * size, color: TextColor).gameObject);
            KeyboardQueryText = Text("", centerY + height / 2f - 0.30f * size, centerZ, 0.014f * size, color: SecondaryTextColor);
            KeyboardObjects.Add(KeyboardQueryText.gameObject);
            MenuTextOriginal.Remove(KeyboardQueryText);
        }
        float top = centerY + height / 2f - (panel ? 0.53f : 0.13f) * size;
        for (int row = 0; row < count; row++)
        {
            string letters = numbers && row == 0 ? "1234567890" : rows[row - (numbers ? 1 : 0)];
            float first = centerZ - (letters.Length - 1) * pitch / 2f;
            for (int key = 0; key < letters.Length; key++)
                KeyboardButton(letters[key].ToString(), top - row * spacing, first + key * pitch, pitch - gap, spacing - gap);
        }
        float bottom = centerY - height / 2f + 0.13f * size;
        KeyboardButton("Space", bottom, centerZ - 0.28f * size, 0.22f * size, 0.17f * size);
        KeyboardButton("Del", bottom, centerZ - 0.072f * size, 0.16f * size, 0.17f * size);
        KeyboardButton("Clear", bottom, centerZ + 0.127f * size, 0.20f * size, 0.17f * size);
        KeyboardButton("Done", bottom, centerZ + 0.351f * size, 0.20f * size, 0.17f * size);
        RefreshKeyboardQuery();
        RefreshDesktopKeyboardBounds(true);
    }
    private static void RefreshKeyboardQuery()
    {
        if (KeyboardQueryText == null) return;
        string value = SearchManager.Query.Length == 0 ? TranslationManager.T("Search...") : SearchManager.Query;
        if (KeyboardQueryText.text == value) return;
        KeyboardQueryText.text = value;
        FitButtonText(KeyboardQueryText, value, 0.95f * KeyboardSettings.Size);
    }
    private static void RefreshDesktopKeyboardBounds(bool force = false)
    {
        if (!OnPC || MenuMain == null || PCamera == null) return;
        if (!force && keyboardScreenWidth == Screen.width && keyboardScreenHeight == Screen.height) return;
        keyboardScreenWidth = Screen.width; keyboardScreenHeight = Screen.height;
        var menu = MenuMain.transform;
        if (!KeyboardBuilt)
        {
            menu.localScale = DesktopMenuScale;
            menu.localPosition = new Vector3(-0.12f, -0.02f, 0.8f);
            return;
        }
        float minY = Mathf.Min(-1.14f, KeyboardY - KeyboardHeight / 2f), maxY = Mathf.Max(1.14f, KeyboardY + KeyboardHeight / 2f);
        float minZ = Mathf.Min(-0.53f, KeyboardZ - KeyboardWidth / 2f), maxZ = Mathf.Max(0.53f, KeyboardZ + KeyboardWidth / 2f);
        float halfHeight = PCamera.orthographic ? PCamera.orthographicSize : Mathf.Tan(PCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 0.7f;
        float factor = Mathf.Min(1f, Mathf.Min(halfHeight * 1.8f / ((maxY - minY) * DesktopMenuScale.y), halfHeight * PCamera.aspect * 1.8f / ((maxZ - minZ) * DesktopMenuScale.z)));
        if (float.IsNaN(factor) || float.IsInfinity(factor) || factor <= 0f) return;
        menu.localScale = DesktopMenuScale * factor;
        menu.localPosition = new Vector3(-(minZ + maxZ) * DesktopMenuScale.z * factor / 2f, -(minY + maxY) * DesktopMenuScale.y * factor / 2f, 0.8f);
    }

    private void BindSearchKeyboard(Keyboard? keyboard)
    {
        if (searchKeyboard == keyboard) return;
        if (searchKeyboard != null) searchKeyboard.onTextInput -= SearchTextInput;
        searchKeyboard = keyboard;
        if (searchKeyboard != null) searchKeyboard.onTextInput += SearchTextInput;
    }
    private void SearchTextInput(char character)
    {
        if (!IsMenuOpen || !SearchManager.Active || !SearchManager.KeyboardVisible || char.IsControl(character) || char.IsSurrogate(character)) return;
        SearchManager.Key(character.ToString());
    }
}
