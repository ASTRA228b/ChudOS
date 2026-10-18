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

public static class GlobalTex
{
    public static Texture2D MakeTex(int W, int H, Color C)
    {
        Texture2D T = new(W, H);
        T.SetPixel(0, 0, C);
        T.Apply();
        return T;
    }
}
