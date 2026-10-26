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

using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace ChudOS.Mods.Room;

public class Room : MonoBehaviourPunCallbacks // room helpers
{
    private static string? pendingRegion;
    public static bool IsEnabled;

    public static void JoinRandom() => RoomSession.JoinRandom();

    public static void Leave()
    {
        RoomSession.Cancel();
        PhotonNetwork.Disconnect();
    }

    public static void JoinRoom(string Name) => RoomSession.JoinRoom(Name);

    public static void ConnectToRegion(string region)
    {
        if (string.IsNullOrEmpty(region))
            return;

        string? currentRegion = PhotonNetwork.CloudRegion?.Split('/')[0];

        if (currentRegion == region)
            return;

        pendingRegion = region;

        if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Disconnected)
        {
            pendingRegion = null;
            PhotonNetwork.ConnectToRegion(region);
        }
        else
            PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        if (string.IsNullOrEmpty(pendingRegion))
            return;

        string region = pendingRegion;
        pendingRegion = null;

        PhotonNetwork.ConnectToRegion(region);
    }
}

[HarmonyPatch(typeof(GorillaNetworkJoinTrigger), nameof(GorillaNetworkJoinTrigger.OnBoxTriggered))]
public static class NoNetworkTriggersPatch
{
    private static bool Prefix() => !Room.IsEnabled;
}