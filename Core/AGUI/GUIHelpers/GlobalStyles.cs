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

namespace ChudOS.Core.AGUI.GUIHelpers;

public static class GlobalStyles
{
    public static GUIStyle? WindowStyle, Buttonss, SliderStyle, SliderThumbStyle, LabelStyle;
    private static Texture2D? WindowTex, ButtonTex, SliderTex, ThumbTex, PressedTex;
    private static ChudOS.Mods.Settings.MenuThemes.Theme? pendingTheme;
    private static bool paletteDirty;
    private static readonly Color WindowColor = new(0.07f, 0.05f, 0.11f);
    private static readonly Color ButtonColor = new(0.17f, 0.10f, 0.27f);
    private static readonly Color SliderColor = new(0.13f, 0.09f, 0.20f);
    private static readonly Color ThumbColor = new(0.47f, 0.25f, 0.79f);

    public static void INIT()
    {
        if (WindowStyle != null) { RefreshPalette(); return; }
        WindowTex = GlobalTex.MakeTex(1, 1, WindowColor);
        ButtonTex = GlobalTex.MakeTex(1, 1, ButtonColor);
        SliderTex = GlobalTex.MakeTex(1, 1, SliderColor);
        ThumbTex = GlobalTex.MakeTex(1, 1, ThumbColor);
        PressedTex = GlobalTex.MakeTex(1, 1, ThumbColor);
        LabelStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
        WindowStyle = new GUIStyle(GUI.skin.window);
        Buttonss = new GUIStyle(GUI.skin.button);
        SliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
        SliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
        ApplyBackground(WindowStyle, WindowTex);
        ApplyBackground(Buttonss, ButtonTex);
        ApplyBackground(SliderStyle, SliderTex);
        ApplyBackground(SliderThumbStyle, ThumbTex);
        WindowStyle.normal.textColor = Color.white;
        Buttonss.normal.textColor = Color.white;
        Buttonss.hover.textColor = Color.white;
        Buttonss.active.textColor = Color.white;
        Buttonss.fixedHeight = 27f;
        RefreshPalette();
    }

    public static void SetTheme(ChudOS.Mods.Settings.MenuThemes.Theme theme)
    {
        pendingTheme = theme;
        paletteDirty = true;
    }
    private static void RefreshPalette()
    {
        if (!paletteDirty || pendingTheme == null || WindowStyle == null) return;
        var theme = pendingTheme;
        SetTexture(WindowTex!, theme.Background); SetTexture(ButtonTex!, theme.Button); SetTexture(SliderTex!, theme.Header);
        SetTexture(ThumbTex!, theme.Accent); SetTexture(PressedTex!, theme.Pressed ?? theme.Accent);
        WindowStyle.normal.textColor = theme.Text; WindowStyle.focused.textColor = theme.Text;
        LabelStyle!.normal.textColor = theme.Text;
        foreach (var state in new[] { Buttonss!.normal, Buttonss.hover, Buttonss.active, Buttonss.focused, Buttonss.onNormal, Buttonss.onHover, Buttonss.onActive, Buttonss.onFocused }) state.textColor = theme.Text;
        Buttonss.active.background = PressedTex; Buttonss.onActive.background = PressedTex;
        paletteDirty = false;
    }
    private static void SetTexture(Texture2D texture, Color color)
    {
        texture.SetPixel(0, 0, color);
        texture.Apply(false, false);
    }
    private static void ApplyBackground(GUIStyle style, Texture2D tex)
    {
        style.normal.background = tex;
        style.hover.background = tex;
        style.active.background = tex;
        style.focused.background = tex;
        style.onNormal.background = tex;
        style.onHover.background = tex;
        style.onActive.background = tex;
        style.onFocused.background = tex;
    }

    public static void Cleanup()
    {
        foreach (Texture2D? tex in new[] { WindowTex, ButtonTex, SliderTex, ThumbTex, PressedTex }) if (tex != null) GameObject.Destroy(tex);
        WindowStyle = Buttonss = SliderStyle = SliderThumbStyle = LabelStyle = null;
        WindowTex = ButtonTex = SliderTex = ThumbTex = PressedTex = null;
        paletteDirty = true;
    }
}
