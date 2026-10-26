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

using BepInEx;
using ChudOS.Core;
using ChudOS.Core.Managers.CustomBoards;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS;

[BepInPlugin(Constantss.GUID, Constantss.Name, Constantss.Version)]
public sealed class Plugin : BaseUnityPlugin
{
    private GameObject? Runtime;

    private void Awake()
    {
        PatchLoader.Apply();
        Runtime = new GameObject(Constantss.ObjectName);
        Runtime.AddComponent<Changetext>();
        Runtime.AddComponent<Main>();
        Runtime.AddComponent<ChudOS.Mods.Room.RoomSession>();
        DontDestroyOnLoad(Runtime);
    }

    private void OnDestroy()
    {
        if (Runtime != null) Destroy(Runtime);
    }
}
