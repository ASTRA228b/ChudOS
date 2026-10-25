// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using UnityEngine;

namespace ChudOS.Mods.Movement;

public static class Predictions
{
    private static GTPlayer? player;
    private static GorillaVelocityTracker? leftTracker, rightTracker;
    private static Transform? leftHand, rightHand, leftController, rightController;
    private static Vector3 lastBody, leftOffset, rightOffset;
    private static float readyAt;

    [MenuButton("Predictions", ButtonType.Toggle, Category = "Movement", Order = 154, runOption = RunOption.Update, Description = "Apply configurable velocity-based hand prediction; configure it in Settings")]
    public static void Tick()
    {
        if (!PortTools.Ready) { Stop(); return; }
        PortTools.Register(Stop);
        var current = GTPlayer.Instance;
        if (current.bodyCollider == null || current.headCollider == null) { Stop(); return; }
        if (player != current || leftTracker != current.LeftHand.velocityTracker || rightTracker != current.RightHand.velocityTracker)
        {
            Stop();
            player = current;
            leftTracker = current.LeftHand.velocityTracker;
            rightTracker = current.RightHand.velocityTracker;
            leftController = current.LeftHand.controllerTransform;
            rightController = current.RightHand.controllerTransform;
            leftHand = GorillaTagger.Instance.leftHandTransform;
            rightHand = GorillaTagger.Instance.rightHandTransform;
            lastBody = current.bodyCollider.transform.position;
            ResetSamples();
        }
        if (leftTracker == null || rightTracker == null || leftHand == null || rightHand == null || leftController == null || rightController == null || current.headCollider == null) return;
        Vector3 body = current.bodyCollider.transform.position;
        if ((body - lastBody).sqrMagnitude > 9f) ResetSamples();
        lastBody = body;
        if (Time.unscaledTime < readyAt) return;

        float alpha = 1f - Mathf.Pow(1f - PredictionSettings.Smoothness, Mathf.Min(Time.deltaTime, 0.1f) * 90f);
        Vector3 scale = current.transform.lossyScale;
        float reach = PredictionSettings.Reach * Mathf.Max(Mathf.Abs(scale.x), Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
        Vector3 head = current.headCollider.transform.position;
        if (!Finite(head) || !Finite(leftController.position) || !Finite(rightController.position)) { Stop(); return; }
        leftOffset = Vector3.Lerp(leftOffset, Offset(leftTracker.GetAverageVelocity(true, 0.08f)), alpha);
        rightOffset = Vector3.Lerp(rightOffset, Offset(rightTracker.GetAverageVelocity(true, 0.08f)), alpha);
        leftHand.position = Limit(leftController.position, leftOffset, head, reach);
        rightHand.position = Limit(rightController.position, rightOffset, head, reach);
    }
    internal static Vector3 Offset(Vector3 velocity)
    {
        if (!Finite(velocity)) velocity = Vector3.zero;
        velocity = Vector3.ClampMagnitude(velocity, 5f);
        return velocity.sqrMagnitude > PredictionSettings.Threshold * PredictionSettings.Threshold ? velocity * PredictionSettings.Strength : Vector3.zero;
    }
    internal static Vector3 Limit(Vector3 tracked, Vector3 offset, Vector3 head, float reach) => head + Vector3.ClampMagnitude(tracked + offset - head, Mathf.Max(0.01f, reach));
    private static bool Finite(Vector3 value) => !float.IsNaN(value.x) && !float.IsInfinity(value.x) && !float.IsNaN(value.y) && !float.IsInfinity(value.y) && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
    public static void ResetSamples() { readyAt = Time.unscaledTime + 0.1f; leftOffset = rightOffset = Vector3.zero; }
    [MenuDisable("Predictions")]
    public static void Stop()
    {
        if (leftHand != null && leftController != null && leftHand != leftController) leftHand.position = leftController.position;
        if (rightHand != null && rightController != null && rightHand != rightController) rightHand.position = rightController.position;
        player = null;
        leftTracker = rightTracker = null;
        leftHand = rightHand = leftController = rightController = null;
        readyAt = 0f;
        leftOffset = rightOffset = Vector3.zero;
    }
}
