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

namespace ChudOS.Mods.Room;

public static class RoomMods
{
    private static readonly string[] Codes = { "1", "12", "1245", "1234", "LUCIO", "MODS", "MOD", "TTTPIG", "MODDING", "MENU", "GHGHFJSDJ", "HSDFHSD", "SDFSDDFS", "HHHHH", "GGG3IU9" };

    [MenuButton("Leave", ButtonType.Press, Category = "Room", Order = 0)]
    public static void Leave() => Room.Leave();

    [MenuButton("JoinRandom", ButtonType.Press, Category = "Room", Order = 1)]
    public static void JoinRando() => Room.JoinRandom();

    [MenuButton("JoinRandom Private", ButtonType.Press, Category = "Room", Order = 2, Description = "Joins a random private code that may or may not have people in it.")]
    public static void JRP()
    {
        string code = Codes[UnityEngine.Random.Range(0, Codes.Length)];
        Room.JoinRoom(code);
    }

    [MenuButton("EU", ButtonType.Press, Category = "Room", Order = 3)]
    public static void EU() => Room.ConnectToRegion("eu");

    [MenuButton("US", ButtonType.Press, Category = "Room", Order = 4)]
    public static void US() => Room.ConnectToRegion("us");

    [MenuButton("USW", ButtonType.Press, Category = "Room", Order = 5)]
    public static void USW() => Room.ConnectToRegion("usw");

    [MenuButton("Quit Game", ButtonType.Press, Category = "Room", Order = 6)]
    public static void KillGame() => Application.Quit();

    [MenuButton("Disable Network Triggers", ButtonType.Toggle, Category = "Room", Order = 7, runOption = RunOption.Once)]
    public static void DisableNetworkTriggers()
    {
        Room.IsEnabled = true;
    }

    [MenuDisable("Disable Network Triggers")]
    public static void EnableNetworkTriggers()
    {
        Room.IsEnabled = false;
    }
}
