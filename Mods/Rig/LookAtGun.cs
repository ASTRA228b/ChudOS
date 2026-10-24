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

internal sealed class LookAtGun : RigBehaviour
{
    private VRRig? target;
    private bool looking;

    protected override void LateUpdate()
    {
        if (GunLib.UpdateRigGun(GunLib.AimHand, GunLib.AimPressed, GunLib.SelectPressed, ref target) && target != null)
        {
            VRRig.LocalRig.head.rigTarget.LookAt(target.head.rigTarget);
            looking = true;
        }
        else if (looking) { RigRuntime.RestorePose(); looking = false; }
    }

    protected override void OnDisable()
    {
        target = null;
        looking = false;
        GunLib.Hide();
    }
}
