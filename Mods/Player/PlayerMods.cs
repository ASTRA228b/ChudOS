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


using System.Collections.Generic;
using System.Linq;
using ChudOS.Libraries;
using ChudOS.Stuff;
using ChudOS.Core.Managers;
using GorillaGameModes;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;

namespace ChudOS.Mods.Player;

public static class PlayerMods
{
    private static VRRig? gunTarget;
    [MenuButton("Body Tracers", ButtonType.Toggle, Category = "Player", Order = 0, runOption = RunOption.LateUpdate, Description = "Draw local tracers from you to other players")]
    public static void BodyTracers() => PlayerESPRuntime.BodyTracers();

    [MenuDisable("Body Tracers")]
    public static void StopBodyTracers() => PlayerESPRuntime.ClearBodyTracers();

    [MenuButton("Bone ESP", ButtonType.Toggle, Category = "Player", Order = 1, runOption = RunOption.LateUpdate, Description = "Show other players' skeleton visuals")]
    public static void BoneESP() => PlayerESPRuntime.BoneESP();

    [MenuDisable("Bone ESP")]
    public static void StopBoneESP() => PlayerESPRuntime.ClearBoneESP();

    [MenuButton("2D Box ESP", ButtonType.Toggle, Category = "Player", Order = 2, runOption = RunOption.LateUpdate, Description = "Billboard outlines around players")]
    public static void BoxESP2D() => PlayerESPRuntime.BoxESP2D();

    [MenuDisable("2D Box ESP")]
    public static void StopBoxESP2D() => PlayerESPRuntime.ClearBoxESP2D();

    [MenuButton("3D Box ESP", ButtonType.Toggle, Category = "Player", Order = 3, runOption = RunOption.LateUpdate, Description = "Transparent 3D boxes around players")]
    public static void BoxESP3D() => PlayerESPRuntime.BoxESP3D();

    [MenuDisable("3D Box ESP")]
    public static void StopBoxESP3D() => PlayerESPRuntime.ClearBoxESP3D();

    [MenuButton("4D Box ESP", ButtonType.Toggle, Category = "Player", Order = 4, runOption = RunOption.LateUpdate, Description = "Thin transparent panels around players")]
    public static void BoxESP4D() => PlayerESPRuntime.BoxESP4D();

    [MenuDisable("4D Box ESP")]
    public static void StopBoxESP4D() => PlayerESPRuntime.ClearBoxESP4D();

    [MenuButton("Trails ESP", ButtonType.Toggle, Category = "Player", Order = 5, runOption = RunOption.LateUpdate, Description = "Show other players' recent movement trails")]
    public static void TrailsESP() => PlayerESPRuntime.TrailsESP();

    [MenuDisable("Trails ESP")]
    public static void StopTrailsESP() => PlayerESPRuntime.ClearTrailsESP();

    [MenuButton("Tag Gun", ButtonType.Toggle, Category = "Player", Order = 6, Description = "Select a player within normal hand reach; does not move your rig")]
    public static void TG()
    {
        if (!ChudOS.Mods.Safety.NetworkSafety.AllowTagMod("Tag Gun")) { GunLib.Hide(); return; }
        if (!PortTools.Ready || GorillaTagger.Instance.rightHandTransform == null) { GunLib.Hide(); return; }
        bool grip = InputLib.RightGrab, trigger = InputLib.RightTrigger;
        if (GunLib.UpdateRigGun(GorillaTagger.Instance.rightHandTransform, grip, trigger, ref gunTarget) && gunTarget != null)
            ChudOS.Mods.Safety.NetworkSafety.TryTag(gunTarget);
    }

    [MenuEnable("Tag Gun")]
    public static void TGStart()
    {
        ButtonManager.SetEnabled("Tag Self", false);
        ButtonManager.SetEnabled("Tag All", false);
        ButtonManager.SetEnabled("Tag Aura", false);
        PortTools.ClaimPointer("Tag Gun");
    }

    [MenuDisable("Tag Gun")]
    public static void TGStop() { gunTarget = null; GunLib.Cleanup(); }

    [MenuButton("Tag Self", ButtonType.Toggle, Category = "Player", Order = 7, Description = "Private modded tag room master only; does not teleport your rig")]
    public static void TagSelf()
    {
        if (!ChudOS.Mods.Safety.NetworkSafety.AllowTagMod("Tag Self")) return;
        ButtonManager.SetEnabled("Tag Self", false);
        if (NetworkSystem.Instance == null || VRRig.LocalRig == null || TagManager.IsTagged(VRRig.LocalRig)) return;
        if (!NetworkSystem.Instance.IsMasterClient || GorillaGameManager.instance is not GorillaTagManager)
        {
            ChudOS.Libraries.Notifications.NotifyLib.MessageToast("Tag Self requires the tag room master.", 3500);
            return;
        }
        if (NetworkSystem.Instance.LocalPlayer != null && ChudOS.Mods.Safety.NetworkSafety.TryRPC())
            TagManager.Instance.AddInfected(NetworkSystem.Instance.LocalPlayer);
    }

    [MenuEnable("Tag Self")]
    public static void TagSelfStart()
    {
        ButtonManager.SetEnabled("Tag Gun", false);
        ButtonManager.SetEnabled("Tag All", false);
        ButtonManager.SetEnabled("Tag Aura", false);
    }

    [MenuButton("Tag Aura", ButtonType.Toggle, Category = "Player", Order = 8, Description = "Attempt nearby tags within normal hand reach; does not increase tag distance")]
    public static void TagAura()
    {
        if (!ChudOS.Mods.Safety.NetworkSafety.AllowTagMod("Tag Aura")) return;
        TagNearby();
    }

    [MenuEnable("Tag Aura")]
    public static void TagAuraStart()
    {
        ButtonManager.SetEnabled("Tag Gun", false);
        ButtonManager.SetEnabled("Tag Self", false);
        ButtonManager.SetEnabled("Tag All", false);
    }

    [MenuButton("Tag All", ButtonType.Toggle, Category = "Player", Order = 9, Description = "One pass over players within normal hand reach; does not teleport your rig")]
    public static void TagAll()
    {
        if (!ChudOS.Mods.Safety.NetworkSafety.AllowTagMod("Tag All")) return;
        try { TagNearby(); }
        finally { ButtonManager.SetEnabled("Tag All", false); }
    }

    [MenuEnable("Tag All")]
    public static void TagAllStart()
    {
        ButtonManager.SetEnabled("Tag Gun", false);
        ButtonManager.SetEnabled("Tag Self", false);
        ButtonManager.SetEnabled("Tag Aura", false);
    }

    private static void TagNearby()
    {
        if (VRRigCache.ActiveRigs == null) return;
        foreach (VRRig rig in VRRigCache.ActiveRigs)
            if (rig != null) ChudOS.Mods.Safety.NetworkSafety.TryTag(rig);
    }
}
