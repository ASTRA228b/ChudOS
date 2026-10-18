using ChudOS.Core.Managers;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Fun;

public static class FunMods
{
    public static float LastBangTime;
    public static readonly float BPM = 159;

    [MenuButton("UpsideDown Head", ButtonType.Press, Category = "Fun", Order = 0)]
    public static void UpsideDownHead() => VRRig.LocalRig.head.trackingRotationOffset.z = 180f;

    [MenuButton("Broken Neck", ButtonType.Press, Category = "Fun", Order = 1)]
    public static void BrokenNeck() => VRRig.LocalRig.head.trackingRotationOffset.z = 90f;

    [MenuButton("Backwards Head", ButtonType.Press, Category = "Fun", Order = 2)]
    public static void BackwardsHead() => VRRig.LocalRig.head.trackingRotationOffset.y = 180f;

    [MenuButton("Sideways Head", ButtonType.Press, Category = "Fun", Order = 3)]
    public static void SidewaysHead() => VRRig.LocalRig.head.trackingRotationOffset.y = 90f;

    [MenuButton("HeadBang", ButtonType.Toggle, Category = "Fun", Order = 4)]
    public static void HeadBang()
    {
        if (Time.time > LastBangTime)
        {
            VRRig.LocalRig.head.trackingRotationOffset.x = 50f;
            LastBangTime = Time.time + 60f / BPM;
        }
        else
            VRRig.LocalRig.head.trackingRotationOffset.x = Mathf.Lerp(VRRig.LocalRig.head.trackingRotationOffset.x, 0f, 0.1f);
    }


    [MenuButton("Fix Head", ButtonType.Press, Category = "Fun", Order = 5)]
    public static void FixHead()
    {
        VRRig.LocalRig.head.trackingRotationOffset.x = 0f;
        VRRig.LocalRig.head.trackingRotationOffset.y = 0f;
        VRRig.LocalRig.head.trackingRotationOffset.z = 0f;
    }

    [MenuButton("Fix All", ButtonType.Press, Category = "Fun", Order = 6, Description = "Fixes HeadPos and Handtap sounds")]
    public static void FixAll()
    {
        VRRig.LocalRig.head.trackingRotationOffset.x = 0f;
        VRRig.LocalRig.head.trackingRotationOffset.y = 0f;
        VRRig.LocalRig.head.trackingRotationOffset.z = 0f;
        GorillaTagger.Instance.handTapVolume = 0.1f;
    }
}
