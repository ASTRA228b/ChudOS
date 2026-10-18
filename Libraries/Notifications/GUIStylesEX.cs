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

namespace ChudOS.Libraries.Notifications;

public static class GUIStylesEX
{
    public static GUIStyle ToastBackground { get; private set; } = null!;
    public static GUIStyle AccentBar { get; private set; } = null!;
    public static GUIStyle ToastLabel { get; private set; } = null!;
    private static bool initialized;

    public static void Init()
    {
        if (initialized)
            return;

        ToastBackground = new GUIStyle(GUI.skin.box)
        {
            border = new RectOffset(12, 12, 12, 12),
            padding = new RectOffset(12, 12, 12, 12)
        };
        ToastBackground.normal.background = MakeRoundedTexture(32, 32, new Color32(22, 28, 39, 245), 12);
        AccentBar = new GUIStyle(GUI.skin.box)
        {
            border = new RectOffset(6, 6, 6, 6)
        };
        AccentBar.normal.background = MakeRoundedTexture(16, 32, new Color32(0, 170, 255, 255), 6);
        ToastLabel = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 14,
            wordWrap = false
        };
        ToastLabel.normal.textColor = Color.white;
        initialized = true;
    }

    private static Texture2D MakeRoundedTexture(int width, int height, Color color, int radius)
    {
        Texture2D texture = new(width, height)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        Color clear = Color.clear;
        float radiusSquared = radius * radius;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inside = true;
                if (x < radius && y < radius)
                {
                    inside = new Vector2(x - radius, y - radius).sqrMagnitude <= radiusSquared;
                }
                else if (x >= width - radius && y < radius)
                {
                    inside = new Vector2(x - (width - radius - 1), y - radius).sqrMagnitude <= radiusSquared;
                }
                else if (x < radius && y >= height - radius)
                {
                    inside = new Vector2(x - radius, y - (height - radius - 1)).sqrMagnitude <= radiusSquared;
                }
                else if (x >= width - radius && y >= height - radius)
                {
                    inside = new Vector2(x - (width - radius - 1), y - (height - radius - 1)).sqrMagnitude <= radiusSquared;
                }

                texture.SetPixel(x, y, inside ? color : clear);
            }
        }

        texture.Apply();
        return texture;
    }
}
