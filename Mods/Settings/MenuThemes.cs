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

using ChudOS.Core.Managers;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Settings;

public static class MenuThemes
{
    public sealed class Theme
    {
        public Color Background = new Color32(12, 11, 16, 255);
        public Color Face = new Color32(14, 13, 17, 255);
        public Color Button = new Color32(30, 30, 33, 255);
        public Color Header = new Color32(31, 31, 34, 255);
        public Color Accent = new Color32(155, 18, 254, 255);
        public Color Border = new Color32(164, 30, 255, 255);
        public Color Divider = new Color32(155, 18, 254, 255);
        public Color SearchBorder = new Color32(69, 70, 75, 255);
        public Color Text = Color.white;
        public Color SecondaryText = new Color32(142, 145, 154, 255);
        public bool TextOnly, OutlineButtons;
    }

    private static Color RGB(byte r, byte g, byte b) => new Color32(r, g, b, 255);

    private static Theme AccentTheme(Color accent) => new()
    {
        Accent = accent,
        Border = accent,
        Divider = accent
    };

    private static readonly Theme[] Themes =
    {
        new(),
        AccentTheme(RGB(50, 140, 255)),
        AccentTheme(RGB(255, 65, 85)),
        new()
        {
            Background = RGB(5, 9, 18),
            Face = RGB(10, 17, 30),
            Button = RGB(19, 32, 50),
            Header = RGB(15, 26, 43),
            Accent = RGB(40, 200, 255),
            Border = RGB(40, 200, 255),
            Divider = RGB(40, 200, 255),
            SearchBorder = RGB(38, 68, 90),
            Text = RGB(230, 245, 255),
            SecondaryText = RGB(130, 160, 185)
        },
        new()
        {
            Background = RGB(7, 14, 10),
            Face = RGB(12, 23, 16),
            Button = RGB(23, 41, 29),
            Header = RGB(18, 33, 23),
            Accent = RGB(75, 210, 120),
            Border = RGB(75, 210, 120),
            Divider = RGB(75, 210, 120),
            SearchBorder = RGB(48, 79, 57),
            Text = RGB(230, 250, 235),
            SecondaryText = RGB(140, 175, 150)
        },
        new()
        {
            Background = RGB(20, 9, 15),
            Face = RGB(30, 14, 24),
            Button = RGB(52, 25, 38),
            Header = RGB(43, 20, 32),
            Accent = RGB(255, 135, 65),
            Border = RGB(255, 100, 105),
            Divider = RGB(255, 135, 65),
            SearchBorder = RGB(100, 55, 70),
            Text = RGB(255, 240, 225),
            SecondaryText = RGB(195, 150, 150)
        },
        new()
        {
            Background = RGB(190, 205, 220),
            Face = RGB(225, 235, 245),
            Button = RGB(195, 210, 225),
            Header = RGB(205, 220, 235),
            Accent = RGB(95, 175, 245),
            Border = RGB(65, 130, 195),
            Divider = RGB(65, 130, 195),
            SearchBorder = RGB(140, 165, 190),
            Text = RGB(20, 35, 50),
            SecondaryText = RGB(65, 85, 105)
        },
        new() { TextOnly = true },
        new()
        {
            Background = RGB(9, 18, 24), Face = RGB(13, 25, 32), Button = RGB(23, 40, 49), Header = RGB(18, 33, 42),
            Accent = RGB(100, 220, 255), Border = RGB(100, 220, 255), Divider = RGB(100, 220, 255),
            Text = RGB(235, 250, 255), SecondaryText = RGB(160, 195, 210)
        },
        new()
        {
            Background = RGB(20, 11, 8), Face = RGB(28, 17, 12), Button = RGB(49, 29, 19),
            Accent = RGB(255, 145, 45), Border = RGB(255, 145, 45), Divider = RGB(255, 145, 45),
            Text = RGB(255, 245, 230), SecondaryText = RGB(205, 170, 135)
        },
        new()
        {
            Background = RGB(22, 12, 21), Face = RGB(31, 18, 29), Button = RGB(50, 29, 46),
            Accent = RGB(255, 145, 195), Border = RGB(255, 145, 195), Divider = RGB(255, 145, 195),
            Text = RGB(255, 240, 248), SecondaryText = RGB(205, 165, 190)
        },
        new()
        {
            Background = RGB(8, 20, 17), Face = RGB(12, 29, 24), Button = RGB(21, 45, 37),
            Accent = RGB(105, 240, 185), Border = RGB(105, 240, 185), Divider = RGB(105, 240, 185),
            Text = RGB(235, 255, 245), SecondaryText = RGB(150, 200, 175)
        },
        new()
        {
            Background = RGB(8, 12, 24), Face = RGB(13, 19, 34), Button = RGB(24, 32, 49),
            Accent = RGB(235, 190, 80), Border = RGB(235, 190, 80), Divider = RGB(235, 190, 80),
            Text = RGB(255, 248, 225), SecondaryText = RGB(180, 180, 165)
        },
        new()
        {
            Background = RGB(5, 12, 9), Face = RGB(7, 17, 12), Button = RGB(12, 29, 20), Header = RGB(10, 24, 17),
            Accent = RGB(35, 230, 115), Border = RGB(55, 255, 145), Divider = RGB(55, 255, 145),
            SearchBorder = RGB(55, 255, 145), Text = RGB(235, 255, 240), SecondaryText = RGB(150, 210, 175), OutlineButtons = true
        }
    };

    [MenuButton("Menu Theme", ButtonType.Option, Category = "Settings", Order = 30, Options = new[] { "OG", "OG Blue", "OG Red", "Midnight", "Forest", "Sunset", "Ice", "Text Only", "Arctic", "Ember", "Sakura", "Mint", "Midnight Gold", "Neon Outline" }, Notify = false)]
    public static void Apply()
    {
        int index = ButtonManager.GetButton("Menu Theme")?.OptionIndex ?? 0;
        ChudOS.Core.Main.ApplyMenuTheme(Themes[Mathf.Clamp(index, 0, Themes.Length - 1)]);
    }

    [MenuButton("Reset Menu Theme", ButtonType.Press, Category = "Settings", Order = 31, Notify = false)]
    public static void Reset()
    {
        var button = ButtonManager.GetButton("Menu Theme");
        if (button != null && button.OptionIndex != 0)
            ButtonManager.ChangeOption(button, -button.OptionIndex);
        else
            Apply();
    }
}
