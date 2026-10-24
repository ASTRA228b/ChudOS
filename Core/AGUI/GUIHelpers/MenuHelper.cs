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
using ChudOS.Core.Managers;

namespace ChudOS.Core.AGUI.GUIHelpers;

public static class MenuHelper
{
    public static Rect MakeWindow(int id, Rect window, GUI.WindowFunction createGUI, string title, params GUILayoutOption[] options)
    {
        GlobalStyles.INIT();
        return GUILayout.Window(id, window, createGUI, TranslationManager.UI(title), GlobalStyles.WindowStyle, options);
    }

    public static void Label(string text) => GUILayout.Label(TranslationManager.UI(text));
    public static bool Button(string text)
    {
        bool pressed = GUILayout.Button(TranslationManager.UI(text), GlobalStyles.Buttonss);
        if (pressed) MenuFeedback.Play();
        return pressed;
    }
    public static void Space(float space = 6f) => GUILayout.Space(space);
    public static bool Toggle(string text, bool state)
    {
        bool value = GUILayout.Toggle(state, TranslationManager.UI(text));
        if (value != state) MenuFeedback.Play();
        return value;
    }
    public static int Toolbar(int selected, string[] tabs)
    {
        int value = GUILayout.Toolbar(selected, tabs, GlobalStyles.Buttonss);
        if (value != selected) MenuFeedback.Play();
        return value;
    }
    public static float Slider(float current, float min, float max)
    {
        float value = GUILayout.HorizontalSlider(current, min, max, GlobalStyles.SliderStyle, GlobalStyles.SliderThumbStyle);
        if (value != current) MenuFeedback.Play();
        return value;
    }
}
