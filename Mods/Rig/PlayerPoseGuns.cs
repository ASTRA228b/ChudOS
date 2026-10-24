// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using UnityEngine;

namespace ChudOS.Mods.Rig;

internal abstract class PlayerPoseGun : RigBehaviour
{
    private VRRig? target;
    private bool posed;
    protected abstract void Pose(VRRig person);

    protected override void LateUpdate()
    {
        if (!GunLib.UpdateRigGun(GunLib.AimHand, GunLib.AimPressed, GunLib.SelectPressed, ref target) || target == null)
        {
            Restore();
            return;
        }
        RigRuntime.ToggleRig(false);
        posed = true;
        Pose(target);
    }

    protected static Vector3 Forward(VRRig person)
    {
        Vector3 forward = Vector3.ProjectOnPlane(person.transform.forward, Vector3.up);
        return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
    }

    protected static void Body(VRRig person, Vector3 position, Vector3 forward)
    {
        VRRig rig = VRRig.LocalRig;
        float scale = Mathf.Clamp(person.scaleFactor, 0.2f, 3f);
        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
        rig.transform.SetPositionAndRotation(position, rotation);
        rig.head.rigTarget.SetPositionAndRotation(position + Vector3.up * (0.35f * scale), rotation);
        Vector3 right = rotation * Vector3.right;
        rig.leftHand.rigTarget.SetPositionAndRotation(position - right * (0.3f * scale), rotation);
        rig.rightHand.rigTarget.SetPositionAndRotation(position + right * (0.3f * scale), rotation);
    }

    private void Restore()
    {
        if (!posed) return;
        RigRuntime.RestorePose();
        posed = false;
    }

    protected override void OnDisable() { target = null; Restore(); GunLib.Hide(); }
}

internal sealed class HugGun : PlayerPoseGun
{
    protected override void Pose(VRRig person)
    {
        float scale = Mathf.Clamp(person.scaleFactor, 0.2f, 3f);
        Vector3 forward = Forward(person);
        Vector3 position = person.transform.position + forward * (0.55f * scale);
        Body(person, position, -forward);
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 shoulders = person.head.rigTarget.position - Vector3.up * (0.35f * scale);
        Vector3 around = shoulders - forward * (0.15f * scale);
        Quaternion palms = Quaternion.LookRotation(forward, Vector3.up);
        VRRig.LocalRig.leftHand.rigTarget.SetPositionAndRotation(around + right * (0.3f * scale), palms);
        VRRig.LocalRig.rightHand.rigTarget.SetPositionAndRotation(around - right * (0.3f * scale), palms);
        VRRig.LocalRig.leftHand.calcT = VRRig.LocalRig.rightHand.calcT = 0f;
    }
}

internal sealed class FollowPlayerGun : PlayerPoseGun
{
    protected override void Pose(VRRig person)
    {
        Vector3 forward = Forward(person);
        Body(person, person.transform.position - forward * Mathf.Clamp(person.scaleFactor, 0.2f, 3f), forward);
    }
}

internal sealed class OrbitPlayerGun : PlayerPoseGun
{
    private float angle;
    protected override void Pose(VRRig person)
    {
        angle = Mathf.Repeat(angle + Mathf.Min(Time.deltaTime, 0.05f) * 60f, 360f);
        Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * (1.1f * Mathf.Clamp(person.scaleFactor, 0.2f, 3f));
        Body(person, person.transform.position + offset, -offset.normalized);
    }
}
