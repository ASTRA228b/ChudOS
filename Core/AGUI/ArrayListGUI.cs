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

using UnityEngine;
using UnityEngine.UI;
using ChudOS.Core.Managers;
using Object = UnityEngine.Object;

namespace ChudOS.Core.AGUI;

public static class ArrayListGUI
{
    private static GameObject? root;
    private static Text? header, entries;
    private static string[] names = Array.Empty<string>();
    public static bool HasEntries => GUIManager.ArrayListEnabled && names.Length > 0;

    public static void Initialize()
    {
        ButtonManager.OnButtonsChanged -= Refresh;
        ButtonManager.OnButtonsChanged += Refresh;
        TranslationManager.Changed -= Refresh;
        TranslationManager.Changed += Refresh;
        Refresh();
    }

    public static void SetVisible(bool value) { GUIManager.ArrayListEnabled = value; Refresh(); }

    private static void Refresh()
    {
        names = ButtonManager.GetEnabledNames().Select(TranslationManager.T).ToArray();
        if (HasEntries && root == null) Create();
        if (root == null) return;
        root.SetActive(HasEntries);
        bool overlayTop = ButtonManager.GetButton("Overlay")?.Enabled == true && (ButtonManager.GetButton("Overlay Position")?.OptionIndex ?? 0) == 0;
        float offset = overlayTop ? 200f : 0f;
        if (header != null) header.rectTransform.anchoredPosition = new Vector2(-20f, -12f - offset);
        if (entries != null) entries.rectTransform.anchoredPosition = new Vector2(-20f, -41f - offset);
        if (header != null) header.text = $"ChudOS | {names.Length} {TranslationManager.T("ACTIVE")}";
        if (entries != null)
        {
            entries.text = string.Join("\n", names);
            entries.rectTransform.sizeDelta = new Vector2(280f, names.Length * 23f);
        }
        RefreshTheme();
    }

    private static void Create()
    {
        root = new GameObject("ChudOS Array List", typeof(RectTransform), typeof(Canvas));
        Object.DontDestroyOnLoad(root);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;
        header = CreateText("Header", 12f, 25f);
        entries = CreateText("Entries", 41f, 25f);
    }

    private static Text CreateText(string name, float y, float height)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        obj.transform.SetParent(root!.transform, false);
        var text = obj.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 15;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.UpperRight;
        text.lineSpacing = 1.2f;
        text.supportRichText = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        var rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-20f, -y);
        rect.sizeDelta = new Vector2(280f, height);
        return text;
    }

    public static void RefreshTheme()
    {
        if (header != null) header.color = ChudOS.Core.Main.PrimaryColorValue;
        if (entries != null) entries.color = ChudOS.Core.Main.PrimaryColorValue;
    }

    public static void Shutdown()
    {
        ButtonManager.OnButtonsChanged -= Refresh;
        TranslationManager.Changed -= Refresh;
        if (root != null) Object.Destroy(root);
        root = null;
        header = entries = null;
        names = Array.Empty<string>();
    }
}
