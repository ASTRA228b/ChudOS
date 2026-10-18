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

using UnityEngine;

namespace ChudOS.Mods.Rig;

internal sealed class GhostAnimations : RigBehaviour
{
    private static Vector3 headPos = Vector3.zero;
    private static Vector3 headRot = Vector3.zero;

    private static Vector3 handPosL = Vector3.zero;
    private static Vector3 handRotL = Vector3.zero;

    private static Vector3 handPosR = Vector3.zero;
    private static Vector3 handRotR = Vector3.zero;

    protected override void Update()
    {
        if (RigRuntime.IsRigEnabled)
            RigRuntime.ToggleRig(false);

        if (headPos == Vector3.zero)
            headPos = GorillaTagger.Instance.headCollider.transform.position;

        if (headRot == Vector3.zero)
            headRot = GorillaTagger.Instance.headCollider.transform.rotation.eulerAngles;

        if (handPosL == Vector3.zero)
            handPosL = GorillaTagger.Instance.leftHandTransform.transform.position;

        if (handRotL == Vector3.zero)
            handRotL = GorillaTagger.Instance.leftHandTransform.transform.rotation.eulerAngles;

        if (handPosR == Vector3.zero)
            handPosR = GorillaTagger.Instance.rightHandTransform.transform.position;

        if (handRotR == Vector3.zero)
            handRotR = GorillaTagger.Instance.rightHandTransform.transform.rotation.eulerAngles;

        const float PositionSpeed = 0.05f;
        const float RotationSpeed = 2.0f;
        const float PositionThreshold = 0.05f;
        const float RotationThreshold = 11.5f;
        if (Vector3.Distance(headPos, GorillaTagger.Instance.headCollider.transform.position) > PositionThreshold)
            headPos += Vector3.Normalize(GorillaTagger.Instance.headCollider.transform.position - headPos) * PositionSpeed;

        if (Quaternion.Angle(Quaternion.Euler(headRot), GorillaTagger.Instance.headCollider.transform.rotation) > RotationThreshold)
            headRot = Quaternion.RotateTowards(Quaternion.Euler(headRot), GorillaTagger.Instance.headCollider.transform.rotation, RotationSpeed).eulerAngles;

        if (Vector3.Distance(handPosL, GorillaTagger.Instance.leftHandTransform.transform.position) > PositionThreshold)
            handPosL += Vector3.Normalize(GorillaTagger.Instance.leftHandTransform.transform.position - handPosL) * PositionSpeed;

        if (Quaternion.Angle(Quaternion.Euler(handRotL), GorillaTagger.Instance.leftHandTransform.transform.rotation) > RotationThreshold)
            handRotL = Quaternion.RotateTowards(Quaternion.Euler(handRotL), GorillaTagger.Instance.leftHandTransform.transform.rotation, RotationSpeed).eulerAngles;

        if (Vector3.Distance(handPosR, GorillaTagger.Instance.rightHandTransform.transform.position) > PositionThreshold)
            handPosR += Vector3.Normalize(GorillaTagger.Instance.rightHandTransform.transform.position - handPosR) * PositionSpeed;

        if (Quaternion.Angle(Quaternion.Euler(handRotR), GorillaTagger.Instance.rightHandTransform.transform.rotation) > RotationThreshold)
            handRotR = Quaternion.RotateTowards(Quaternion.Euler(handRotR), GorillaTagger.Instance.rightHandTransform.transform.rotation, RotationSpeed).eulerAngles;

        RigRuntime.RigPosition = headPos - new Vector3(0f, 0.15f, 0f);
        RigRuntime.RigRotation = Quaternion.Euler(new Vector3(0f, headRot.y, 0f));

        VRRig.LocalRig.head.rigTarget.transform.rotation = Quaternion.Euler(headRot);

        VRRig.LocalRig.leftHand.rigTarget.transform.position = handPosL;
        VRRig.LocalRig.rightHand.rigTarget.transform.position = handPosR;

        VRRig.LocalRig.leftHand.rigTarget.transform.rotation = Quaternion.Euler(handRotL);
        VRRig.LocalRig.rightHand.rigTarget.transform.rotation = Quaternion.Euler(handRotR);

        VRRig.LocalRig.leftIndex.calcT = ControllerInputPoller.instance.leftControllerIndexFloat;
        VRRig.LocalRig.leftMiddle.calcT = RigInput.Instance.LeftGrip.IsPressed ? 1 : 0;
        VRRig.LocalRig.leftThumb.calcT = RigInput.Instance.LeftPrimary.IsPressed || RigInput.Instance.LeftSecondary.IsPressed ? 1 : 0;

        VRRig.LocalRig.leftIndex.LerpFinger(1f, false);
        VRRig.LocalRig.leftMiddle.LerpFinger(1f, false);
        VRRig.LocalRig.leftThumb.LerpFinger(1f, false);

        VRRig.LocalRig.rightIndex.calcT = ControllerInputPoller.instance.rightControllerIndexFloat;
        VRRig.LocalRig.rightMiddle.calcT = RigInput.Instance.RightGrip.IsPressed ? 1 : 0;
        VRRig.LocalRig.rightThumb.calcT = RigInput.Instance.RightPrimary.IsPressed || RigInput.Instance.RightSecondary.IsPressed ? 1 : 0;

        VRRig.LocalRig.rightIndex.LerpFinger(1f, false);
        VRRig.LocalRig.rightMiddle.LerpFinger(1f, false);
        VRRig.LocalRig.rightThumb.LerpFinger(1f, false);

        VRRig.LocalRig.leftHand.rigTarget.transform.rotation *= Quaternion.Euler(VRRig.LocalRig.leftHand.trackingRotationOffset);

        VRRig.LocalRig.rightHand.rigTarget.transform.rotation *= Quaternion.Euler(VRRig.LocalRig.rightHand.trackingRotationOffset);
    }

    protected override void OnDisable()
    {
        headPos = Vector3.zero;
        headRot = Vector3.zero;

        handPosL = Vector3.zero;
        handRotL = Vector3.zero;

        handPosR = Vector3.zero;
        handRotR = Vector3.zero;

        RigRuntime.ToggleRig(true);
    }
}