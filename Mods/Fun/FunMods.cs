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

using ChudOS.Core.Managers;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using ChudOS.Libraries;
using UnityEngine;

namespace ChudOS.Mods.Fun;

public static class FunMods
{
    public static float LastBangTime;
    public static readonly float BPM = 159;
    private static VRRig? headOwner;
    private static float oldHeadPitch;
    private static VRRig? offsetOwner;
    private static Vector3 originalOffset;
    private static NetworkSystem? headNetwork;
    private static Vector3 headGoal;
    private static int lastHeadFrame = -1;
    private static float spinSpeed = 60f;

    [MenuButton("UpsideDown Head", ButtonType.Press, Category = "Detected", Order = 0, Description = "Smoothly rotate your head upside down")]
    public static void UpsideDownHead() => SetHeadAxis(2, 180f);

    [MenuButton("Broken Neck", ButtonType.Press, Category = "Detected", Order = 1, Description = "Smoothly roll your head sideways")]
    public static void BrokenNeck() => SetHeadAxis(2, 90f);

    [MenuButton("Backwards Head", ButtonType.Press, Category = "Detected", Order = 2, Description = "Smoothly turn your head backwards")]
    public static void BackwardsHead() => SetHeadAxis(1, 180f);

    [MenuButton("Sideways Head", ButtonType.Press, Category = "Detected", Order = 3, Description = "Smoothly turn your head sideways")]
    public static void SidewaysHead() => SetHeadAxis(1, 90f);

    [MenuButton("HeadBang", ButtonType.Toggle, Category = "Detected", Order = 4, Description = "Smooth head animation with a bounded rotation speed")]
    public static void HeadBang()
    {
        if (!PrepareHead()) return;
        if (headOwner != VRRig.LocalRig)
        {
            StopHeadBang();
            headOwner = VRRig.LocalRig;
            oldHeadPitch = headOwner.head.trackingRotationOffset.x;
            LastBangTime = Time.unscaledTime;
            ChudOS.Libraries.PortTools.Register(StopHeadBang);
        }
        float phase = (Time.unscaledTime - LastBangTime) * (BPM / 60f) * Mathf.PI * 2f;
        headGoal.x = HeadPoseMath.Wrap(oldHeadPitch + Mathf.Sin(phase) * 20f);
    }

    [MenuDisable("HeadBang")]
    public static void StopHeadBang()
    {
        if (headOwner != null)
        {
            headOwner.head.trackingRotationOffset.x = HeadPoseMath.Wrap(oldHeadPitch);
            headGoal.x = HeadPoseMath.Wrap(oldHeadPitch);
        }
        headOwner = null;
        LastBangTime = 0f;
    }


    [MenuButton("Fix Head", ButtonType.Press, Category = "Fun", Order = 5)]
    public static void FixHead()
    {
        RestoreHeadOffsets();
        if (VRRig.LocalRig == null) return;
        VRRig.LocalRig.head.trackingRotationOffset.x = 0f;
        VRRig.LocalRig.head.trackingRotationOffset.y = 0f;
        VRRig.LocalRig.head.trackingRotationOffset.z = 0f;
    }

    [MenuButton("Fix All", ButtonType.Press, Category = "Fun", Order = 6, Description = "Fixes HeadPos and Handtap sounds")]
    public static void FixAll()
    {
        FixHead();
        if (GorillaTagger.Instance != null) GorillaTagger.Instance.handTapVolume = 0.1f;
    }

