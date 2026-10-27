// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using UnityEngine;

namespace ChudOS.Core.Managers;

public static partial class MacroManager
{
    private static VRRig? poseRig;
    private static bool poseWasEnabled, legacyPose;
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
        legacyPose = legacyPlayback;
        poseWasEnabled = rig.enabled;
        rig.enabled = false;
        return true;
    }

    private static void ApplyMacroPose(Frame a, Frame b, float t, Vector3 offsetA)
    {
        if (poseRig == null) return;
        poseRig.transform.SetPositionAndRotation(Vector3.Lerp(Vector(a.Body) + offsetA, Vector(b.Body) + playbackOffset, t), Quaternion.Slerp(Rotation(a.RigRotation), Rotation(b.RigRotation), t));
        if (legacyPose) poseRig.head.rigTarget.position = Vector3.Lerp(Vector(a.HeadPosition) + offsetA, Vector(b.HeadPosition) + playbackOffset, t);
        poseRig.head.rigTarget.rotation = Quaternion.Slerp(Rotation(a.HeadRotation), Rotation(b.HeadRotation), t);
        poseRig.leftHand.rigTarget.SetPositionAndRotation(Vector3.Lerp(Vector(a.LeftPosition) + offsetA, Vector(b.LeftPosition) + playbackOffset, t), Quaternion.Slerp(Rotation(a.LeftRotation), Rotation(b.LeftRotation), t));
        poseRig.rightHand.rigTarget.SetPositionAndRotation(Vector3.Lerp(Vector(a.RightPosition) + offsetA, Vector(b.RightPosition) + playbackOffset, t), Quaternion.Slerp(Rotation(a.RightRotation), Rotation(b.RightRotation), t));
    }

    private static void RestoreMacroPose()
    {
        if (poseRig == null && poseTargets.Length == 0) return;
        for (int i = legacyPose ? 0 : 1; i < poseTargets.Length; i++)
            if (poseTargets[i] != null) { poseTargets[i].localPosition = posePositions[i]; poseTargets[i].localRotation = poseRotations[i]; }
        if (poseRig != null) poseRig.enabled = poseWasEnabled;
        poseRig = null;
        poseTargets = Array.Empty<Transform>();
        posePositions = Array.Empty<Vector3>();
        poseRotations = Array.Empty<Quaternion>();
    }
}
