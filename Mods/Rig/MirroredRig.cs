// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Rig;

public static class MirroredRig
{
    [MenuButton("Mirrored Rig", ButtonType.Toggle, Category = "Rig", Order = 820, runOption = RunOption.Once, Description = "Reflect your tracked avatar pose across its vertical center plane.")]
    public static void Enable() => RigRuntime.Enable<MirroredPose>("Mirrored Rig");
    [MenuDisable("Mirrored Rig")]
    public static void Disable() => RigRuntime.Disable("Mirrored Rig");
}
internal sealed class MirroredPose : RigBehaviour
{
    protected override void OnEnable() => RigRuntime.ToggleRig(false);
    private static Vector3 Reflect(Vector3 direction, Vector3 normal) => direction - 2f * Vector3.Dot(direction, normal) * normal;
    private static Quaternion ReflectRotation(Quaternion rotation, Vector3 normal) => Quaternion.LookRotation(Reflect(rotation * Vector3.forward, normal), Reflect(rotation * Vector3.up, normal));
    protected override void LateUpdate()
    {
        var rig = VRRig.LocalRig;
        Vector3 center = CreativeTools.Position;
        Quaternion yaw = Quaternion.Euler(0f, CreativeTools.Head.eulerAngles.y, 0f);
        Vector3 normal = yaw * Vector3.right;
        rig.transform.SetPositionAndRotation(center, yaw);
        rig.head.rigTarget.SetPositionAndRotation(center + Reflect(CreativeTools.Head.position - center, normal), ReflectRotation(CreativeTools.Head.rotation, normal));
        rig.leftHand.rigTarget.SetPositionAndRotation(center + Reflect(CreativeTools.Right.position - center, normal), ReflectRotation(CreativeTools.Right.rotation, normal));
        rig.rightHand.rigTarget.SetPositionAndRotation(center + Reflect(CreativeTools.Left.position - center, normal), ReflectRotation(CreativeTools.Left.rotation, normal));
    }
    protected override void OnDisable() => RigRuntime.RestorePose();
}