    [MenuButton("Head Spin Yaw", ButtonType.Toggle, Category = "Rig", Order = 40)]
    public static void SpinYaw() => Spin(1);
    [MenuButton("Head Spin Pitch", ButtonType.Toggle, Category = "Rig", Order = 41)]
    public static void SpinPitch() => Spin(0);
    [MenuButton("Head Spin Roll", ButtonType.Toggle, Category = "Rig", Order = 42)]
    public static void SpinRoll() => Spin(2);
    [MenuDisable("Head Spin Yaw")]
    public static void StopYaw() => ResetHeadState();
    [MenuDisable("Head Spin Pitch")]
    public static void StopPitch() => ResetHeadState();
    [MenuDisable("Head Spin Roll")]
    public static void StopRoll() => ResetHeadState();
    [MenuButton("Head Spin Speed", ButtonType.Option, Category = "Settings", Order = 246, Options = new[] { "60", "30", "90", "120" }, Notify = false)]
    public static void SpinSpeed() => spinSpeed = new[] { 60f, 30f, 90f, 120f }[ButtonManager.GetButton("Head Spin Speed")!.OptionIndex];
    private static void Spin(int axis)
    {
        if (!PrepareHead()) return;
        headGoal[axis] = HeadPoseMath.Wrap(headGoal[axis] + spinSpeed * Mathf.Min(Time.unscaledDeltaTime, 0.05f));
    }

    private static bool PrepareHead()
    {
        if (VRRig.LocalRig == null) return false;
        if (offsetOwner != VRRig.LocalRig)
        {
            if (offsetOwner != null) RestoreHeadOffsets();
            offsetOwner = VRRig.LocalRig;
            originalOffset = offsetOwner.head.trackingRotationOffset;
            originalOffset = new Vector3(HeadPoseMath.Wrap(originalOffset.x), HeadPoseMath.Wrap(originalOffset.y), HeadPoseMath.Wrap(originalOffset.z));
            headGoal = originalOffset;
            lastHeadFrame = -1;
            PortTools.Register(RestoreHeadOffsets, MaintainHead);
        }
        MaintainHead();
        return offsetOwner != null;
    }

    private static void SetHeadAxis(int axis, float angle)
    {
        ButtonManager.SetEnabled("Head Spin Yaw", false);
        ButtonManager.SetEnabled("Head Spin Pitch", false);
        ButtonManager.SetEnabled("Head Spin Roll", false);
        if (!PrepareHead()) return;
        headGoal[axis] = HeadPoseMath.Wrap(angle);
    }

    private static void MaintainHead()
    {
        if (offsetOwner == null) return;
        if (headNetwork != NetworkSystem.Instance)
        {
            if (headNetwork != null) headNetwork.OnJoinedRoomEvent -= RestoreHeadOffsets;
            headNetwork = NetworkSystem.Instance;
            if (headNetwork != null) headNetwork.OnJoinedRoomEvent += RestoreHeadOffsets;
        }
        if (offsetOwner != VRRig.LocalRig) { RestoreHeadOffsets(); return; }
        if (lastHeadFrame == Time.frameCount) return;
        lastHeadFrame = Time.frameCount;
        float step = 120f * Mathf.Clamp(Time.unscaledDeltaTime, 0f, 0.05f);
        Vector3 current = offsetOwner.head.trackingRotationOffset;
        offsetOwner.head.trackingRotationOffset = new Vector3(HeadPoseMath.Step(current.x, headGoal.x, step), HeadPoseMath.Step(current.y, headGoal.y, step), HeadPoseMath.Step(current.z, headGoal.z, step));
    }

    private static void RestoreHeadOffsets()
    {
        PortTools.RemoveTick(MaintainHead);
        ButtonManager.SetEnabled("Head Spin Yaw", false);
        ButtonManager.SetEnabled("Head Spin Pitch", false);
        ButtonManager.SetEnabled("Head Spin Roll", false);
        ButtonManager.SetEnabled("HeadBang", false);
        ResetHeadState();
    }

    private static void ResetHeadState()
    {
        PortTools.RemoveTick(MaintainHead);
        StopHeadBang();
        if (offsetOwner != null) offsetOwner.head.trackingRotationOffset = originalOffset;
        offsetOwner = null;
        if (headNetwork != null) headNetwork.OnJoinedRoomEvent -= RestoreHeadOffsets;
        headNetwork = null;
    }
}
