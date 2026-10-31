// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Movement;

public static class CreativeMovement
{
    private static readonly CreativeTools.Session Session = new();
    private static bool storedInput, launchInput, paddleReady;
    private static float storedSpeed;
    private static Vector3 leftPrevious, rightPrevious;
    private static bool Ready()
    {
        if (Session.Changed()) Clear();
        if (!CreativeTools.Ready) { Clear(); return false; }
        PortTools.Register(Clear);
        return true;
    }
    [MenuButton("Momentum Bank", ButtonType.Toggle, Category = "Movement", Order = 400, Controls = "(X)(A)", runOption = RunOption.FixedUpdate, Description = "Press X to store your speed and stop; press A to launch in your right hand's direction")]
    public static void MomentumBank()
    {
        if (!Ready()) return;
        bool store = CreativeTools.Pressed(InputLib.LeftControllerXButton, ref storedInput), launch = CreativeTools.Pressed(InputLib.RightControllerAButton, ref launchInput);
        Rigidbody body = CreativeTools.Body;
        if (store) { storedSpeed = Mathf.Min(body.linearVelocity.magnitude, CreativeSettings.BankCapacity * CreativeTools.Scale); body.linearVelocity = Vector3.zero; }
        if (!launch || storedSpeed <= 0f) return;
        Vector3 direction = CreativeTools.Right.forward;
        if (!CreativeTools.Finite(direction) || direction.sqrMagnitude < 0.001f) return;
        body.linearVelocity = direction.normalized * storedSpeed;
        storedSpeed = 0f;
    }
    [MenuDisable("Momentum Bank")]
    public static void StopBank() { storedSpeed = 0f; storedInput = launchInput = false; }
    [MenuButton("Surface Skate", ButtonType.Toggle, Category = "Movement", Order = 401, Controls = "(RG)", runOption = RunOption.FixedUpdate, Description = "Hold right grip near a surface; look along it to glide in that direction")]
    public static void SurfaceSkate()
    {
        if (!Ready() || !InputLib.RightGrab) return;
        Transform hand = CreativeTools.Right;
        if (!Physics.Raycast(hand.position, -hand.up, out RaycastHit hit, 0.35f * CreativeTools.Scale, NoClipState.CollisionLayers, QueryTriggerInteraction.Ignore)) return;
        if (hit.collider == null || hit.collider.GetComponentInParent<VRRig>() != null) return;
        Vector3 direction = Vector3.ProjectOnPlane(CreativeTools.Head.forward, hit.normal);
        if (direction.sqrMagnitude < 0.01f) return;
        Vector3 target = direction.normalized * CreativeSettings.SkateSpeed * CreativeTools.Scale - hit.normal * 0.4f;
        CreativeTools.Body.linearVelocity = Vector3.MoveTowards(CreativeTools.Body.linearVelocity, target, 25f * CreativeTools.Scale * Time.fixedDeltaTime);
    }
    [MenuButton("Air Paddle", ButtonType.Toggle, Category = "Movement", Order = 402, Controls = "(RG)(LG)", runOption = RunOption.FixedUpdate, Description = "Hold both grips and paddle your hands; you move opposite each stroke")]
    public static void AirPaddle()
    {
        if (!Ready()) return;
        Vector3 left = CreativeTools.Left.position - CreativeTools.Position, right = CreativeTools.Right.position - CreativeTools.Position;
        bool held = InputLib.LeftGrab && InputLib.RightGrab;
        if (held && paddleReady)
        {
            Vector3 stroke = ((left - leftPrevious) + (right - rightPrevious)) / (2f * Mathf.Max(Time.fixedDeltaTime, 0.001f));
            if (CreativeTools.Finite(stroke) && stroke.magnitude < 12f * CreativeTools.Scale) CreativeTools.Body.AddForce(-stroke * CreativeSettings.PaddlePower, ForceMode.Acceleration);
        }
        leftPrevious = left; rightPrevious = right; paddleReady = held;
    }
    [MenuDisable("Air Paddle")]
    public static void StopPaddle() => paddleReady = false;
    [MenuButton("Gravity Compass", ButtonType.Toggle, Category = "Movement", Order = 403, Controls = "(X)", runOption = RunOption.FixedUpdate, Description = "Hold X to pull yourself in your right hand's pointing direction; release for normal gravity")]
    public static void GravityCompass()
    {
        if (!Ready() || !InputLib.LeftControllerXButton) return;
        Vector3 direction = CreativeTools.Right.forward;
        if (!CreativeTools.Finite(direction) || direction.sqrMagnitude < 0.001f) return;
        Rigidbody body = CreativeTools.Body;
        body.AddForce((body.useGravity ? -Physics.gravity : Vector3.zero) + direction.normalized * CreativeSettings.CompassGravity, ForceMode.Acceleration);
    }
    [MenuButton("Landing Brake", ButtonType.Toggle, Category = "Movement", Order = 404, runOption = RunOption.FixedUpdate, Description = "Automatically reduce downward speed shortly before a landing")]
    public static void LandingBrake()
    {
        if (!Ready()) return;
        Rigidbody body = CreativeTools.Body;
        Vector3 velocity = body.linearVelocity;
        float scale = CreativeTools.Scale;
        if (velocity.y >= -4f * scale || !CreativeTools.Ground(out _, CreativeSettings.LandingDistance)) return;
        velocity.y = Mathf.MoveTowards(velocity.y, -scale, 35f * scale * Time.fixedDeltaTime);
        body.linearVelocity = velocity;
    }
    internal static void Clear() { StopBank(); StopPaddle(); leftPrevious = rightPrevious = Vector3.zero; }
}
