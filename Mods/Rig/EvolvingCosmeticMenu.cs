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

using ChudOS.Stuff;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Rig;

public static class EvolvingCosmeticMenu
{
    [MenuButton("Evolving Cosmetic", ButtonType.Press, Category = "Rig", Order = 17, Description = "Manage stages, local age and stage cycling for your worn evolving cosmetics")]
    public static void Open()
    {
        ChudOS.Core.Main main = Object.FindFirstObjectByType<ChudOS.Core.Main>();
        if (main == null) return;
        EvolvingCosmeticManager manager = main.GetComponent<EvolvingCosmeticManager>() ?? main.gameObject.AddComponent<EvolvingCosmeticManager>();
        manager.Open();
    }
}
