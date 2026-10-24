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

namespace ChudOS.Mods.Rig;

internal sealed class CopyMovementGun : RigBehaviour
{
    private VRRig? target;
    private bool posed;

    protected override void LateUpdate()
    {
        if (!GunLib.UpdateRigGun(GunLib.AimHand, GunLib.AimPressed, GunLib.SelectPressed, ref target) || target == null)
        {
            if (posed) { RigRuntime.RestorePose(); posed = false; }
            return;
        }

        RigRuntime.ToggleRig(false);
        posed = true;
        VRRig local = VRRig.LocalRig;
        local.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
        local.head.rigTarget.SetPositionAndRotation(target.head.rigTarget.position, target.head.rigTarget.rotation);
        local.leftHand.rigTarget.SetPositionAndRotation(target.leftHand.rigTarget.position, target.leftHand.rigTarget.rotation);
        local.rightHand.rigTarget.SetPositionAndRotation(target.rightHand.rigTarget.position, target.rightHand.rigTarget.rotation);
        local.leftHand.calcT = target.leftHand.calcT;
        local.rightHand.calcT = target.rightHand.calcT;
    }

    protected override void OnDisable()
    {
        target = null;
        posed = false;
        GunLib.Hide();
        RigRuntime.ToggleRig(true);
    }
}
