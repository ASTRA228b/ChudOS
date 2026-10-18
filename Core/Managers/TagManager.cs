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

using Photon.Pun;

namespace ChudOS.Core.Managers;

public sealed class TagManager
{
    public static TagManager Instance { get; } = new();
    private readonly HashSet<VRRig> taggedRigs = new();
    private TagManager() { }

    public HashSet<VRRig> TaggedRigs
    {
        get
        {
            taggedRigs.Clear();
            if (VRRigCache.ActiveRigs != null)
            {
                foreach (VRRig rig in VRRigCache.ActiveRigs)
                {
                    if (IsTagged(rig)) taggedRigs.Add(rig);
                }
            }
            return taggedRigs;
        }
    }

    public static bool IsTagged(VRRig? rig)
    {
        if (rig == null) return false;
        return rig.setMatIndex == 1 || rig.setMatIndex == 2 || rig.setMatIndex == 11;
    }

    public void AddInfected(NetPlayer player)
    {
        if (player == null || !PhotonNetwork.InRoom || NetworkSystem.Instance == null || !NetworkSystem.Instance.IsMasterClient) return;
        GorillaTagManager? manager = GorillaGameManager.instance as GorillaTagManager;
        if (manager == null) return;
        if (manager.isCurrentlyTag)
        {
            if (manager.currentIt != player)
            {
                var changeCurrentItMethod = typeof(GorillaTagManager).GetMethod("ChangeCurrentIt", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                changeCurrentItMethod?.Invoke(manager, new object[] { player, true });
            }
        }
        else if (manager.currentInfected != null && !manager.currentInfected.Contains(player))
        {
            manager.AddInfectedPlayer(player);
        }
    }
}
