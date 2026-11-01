// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using UnityEngine;

namespace ChudOS.Mods.Rig;

internal abstract class ExpansionPose : RigBehaviour
{
    protected abstract string Mode { get; }
    private Vector3 frozen;
    private Quaternion rotation;
    private readonly List<(Transform Bone, Vector3 Scale)> bones = new();
    protected override void OnEnable()
    {
        frozen = RigRuntime.RigPosition; rotation = RigRuntime.RigRotation;
        RigRuntime.ToggleRig(false);
        if (Mode == "Amputate Rig")
        {
            foreach (SkinnedMeshRenderer mesh in VRRig.LocalRig.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                foreach (Transform bone in mesh.bones)
                    if (bone != null && (bone.name.IndexOf("forearm", StringComparison.OrdinalIgnoreCase) >= 0 || bone.name.IndexOf("upperarm", StringComparison.OrdinalIgnoreCase) >= 0 || bone.name.IndexOf("arm.", StringComparison.OrdinalIgnoreCase) >= 0) && !bones.Any(b => b.Bone == bone))
                    { bones.Add((bone, bone.localScale)); bone.localScale = Vector3.zero; }
            if (bones.Count == 0) CreativeTools.Notice("Amputate Rig: no compatible arm bones were found in this avatar.");
        }
    }
    protected override void LateUpdate()
    {
        var rig = VRRig.LocalRig;
        Vector3 center = CreativeTools.Head.position - Vector3.up * 0.35f * CreativeTools.Scale;
        Quaternion yaw = Quaternion.Euler(0f, CreativeTools.Head.eulerAngles.y, 0f);
        rig.transform.SetPositionAndRotation(center, yaw);
        rig.head.rigTarget.SetPositionAndRotation(CreativeTools.Head.position, CreativeTools.Head.rotation);
        rig.leftHand.rigTarget.SetPositionAndRotation(CreativeTools.Left.position, CreativeTools.Left.rotation);
        rig.rightHand.rigTarget.SetPositionAndRotation(CreativeTools.Right.position, CreativeTools.Right.rotation);
        float t = Time.unscaledTime;
        switch (Mode)
        {
            case "Weeping Angel":
                bool watched = false;
                foreach (VRRig other in VRRigCache.ActiveRigs)
                {
                    if (other == null || other == rig || other.head?.rigTarget == null) continue;
                    Transform head = other.head.rigTarget;
                    Vector3 toward = frozen - head.position;
                    if (toward.sqrMagnitude < 225f && Vector3.Dot(head.forward, toward.normalized) > 0.8f) { watched = true; break; }
                }
                if (!watched) { frozen = Vector3.MoveTowards(frozen, center, Time.unscaledDeltaTime * 5f); rotation = yaw; }
                Vector3 offset = frozen - center;
                rig.transform.SetPositionAndRotation(frozen, rotation);
                rig.head.rigTarget.position += offset; rig.leftHand.rigTarget.position += offset; rig.rightHand.rigTarget.position += offset;
                break;
            case "Minecraft Animations":
                float swing = Mathf.Sin(t * 7f) * Mathf.Clamp(CreativeTools.Body.linearVelocity.magnitude * 5f, 0f, 55f);
                rig.leftHand.rigTarget.position = center + yaw * new Vector3(-0.25f, -0.2f, Mathf.Sin(swing * Mathf.Deg2Rad) * 0.45f);
                rig.rightHand.rigTarget.position = center + yaw * new Vector3(0.25f, -0.2f, -Mathf.Sin(swing * Mathf.Deg2Rad) * 0.45f);
                rig.leftHand.rigTarget.rotation = yaw * Quaternion.Euler(swing, 0f, 0f);
                rig.rightHand.rigTarget.rotation = yaw * Quaternion.Euler(-swing, 0f, 0f);
                break;
            case "Chicken Rig":
                float flap = Mathf.Sin(t * 9f) * 0.15f;
                rig.leftHand.rigTarget.position = center + yaw * new Vector3(-0.32f, flap, 0f);
                rig.rightHand.rigTarget.position = center + yaw * new Vector3(0.32f, flap, 0f);
                break;
            case "Floating Rig":
                Vector3 lift = Vector3.up * (0.25f + Mathf.Sin(t * 2f) * 0.15f);
                rig.transform.position += lift; rig.head.rigTarget.position += lift; rig.leftHand.rigTarget.position += lift; rig.rightHand.rigTarget.position += lift;
                break;
            case "Eye Contact":
                VRRig? target = null; float nearest = 100f;
                foreach (VRRig other in VRRigCache.ActiveRigs)
                    if (other != null && other != rig && other.head?.rigTarget != null)
                    { float d = (other.head.rigTarget.position - CreativeTools.Head.position).sqrMagnitude; if (d < nearest) { nearest = d; target = other; } }
                if (target != null) rig.head.rigTarget.rotation = Quaternion.LookRotation(target.head.rigTarget.position - rig.head.rigTarget.position, Vector3.up);
                break;
        }
    }
    protected override void OnDisable()
    {
        foreach (var bone in bones) if (bone.Bone != null) bone.Bone.localScale = bone.Scale;
        bones.Clear(); RigRuntime.RestorePose();
    }
}
internal sealed class AngelPose : ExpansionPose { protected override string Mode => "Weeping Angel"; }
internal sealed class MinecraftPose : ExpansionPose { protected override string Mode => "Minecraft Animations"; }
internal sealed class ChickenPose : ExpansionPose { protected override string Mode => "Chicken Rig"; }
internal sealed class AmputatePose : ExpansionPose { protected override string Mode => "Amputate Rig"; }
internal sealed class FloatingPose : ExpansionPose { protected override string Mode => "Floating Rig"; }
internal sealed class EyeContactPose : ExpansionPose { protected override string Mode => "Eye Contact"; }
