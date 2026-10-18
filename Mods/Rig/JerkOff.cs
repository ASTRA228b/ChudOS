using GorillaLocomotion;
using HarmonyLib;
using UnityEngine;

namespace ChudOS.Mods.Rig;

internal sealed class JerkOff : RigBehaviour
{
    public static JerkOff? Instance { get; private set; }
    protected override void Start() => Instance = this;
    protected override void OnDisable() => Instance = null;

    protected override void Update()
    {
        Transform jonklerHand = GTPlayer.Instance.RightHand.controllerTransform;
        Vector3 basePos = GTPlayer.Instance.bodyCollider.transform.position + new Vector3(0f, -0.2f, 0f);
        float sinusTypeShiiii = Mathf.Sin(Time.time * 6f) * 0.1f;
        Vector3 adjustedPos = basePos + GTPlayer.Instance.bodyCollider.transform.rotation * new Vector3(0f, 0f, sinusTypeShiiii + 0.1f);

        Quaternion adjustedRot = Quaternion.Euler(0f, 10f, 0f) * GTPlayer.Instance.bodyCollider.transform.rotation;
        jonklerHand.position = adjustedPos;
        jonklerHand.rotation = adjustedRot;
    }
}

[HarmonyPatch(typeof(GTPlayer), "LateUpdate")]
internal static class RigGestureFingerPatch
{
    private static void Postfix()
    {
        if (JerkOff.Instance == null || !JerkOff.Instance.Enabled || VRRig.LocalRig == null) return;
        VRRig.LocalRig.rightIndex.calcT = VRRig.LocalRig.rightMiddle.calcT = VRRig.LocalRig.rightThumb.calcT = 1f;
        VRRig.LocalRig.rightIndex.LerpFinger(1f, false);
        VRRig.LocalRig.rightMiddle.LerpFinger(1f, false);
        VRRig.LocalRig.rightThumb.LerpFinger(1f, false);
    }
}
