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

using System;
using HarmonyLib;
using UnityEngine;

namespace ChudOS.Mods.Rig;

internal sealed class RigTweaksV2 : RigBehaviour
{
    public static bool IsEnabled;
    public static Quaternion RigRotation = Quaternion.identity;

    protected override void Update()
    {
        if (!IsEnabled)
            return;

        float speed = 90f;

        if (RigKeys.Held(KeyCode.UpArrow) || RigInput.Instance.LeftJoystick.Axis.y > 0.5f)
            RigRotation *= Quaternion.Euler(speed * Time.deltaTime, 0, 0);

        if (RigKeys.Held(KeyCode.DownArrow) || RigInput.Instance.LeftJoystick.Axis.y < -0.5f)
            RigRotation *= Quaternion.Euler(-speed * Time.deltaTime, 0, 0);

        if (RigKeys.Held(KeyCode.LeftArrow) || RigInput.Instance.LeftJoystick.Axis.x < -0.5f)
            RigRotation *= Quaternion.Euler(0, speed * Time.deltaTime, 0);

        if (RigKeys.Held(KeyCode.RightArrow) || RigInput.Instance.LeftJoystick.Axis.x > 0.5f)
            RigRotation *= Quaternion.Euler(0, -speed * Time.deltaTime, 0);

        if (RigKeys.Held(KeyCode.RightControl) || RigInput.Instance.RightJoystick.Axis.x > 0.5f)
            RigRotation *= Quaternion.Euler(0, 0, speed * Time.deltaTime);

        if (RigKeys.Held(KeyCode.RightShift) || RigInput.Instance.RightJoystick.Axis.x < -0.5f)
            RigRotation *= Quaternion.Euler(0, 0, -speed * Time.deltaTime);

        if (RigKeys.Pressed(KeyCode.R))
            RigRotation = Quaternion.identity;
    }

    protected override void OnEnable() { RigRotation = Quaternion.identity; IsEnabled = true; }

    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public static class RigTweaksV2_LateUpdatePatch
{
    private static void Postfix(VRRig __instance)
    {
        try
        {
            if (!RigTweaksV2.IsEnabled || !RigRuntime.Ready || GorillaTagger.Instance.headCollider == null)
                return;

            if (!__instance.IsLocalRig())
                return;

            __instance.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation * RigTweaksV2.RigRotation;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LateUpdate postfix error: {e}");
        }
    }
}
