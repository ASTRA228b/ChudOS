// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;

namespace ChudOS.Mods.Movement;

public static class ExtraMovementMods
{
    private static readonly FlightState drone = new(), gun = new();
    private static Rigidbody? velocityBody;
    private static GTPlayer? throwOwner, jumpOwner;
    private static readonly Vector3[] handPositions = new Vector3[2];
    private static readonly bool[] handReady = new bool[2], swinging = new bool[2];
    private static readonly float[] blockedUntil = new float[2];
    private static float droneYaw, lastJump;
    private static bool jumpHeld, jumpSeeded, extraJumpUsed;

    [MenuButton("Drone Fly", ButtonType.Toggle, Category = "Movement", Order = 156, runOption = RunOption.FixedUpdate, Description = "Left stick moves; right stick turns and controls height")]
    public static void DroneFly()
    {
        if (!PortTools.Ready || GTPlayer.Instance.headCollider == null) { StopDrone(); return; }
        PortTools.Register(Clear);
        var body = GorillaTagger.Instance.rigidbody;
        if (drone.Body != body) droneYaw = GTPlayer.Instance.headCollider.transform.eulerAngles.y;
        drone.Take(body);
        Vector2 left = Deadzone(InputLib.LeftJoyStickAxis), right = Deadzone(InputLib.RightJoyStickAxis);
        droneYaw = Mathf.Repeat(droneYaw + right.x * 90f * Time.fixedDeltaTime, 360f);
        Vector3 input = Vector3.ClampMagnitude(new Vector3(left.x, right.y, left.y), 1f);
        Vector3 target = Quaternion.Euler(0f, droneYaw, 0f) * input * ExtraMovementSettings.DroneSpeed;
        float blend = 1f - Mathf.Exp(-ExtraMovementSettings.DroneResponse * Time.fixedDeltaTime);
        body.linearVelocity = Vector3.Lerp(body.linearVelocity, target, blend);
    }
    [MenuDisable("Drone Fly")]
    public static void StopDrone() => drone.Release();

    [MenuButton("Left And Right", ButtonType.Toggle, Category = "Movement", Order = 157, runOption = RunOption.FixedUpdate, Description = "Left trigger moves left; right trigger moves right")]
    public static void LeftAndRight()
    {
        if (!PortTools.Ready || GTPlayer.Instance.headCollider == null) return;
        Vector3 right = Flat(GTPlayer.Instance.headCollider.transform.right);
        float input = (InputLib.RightTrigger ? 1f : 0f) - (InputLib.LeftTrigger ? 1f : 0f);
        GorillaTagger.Instance.rigidbody.AddForce(right * (input * 50f), ForceMode.Acceleration);
    }

    [MenuButton("Forward And Backwards", ButtonType.Toggle, Category = "Movement", Order = 158, runOption = RunOption.FixedUpdate, Description = "Left trigger moves forward; right trigger moves backwards")]
    public static void ForwardAndBackwards()
    {
        if (!PortTools.Ready || GTPlayer.Instance.headCollider == null) return;
        Vector3 forward = Flat(GTPlayer.Instance.headCollider.transform.forward);
        float input = (InputLib.LeftTrigger ? 1f : 0f) - (InputLib.RightTrigger ? 1f : 0f);
        GorillaTagger.Instance.rigidbody.AddForce(forward * (input * 50f), ForceMode.Acceleration);
    }

