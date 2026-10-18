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

using HarmonyLib;
using UnityEngine;

namespace ChudOS.Mods.Rig;

internal sealed class SpinBot : RigBehaviour
{
    public static bool IsEnabled;

    protected override void OnEnable() => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public static class SpinBotPatch
{
    private static float yRot;

    private static void Postfix(VRRig __instance)
    {
        if (!SpinBot.IsEnabled || !__instance.IsLocalRig())
            return;

        yRot = (yRot + 1350 * Time.deltaTime) % 360f;

        __instance.transform.rotation = Quaternion.Euler(0, yRot, 0);
    }
}