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
[System.Serializable]
internal class GUISettingsData
{
    public Color WindowColor = new(0.07f, 0.05f, 0.11f, 1f);
    public Color ButtonColor = new(0.17f, 0.10f, 0.27f, 1f);
    public Color SliderTrackColor = new(0.13f, 0.09f, 0.20f, 1f);
    public Color SliderThumbColor = new(0.47f, 0.25f, 0.79f, 1f);
}