    [MenuButton("Velocity Movement", ButtonType.Toggle, Category = "Movement", Order = 159, runOption = RunOption.FixedUpdate, Description = "Use the direction, input, speed and acceleration configured in Settings")]
    public static void VelocityMovement()
    {
        if (!PortTools.Ready || GTPlayer.Instance.headCollider == null) { StopVelocity(); return; }
        PortTools.Register(Clear);
        if (!ModInput.Held(ExtraMovementSettings.VelocityInput)) { StopVelocity(); return; }
        var player = GTPlayer.Instance;
        Vector3 direction;
        if (ExtraMovementSettings.VelocityDirection == 2)
        {
            Vector2 stick = Deadzone(InputLib.LeftJoyStickAxis);
            direction = Flat(player.headCollider.transform.forward) * stick.y + Flat(player.headCollider.transform.right) * stick.x;
            direction = Vector3.ClampMagnitude(direction, 1f);
        }
        else
        {
            Transform target = ExtraMovementSettings.VelocityDirection == 1 ? player.headCollider.transform : player.RightHand.controllerTransform;
            if (target == null) { StopVelocity(); return; }
            direction = target.forward.normalized;
        }
        var body = GorillaTagger.Instance.rigidbody;
        if (velocityBody != body) StopVelocity();
        velocityBody = body;
        Vector3 targetVelocity = direction * ExtraMovementSettings.VelocitySpeed;
        if (!Finite(targetVelocity)) { StopVelocity(); return; }
        body.linearVelocity = Vector3.MoveTowards(body.linearVelocity, targetVelocity, ExtraMovementSettings.VelocityAcceleration * Time.fixedDeltaTime);
    }
    [MenuDisable("Velocity Movement")]
    public static void StopVelocity()
    {
        if (velocityBody != null && ExtraMovementSettings.VelocityRelease == 0) velocityBody.linearVelocity = Vector3.zero;
        velocityBody = null;
    }

    [MenuButton("Throw", ButtonType.Toggle, Category = "Movement", Order = 160, runOption = RunOption.FixedUpdate, Description = "Quick hand movements away from surfaces throw you in that direction")]
    public static void Throw()
    {
        if (!PortTools.Ready || GTPlayer.Instance.bodyCollider == null) { StopThrow(); return; }
        PortTools.Register(Clear);
        var player = GTPlayer.Instance;
        if (throwOwner != player) { StopThrow(); throwOwner = player; }
        for (int i = 0; i < 2; i++)
        {
            Transform hand = i == 0 ? player.LeftHand.controllerTransform : player.RightHand.controllerTransform;
            if (hand == null) { handReady[i] = false; continue; }
            Vector3 position = hand.position - player.bodyCollider.transform.position;
            if (!Finite(position)) { handReady[i] = false; continue; }
            if (!handReady[i])
            {
                handPositions[i] = position;
                handReady[i] = true;
                blockedUntil[i] = Time.fixedTime + 0.15f;
                continue;
            }
            Vector3 delta = position - handPositions[i];
            handPositions[i] = position;
            if (delta.sqrMagnitude > 1f)
            {
                swinging[i] = true;
                blockedUntil[i] = Time.fixedTime + 0.15f;
                continue;
            }
            Vector3 velocity = delta / Mathf.Max(Time.fixedDeltaTime, 0.001f);
            float speed = velocity.magnitude;
            if (player.IsHandTouching(i == 0))
            {
                swinging[i] = true;
                blockedUntil[i] = Time.fixedTime + 0.12f;
                continue;
            }
            if (speed < 1.5f) swinging[i] = false;
            if (speed < 3f || swinging[i] || Time.fixedTime < blockedUntil[i]) continue;
            Vector3 burst = Vector3.ClampMagnitude(velocity * ExtraMovementSettings.ThrowPower, 12f);
            GorillaTagger.Instance.rigidbody.AddForce(burst, ForceMode.VelocityChange);
            swinging[i] = true;
            blockedUntil[i] = Time.fixedTime + 0.25f;
        }
    }
    [MenuDisable("Throw")]
    public static void StopThrow()
    {
        throwOwner = null;
        for (int i = 0; i < 2; i++)
        {
            handReady[i] = swinging[i] = false;
            handPositions[i] = Vector3.zero;
            blockedUntil[i] = 0f;
        }
    }

