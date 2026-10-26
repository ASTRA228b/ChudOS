// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using GorillaLocomotion;
using UnityEngine;

namespace ChudOS.Mods.Rig;

internal sealed class Helicopter : RigBehaviour
{
    private bool posed;
    private Vector3 position;
    private float yaw;

    protected override void LateUpdate()
    {
        if (!InputLib.RightControllerAButton) { Restore(); return; }
        VRRig rig = VRRig.LocalRig;
        if (!posed)
        {
            position = rig.transform.position;
            yaw = rig.transform.eulerAngles.y;
            posed = true;
        }
        float delta = Mathf.Min(Time.deltaTime, 0.05f);
        float scale = Mathf.Clamp(GTPlayer.Instance.scale, 0.2f, 3f);
        position += Vector3.up * (1.5f * scale * delta);
        yaw = Mathf.Repeat(yaw + 360f * delta, 360f);
        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        RigRuntime.ToggleRig(false);
        rig.transform.SetPositionAndRotation(position, rotation);
        rig.head.rigTarget.rotation = rotation;
        rig.leftHand.rigTarget.SetPositionAndRotation(position - rotation * Vector3.right * scale, rotation);
        rig.rightHand.rigTarget.SetPositionAndRotation(position + rotation * Vector3.right * scale, rotation);
    }
    private void Restore()
    {
        if (!posed) return;
        RigRuntime.RestorePose();
        posed = false;
    }
    protected override void OnDisable() => Restore();
}

internal sealed class TPose : RigBehaviour
{
    private bool posed;

    protected override void LateUpdate()
    {
        if (!InputLib.RightControllerAButton) { Restore(); return; }
        var player = GTPlayer.Instance;
        VRRig rig = VRRig.LocalRig;
        float scale = Mathf.Clamp(player.scale, 0.2f, 3f);
        Vector3 position = player.bodyCollider.transform.position + Vector3.up * (0.15f * scale);
        Quaternion rotation = player.bodyCollider.transform.rotation;
        Vector3 right = rotation * Vector3.right;
        RigRuntime.ToggleRig(false);
        posed = true;
        rig.transform.SetPositionAndRotation(position, rotation);
        rig.head.rigTarget.rotation = rotation;
        rig.leftHand.rigTarget.SetPositionAndRotation(position - right * scale, rotation);
        rig.rightHand.rigTarget.SetPositionAndRotation(position + right * scale, rotation);
    }
    private void Restore()
    {
        if (!posed) return;
        RigRuntime.RestorePose();
        posed = false;
    }
    protected override void OnDisable() => Restore();
}

internal sealed class FreezeRigBody : RigBehaviour
{
    private Vector3 position;
    private Quaternion rotation;

    protected override void OnEnable()
    {
        position = VRRig.LocalRig.transform.position;
        rotation = VRRig.LocalRig.transform.rotation;
    }
    protected override void LateUpdate()
    {
        var player = GTPlayer.Instance;
        VRRig rig = VRRig.LocalRig;
        Transform left = player.LeftHand.controllerTransform, right = player.RightHand.controllerTransform;
        RigRuntime.ToggleRig(false);
        rig.transform.SetPositionAndRotation(position, rotation);
        rig.head.rigTarget.rotation = player.headCollider.transform.rotation;
        rig.leftHand.rigTarget.SetPositionAndRotation(left.position, left.rotation);
        rig.rightHand.rigTarget.SetPositionAndRotation(right.position, right.rotation);
    }
}

internal sealed class FreezeRigLimbs : RigBehaviour
{
    private Vector3 headPosition, leftPosition, rightPosition;
    private Quaternion headRotation, leftRotation, rightRotation;

    protected override void OnEnable()
    {
        VRRig rig = VRRig.LocalRig;
        headPosition = rig.head.rigTarget.position;
        headRotation = rig.head.rigTarget.rotation;
        leftPosition = rig.leftHand.rigTarget.position;
        leftRotation = rig.leftHand.rigTarget.rotation;
        rightPosition = rig.rightHand.rigTarget.position;
        rightRotation = rig.rightHand.rigTarget.rotation;
    }
    protected override void LateUpdate()
    {
        var player = GTPlayer.Instance;
        VRRig rig = VRRig.LocalRig;
        float scale = Mathf.Clamp(player.scale, 0.2f, 3f);
        RigRuntime.ToggleRig(false);
        rig.transform.SetPositionAndRotation(player.bodyCollider.transform.position + Vector3.up * (0.15f * scale), player.bodyCollider.transform.rotation);
        rig.head.rigTarget.SetPositionAndRotation(headPosition, headRotation);
        rig.leftHand.rigTarget.SetPositionAndRotation(leftPosition, leftRotation);
        rig.rightHand.rigTarget.SetPositionAndRotation(rightPosition, rightRotation);
    }
}

internal sealed class JumpscareGun : PlayerPoseGun
{
    protected override void Pose(VRRig person)
    {
        Transform head = person.head.rigTarget;
        float scale = Mathf.Clamp(person.scaleFactor, 0.2f, 3f);
        Vector3 forward = Forward(person);
        Vector3 facePosition = head.position + forward * (0.4f * scale);
        Vector3 bodyPosition = facePosition - Vector3.up * (0.35f * scale);
        Body(person, bodyPosition, -forward);
        VRRig rig = VRRig.LocalRig;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 hands = head.position + forward * (0.2f * scale);
        Quaternion rotation = Quaternion.LookRotation(-forward, Vector3.up);
        rig.head.rigTarget.SetPositionAndRotation(facePosition, rotation);
        rig.leftHand.rigTarget.SetPositionAndRotation(hands + right * (0.25f * scale), rotation);
        rig.rightHand.rigTarget.SetPositionAndRotation(hands - right * (0.25f * scale), rotation);
    }
}
