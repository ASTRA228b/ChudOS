// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Safety;
using ChudOS.Stuff;
using GorillaLocomotion;
using HarmonyLib;
using UnityEngine;

namespace ChudOS.Mods.Movement;

public static class WallWalkContact
{
    private static readonly AccessTools.FieldRef<GTPlayer, RaycastHit> LastHandHit = AccessTools.FieldRefAccess<GTPlayer, RaycastHit>("lastHitInfoHand");
    private static readonly float[] Strengths = { 15f, 9.81f, 5f, 25f, 40f, 55f };
    private static float strength = 15f;
    private static int input;

    [MenuButton("Wall Walk V2", ButtonType.Toggle, Category = "Movement", Order = 166, runOption = RunOption.FixedUpdate, Description = "Hold selected inputs while touching a wall to attach using the surface normal")]
    public static void Tick()
    {
        if (!PortTools.Ready || !MovementInput.Both(input)) return;
        var player = GTPlayer.Instance;
        var body = GorillaTagger.Instance.rigidbody;
        if (body == null || strength <= 0f) return;
        bool left = player.IsHandTouching(true), right = player.IsHandTouching(false);
        if (!left && !right) return;
        RaycastHit hit = LastHandHit(player);
        if (hit.collider == null || !NetworkSafety.Finite(hit.normal) || !NetworkSafety.Finite(hit.point) || hit.normal.sqrMagnitude < 0.0001f) return;
        float distance = float.PositiveInfinity;
        if (left) distance = Vector3.Distance(player.LeftHand.controllerTransform.position, hit.point);
        if (right) distance = Mathf.Min(distance, Vector3.Distance(player.RightHand.controllerTransform.position, hit.point));
        if (!NetworkSafety.Finite(player.scale) || player.scale <= 0f || distance > 0.4f * player.scale) return;
        body.AddForce(-hit.normal.normalized * strength, ForceMode.Acceleration);
        if (body.useGravity) body.AddForce(-Physics.gravity, ForceMode.Acceleration);
    }
    [MenuButton("Wall Walk V2 Strength", ButtonType.Option, Category = "Settings", Order = 264, Options = new[] { "15", "9.81", "5", "25", "40", "55" }, Notify = false)]
    public static void SetStrength() => strength = Strengths[ButtonManager.GetButton("Wall Walk V2 Strength")!.OptionIndex];
    [MenuButton("Wall Walk V2 Input", ButtonType.Option, Category = "Settings", Order = 265, Options = new[] { "Both Triggers", "Both Grips", "Both Joystick Clicks", "A + X", "B + Y" }, Notify = false)]
    public static void SetInput() => input = ButtonManager.GetButton("Wall Walk V2 Input")!.OptionIndex;
}
