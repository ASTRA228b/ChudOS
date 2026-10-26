// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using GorillaLocomotion;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ChudOS.Mods.Rig;

internal sealed class RigGun : RigBehaviour
{
    private bool posed;

    protected override void LateUpdate()
    {
        if (!GunLib.UpdateGun(GunLib.AimHand, GunLib.AimPressed, GunLib.SelectPressed, out Vector3 point)) { Restore(); return; }
        RigRuntime.ToggleRig(false);
        posed = true;
        VRRig.LocalRig.transform.position = point + GunLib.LastHit.normal * (0.2f * GTPlayer.Instance.scale);
    }
    private void Restore()
    {
        if (!posed) return;
        RigRuntime.RestorePose();
        posed = false;
    }
    protected override void OnDisable() { Restore(); GunLib.Hide(); }
}

internal sealed class SpazzRig : RigBehaviour
{
    private VRRig rig = null!;
    private Vector3 left, right, head;

    protected override void OnEnable()
    {
        rig = VRRig.LocalRig;
        left = rig.leftHand.trackingPositionOffset;
        right = rig.rightHand.trackingPositionOffset;
        head = rig.head.trackingPositionOffset;
    }
    protected override void Update()
    {
        if (!InputLib.RightControllerAButton) { Restore(); return; }
        rig.leftHand.trackingPositionOffset = left + Offset();
        rig.rightHand.trackingPositionOffset = right + Offset();
        rig.head.trackingPositionOffset = head + Offset();
    }
    private static Vector3 Offset() => new(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
    private void Restore()
    {
        if (rig == null) return;
        rig.leftHand.trackingPositionOffset = left;
        rig.rightHand.trackingPositionOffset = right;
        rig.head.trackingPositionOffset = head;
    }
    protected override void OnDisable() => Restore();
}

internal sealed class SpazzHands : RigBehaviour
{
    private bool posed;

    protected override void LateUpdate()
    {
        if (!InputLib.RightControllerAButton)
        {
            if (posed) RigRuntime.RestorePose();
            posed = false;
            return;
        }
        var tagger = GorillaTagger.Instance;
        var rig = VRRig.LocalRig;
        RigRuntime.ToggleRig(false);
        posed = true;
        rig.transform.SetPositionAndRotation(tagger.bodyCollider.transform.position + Vector3.up * 0.15f, tagger.bodyCollider.transform.rotation);
        rig.head.rigTarget.rotation = tagger.headCollider.transform.rotation;
        rig.leftHand.rigTarget.rotation = Random.rotation;
        rig.rightHand.rigTarget.rotation = Random.rotation;
        rig.leftHand.rigTarget.position = GTPlayer.Instance.LeftHand.controllerTransform.position + rig.leftHand.rigTarget.forward * 3f;
        rig.rightHand.rigTarget.position = GTPlayer.Instance.RightHand.controllerTransform.position + rig.rightHand.rigTarget.forward * 3f;
    }
    protected override void OnDisable()
    {
        if (posed) RigRuntime.RestorePose();
        posed = false;
    }
}

internal sealed class FixedLongArms : RigBehaviour
{
    private Transform? player;
    private Vector3 original;

    protected override void OnEnable()
    {
        ButtonManager.SetEnabled("Long Arms", false);
        ButtonManager.SetEnabled("Size Changer", false);
        player = GTPlayer.Instance.transform;
        original = player.localScale;
    }
    protected override void LateUpdate()
    {
        if (player == null || GTPlayer.Instance == null || player != GTPlayer.Instance.transform)
        {
            ButtonManager.SetEnabled("Fixed LongArms", false);
            return;
        }
        player.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }
    protected override void OnDisable()
    {
        if (player != null) player.localScale = original;
        player = null;
    }
}

internal sealed class GrabRig : RigBehaviour
{
    private bool posed;

    protected override void LateUpdate()
    {
        if (!InputLib.RightGrab)
        {
            if (posed) RigRuntime.RestorePose();
            posed = false;
            return;
        }
        Transform hand = GTPlayer.Instance.RightHand.controllerTransform;
        RigRuntime.ToggleRig(false);
        posed = true;
        VRRig.LocalRig.transform.SetPositionAndRotation(hand.position, hand.rotation);
    }
    protected override void OnDisable()
    {
        if (posed) RigRuntime.RestorePose();
        posed = false;
    }
}

internal sealed class FakeLag : RigBehaviour
{
    private float nextUpdate;

    protected override void OnEnable() => nextUpdate = 0f;
    protected override void LateUpdate()
    {
        RigRuntime.ToggleRig(false);
        if (Time.unscaledTime < nextUpdate) return;
        nextUpdate = Time.unscaledTime + 0.5f;
        VRRig rig = VRRig.LocalRig;
        rig.enabled = true;
        try { rig.PostTick(); }
        finally { if (rig != null) rig.enabled = false; }
    }
}
