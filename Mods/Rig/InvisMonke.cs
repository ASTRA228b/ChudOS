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

namespace ChudOS.Mods.Rig;

internal sealed class InvisMonke : RigBehaviour
{
    protected override void Update()
    {
        if (!RigInput.Instance.RightSecondary.WasPressed)
            return;

        RigRuntime.ToggleRig(!RigRuntime.IsRigEnabled, new Vector3(999f, 999f, 999f));
    }

    protected override void OnDisable() => RigRuntime.ToggleRig(true);
}