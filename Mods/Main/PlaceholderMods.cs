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
using ChudOS.Stuff;
using System.Diagnostics;


namespace ChudOS.Mods.Main;

public static class PlaceholderMods
{
    [MenuButton("Join Discord", ButtonType.Press, Category = "Main", Order = 0)]
    public static void JoinDiscord() => OpenLink("https://discord.gg/F6Et6BMQKw");

    [MenuButton("Menu Source", ButtonType.Press, Category = "Main", Order = 2)]
    public static void Source() => OpenLink("https://github.com/ASTRA228b/ChudOS");


    private static void OpenLink(string link)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = link,
            UseShellExecute = true
        });
    }
}
