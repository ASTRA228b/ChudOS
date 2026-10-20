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

using ChudOS.Libraries;
using ChudOS.Core.Managers;
using ChudOS.Libraries.Notifications;
using Photon.Pun;
using ChudOS.Stuff;

namespace ChudOS.Mods.Rig;

public static class MaxQuestScore
{
    private static bool enabled;
    private static NetworkSystem? subscribed;
    private static VRRig? scoreRig;
    private static int originalScore;

    [MenuButton("Max Quest Score", ButtonType.Toggle, Category = "Rig", Order = 16, runOption = RunOption.Once, Description = "Sets your rig's displayed quest score while enabled")]
    public static void Enable()
    {
        enabled = true;
        PortTools.Register(Cleanup, Maintain);
        Maintain();
    }

    private static void Maintain()
    {
        if (!enabled) return;
        if (subscribed != NetworkSystem.Instance)
        {
            if (subscribed != null) subscribed.OnJoinedRoomEvent -= Apply;
            subscribed = NetworkSystem.Instance;
            if (subscribed != null) subscribed.OnJoinedRoomEvent += Apply;
        }
        if (VRRig.LocalRig != null && scoreRig != VRRig.LocalRig) Apply();
    }

    private static void Apply()
    {
        if (!enabled || VRRig.LocalRig == null) return;
        if (PhotonNetwork.InRoom)
        {
            ButtonManager.SetEnabled("Max Quest Score", false);
            NotifyLib.MessageToast("Max Quest Score is an offline display option.", 3500);
            return;
        }
        if (scoreRig != VRRig.LocalRig)
        {
            RestoreScore();
            scoreRig = VRRig.LocalRig;
            originalScore = scoreRig.GetCurrentQuestScore();
        }
        scoreRig.SetQuestScore(int.MaxValue);
    }

    private static void RestoreScore()
    {
        if (scoreRig != null) scoreRig.SetQuestScore(originalScore);
        scoreRig = null;
    }

    [MenuDisable("Max Quest Score")]
    public static void Disable() => Cleanup();

    private static void Cleanup()
    {
        enabled = false;
        if (subscribed != null) subscribed.OnJoinedRoomEvent -= Apply;
        subscribed = null;
        RestoreScore();
    }
}
