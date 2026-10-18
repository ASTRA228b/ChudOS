using GorillaLocomotion;
using UnityEngine;

namespace ChudOS.Mods.Rig;

internal sealed class SmoothRig : RigBehaviour
{
    private const float LerpAmount = 10f;

    private readonly Vector3 headRotationOffset = new(0f, 0f, 0f);

    private readonly Vector3 leftHandPositionOffset = new(-0.06f, 0.04f, -0.1f);
    private readonly Vector3 leftHandRotationOffset = new(-50f, 0f, 180f);

    private readonly Vector3 rightHandPositionOffset = new(0.06f, 0.04f, -0.1f);
    private readonly Vector3 rightHandRotationOffset = new(-50f, 0f, 180f);

    private readonly Vector3 rigPositionOffset = new(0f, 0.14f, 0f);

    protected override void OnEnable() => RigRuntime.ToggleRig(false);

    protected override void LateUpdate()
    {
        RigRuntime.RigPosition = Vector3.Lerp(RigRuntime.RigPosition, GorillaTagger.Instance.bodyCollider.transform.TransformPoint(rigPositionOffset), Time.deltaTime * (LerpAmount * 7f));

        RigRuntime.RigRotation = Quaternion.Slerp(RigRuntime.RigRotation, GorillaTagger.Instance.bodyCollider.transform.rotation, Time.deltaTime * LerpAmount);

        VRRig.LocalRig.leftHand.rigTarget.transform.position = Vector3.Lerp(VRRig.LocalRig.leftHand.rigTarget.transform.position, GorillaTagger.Instance.leftHandTransform.TransformPoint(leftHandPositionOffset), Time.deltaTime * (LerpAmount * 0.8f));

        VRRig.LocalRig.leftHand.rigTarget.transform.rotation = Quaternion.Slerp(VRRig.LocalRig.leftHand.rigTarget.transform.rotation, GorillaTagger.Instance.leftHandTransform.rotation * Quaternion.Euler(leftHandRotationOffset), Time.deltaTime * LerpAmount);

        VRRig.LocalRig.rightHand.rigTarget.transform.position = Vector3.Lerp(VRRig.LocalRig.rightHand.rigTarget.transform.position, GorillaTagger.Instance.rightHandTransform.TransformPoint(rightHandPositionOffset), Time.deltaTime * (LerpAmount * 0.8f));

        VRRig.LocalRig.rightHand.rigTarget.transform.rotation = Quaternion.Slerp(VRRig.LocalRig.rightHand.rigTarget.transform.rotation, GorillaTagger.Instance.rightHandTransform.rotation * Quaternion.Euler(rightHandRotationOffset), Time.deltaTime * LerpAmount);

        VRRig.LocalRig.head.rigTarget.transform.rotation = Quaternion.Slerp(VRRig.LocalRig.head.rigTarget.transform.rotation, GTPlayer.Instance.headCollider.transform.rotation * Quaternion.Euler(headRotationOffset), Time.deltaTime * (LerpAmount * 0.5f));
    }

    protected override void OnDisable() => RigRuntime.ToggleRig(true);
}