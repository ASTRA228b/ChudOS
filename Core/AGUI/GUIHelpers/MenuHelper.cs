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

public static class MenuHelper
{
    public static void Label(string text) => GUILayout.Label(text);
    public static bool Button(string text) => GUILayout.Button(text, GlobalStyles.Buttonss);
    public static void Space(float space = 6f) => GUILayout.Space(space);
    public static bool Toggle(string text, bool state) => GUILayout.Toggle(state, text);
    public static float Slider(float current, float min, float max) => GUILayout.HorizontalSlider(current, min, max, GlobalStyles.SliderStyle, GlobalStyles.SliderThumbStyle);
}