    [MenuButton("Double Jump", ButtonType.Toggle, Category = "Movement", Order = 161, runOption = RunOption.FixedUpdate, Description = "Press the selected button for one extra airborne jump")]
    public static void DoubleJump()
    {
        if (!PortTools.Ready) { StopDoubleJump(); return; }
        PortTools.Register(Clear);
        var player = GTPlayer.Instance;
        bool held = ModInput.Held(ExtraMovementSettings.JumpInput + 4);
        if (jumpOwner != player || !jumpSeeded)
        {
            jumpOwner = player;
            jumpSeeded = true;
            jumpHeld = held;
            extraJumpUsed = false;
            return;
        }
        bool grounded = player.IsGroundedHand || player.IsGroundedButt;
        if (grounded && Time.fixedTime - lastJump > 0.2f) extraJumpUsed = false;
        if (held && !jumpHeld && !grounded && !extraJumpUsed)
        {
            var body = GorillaTagger.Instance.rigidbody;
            Vector3 velocity = body.linearVelocity;
            velocity.y = Mathf.Max(velocity.y, ExtraMovementSettings.JumpPower);
            body.linearVelocity = velocity;
            extraJumpUsed = true;
            lastJump = Time.fixedTime;
        }
        jumpHeld = held;
    }
    [MenuDisable("Double Jump")]
    public static void StopDoubleJump()
    {
        jumpOwner = null;
        jumpHeld = jumpSeeded = extraJumpUsed = false;
        lastJump = 0f;
    }

    [MenuEnable("Fly Towards Gun")]
    public static void StartFlyGun()
    {
        PortTools.ClaimPointer("Fly Towards Gun");
        foreach (var button in ButtonManager.Buttons)
            if (button.Category == "Rig" && button.Type == ButtonType.Toggle && button.Name.EndsWith(" Gun", StringComparison.Ordinal)) ButtonManager.SetEnabled(button, false);
    }
    [MenuButton("Fly Towards Gun", ButtonType.Toggle, Category = "Movement", Order = 162, runOption = RunOption.FixedUpdate, Description = "Aim and hold the gun trigger to fly toward its hit point")]
    public static void FlyTowardsGun()
    {
        if (!PortTools.Ready || GTPlayer.Instance.bodyCollider == null) { StopFlyGun(); return; }
        PortTools.Register(Clear);
        bool selected = GunLib.UpdateGun(GunLib.AimHand, GunLib.AimPressed, GunLib.SelectPressed, out Vector3 point);
        if (!selected) { gun.Release(); return; }
        var body = GorillaTagger.Instance.rigidbody;
        Vector3 offset = point - GTPlayer.Instance.bodyCollider.transform.position;
        gun.Take(body);
        float distance = offset.magnitude;
        body.linearVelocity = distance > 0.3f ? offset.normalized * Mathf.Min(MovementMods.FlySpeed, (distance - 0.3f) * 4f) : Vector3.zero;
    }
    [MenuDisable("Fly Towards Gun")]
    public static void StopFlyGun() { gun.Release(); GunLib.Hide(); }

    private static Vector2 Deadzone(Vector2 value) => value.sqrMagnitude < 0.04f ? Vector2.zero : Vector2.ClampMagnitude(value, 1f);
    private static Vector3 Flat(Vector3 value) => Vector3.ProjectOnPlane(value, Vector3.up).normalized;
    private static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);
    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

    internal static void Clear()
    {
        StopDrone(); StopVelocity(); StopThrow(); StopDoubleJump(); StopFlyGun();
    }

    private sealed class FlightState
    {
        internal Rigidbody? Body;
        private bool gravity;

        internal void Take(Rigidbody body)
        {
            if (Body != body) { Release(); Body = body; gravity = body.useGravity; }
            body.useGravity = false;
        }
        internal void Release()
        {
            if (Body != null) { Body.useGravity = gravity; Body.linearVelocity = Vector3.zero; }
            Body = null;
        }
    }
}
