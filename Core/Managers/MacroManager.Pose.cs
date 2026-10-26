// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using UnityEngine;

namespace ChudOS.Core.Managers;

public static partial class MacroManager
{
    private static VRRig? poseRig;
    private static bool poseWasEnabled;
    private static Transform[] poseTargets = Array.Empty<Transform>();
    private static Vector3[] posePositions = Array.Empty<Vector3>();
    private static Quaternion[] poseRotations = Array.Empty<Quaternion>();

    private static bool BeginMacroPose()
    {
        var rig = VRRig.LocalRig;
        var tagger = GorillaTagger.Instance;
        if (rig == null || tagger == null || rig.head.rigTarget == null || rig.leftHand.rigTarget == null || rig.rightHand.rigTarget == null) return false;
        poseTargets = new[] { rig.transform, rig.head.rigTarget, rig.leftHand.rigTarget, rig.rightHand.rigTarget, tagger.leftHandTransform, tagger.rightHandTransform };
        posePositions = new Vector3[poseTargets.Length];
        poseRotations = new Quaternion[poseTargets.Length];
        for (int i = 0; i < poseTargets.Length; i++) { posePositions[i] = poseTargets[i].localPosition; poseRotations[i] = poseTargets[i].localRotation; }
        poseRig = rig;
        poseWasEnabled = rig.enabled;
        rig.enabled = false;
        return true;
    }

    private static void ApplyMacroPose(Frame a, Frame b, float t)
    {
        if (poseRig == null) return;
        poseRig.transform.SetPositionAndRotation(Vector3.Lerp(Vector(a.Body), Vector(b.Body), t), Quaternion.Slerp(Rotation(a.RigRotation), Rotation(b.RigRotation), t));
        poseRig.head.rigTarget.SetPositionAndRotation(Vector3.Lerp(Vector(a.HeadPosition), Vector(b.HeadPosition), t), Quaternion.Slerp(Rotation(a.HeadRotation), Rotation(b.HeadRotation), t));
        poseRig.leftHand.rigTarget.SetPositionAndRotation(Vector3.Lerp(Vector(a.LeftPosition), Vector(b.LeftPosition), t), Quaternion.Slerp(Rotation(a.LeftRotation), Rotation(b.LeftRotation), t));
        poseRig.rightHand.rigTarget.SetPositionAndRotation(Vector3.Lerp(Vector(a.RightPosition), Vector(b.RightPosition), t), Quaternion.Slerp(Rotation(a.RightRotation), Rotation(b.RightRotation), t));
    }

    private static void RestoreMacroPose()
    {
        if (poseRig == null && poseTargets.Length == 0) return;
        for (int i = 0; i < poseTargets.Length; i++)
            if (poseTargets[i] != null) { poseTargets[i].localPosition = posePositions[i]; poseTargets[i].localRotation = poseRotations[i]; }
        if (poseRig != null) poseRig.enabled = poseWasEnabled;
        poseRig = null;
        poseTargets = Array.Empty<Transform>();
        posePositions = Array.Empty<Vector3>();
        poseRotations = Array.Empty<Quaternion>();
    }
}
