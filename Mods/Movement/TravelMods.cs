// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Movement;

public static class TravelMods
{
    private static Vector2 drive;
    private static bool reversed, holding, flying, rightPearl;
    private static float reverseAfter, expires;
    private static GameObject? pearl;
    private static Material? pearlMaterial;
    private static Vector3 velocity, launchPosition;
    private static object? pearlRoom;
    private static float driveSpeed = 6f, brakeStrength = 6f;
    private static readonly Vector3[] pearlSamples = new Vector3[8];
    private static readonly float[] pearlSampleTimes = new float[8];
    private static int pearlSampleIndex, pearlSampleCount;
    private static bool previousRightGrip, previousLeftGrip;
    private static LayerMask pearlLayers;

    [MenuButton("Car", ButtonType.Toggle, Category = "Movement", Order = 130, runOption = RunOption.FixedUpdate, Description = "Use the left joystick to drive along the ground")]
    public static void Car()
    {
        if (!PortTools.Ready) { StopCar(); return; }
        PortTools.Register(Cleanup);
        var player = GTPlayer.Instance;
        var body = GorillaTagger.Instance.rigidbody;
        if (player.bodyCollider == null) return;
        drive = Vector2.Lerp(drive, InputLib.LeftJoyStickAxis, 1f - Mathf.Exp(-8f * Time.fixedDeltaTime));
        if (!Physics.Raycast(player.bodyCollider.transform.position, Vector3.down, out _, player.bodyCollider.bounds.extents.y + 0.15f, player.locomotionEnabledLayers, QueryTriggerInteraction.Ignore)) return;
        Vector3 forward = Vector3.ProjectOnPlane(player.bodyCollider.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 motion = (forward * drive.y + right * drive.x) * driveSpeed;
        body.linearVelocity = new Vector3(motion.x, body.linearVelocity.y, motion.z);
    }
    [MenuDisable("Car")]
    public static void StopCar() => drive = Vector2.zero;

    [MenuButton("Reverse Velocity", ButtonType.Toggle, Category = "Movement", Order = 131, runOption = RunOption.FixedUpdate, Description = "Press X to reverse momentum; release before pressing again")]
    public static void ReverseVelocity()
    {
        if (!PortTools.Ready) { StopReverse(); return; }
        PortTools.Register(Cleanup);
        bool pressed = InputLib.LeftControllerXButton;
        if (pressed && !reversed && Time.unscaledTime >= reverseAfter)
        {
            GorillaTagger.Instance.rigidbody.linearVelocity *= -1f;
            reverseAfter = Time.unscaledTime + 0.35f;
        }
        reversed = pressed;
    }
    [MenuDisable("Reverse Velocity")]
    public static void StopReverse() { reversed = false; reverseAfter = 0f; }

    [MenuButton("Air Brake", ButtonType.Toggle, Category = "Movement", Order = 132, Controls = "(RG)(LG)", runOption = RunOption.FixedUpdate, Description = "Hold both grips together to slow your momentum")]
    public static void AirBrake()
    {
        if (!PortTools.Ready || !InputLib.LeftGrab || !InputLib.RightGrab) return;
        GorillaTagger.Instance.rigidbody.linearVelocity *= Mathf.Exp(-brakeStrength * Time.fixedDeltaTime);
    }

    [MenuButton("Car Speed", ButtonType.Option, Category = "Settings", Order = 153, Options = new[] { "6", "3", "9", "12" }, Notify = false)]
    public static void SetCarSpeed() => driveSpeed = new[] { 6f, 3f, 9f, 12f }[ButtonManager.GetButton("Car Speed")?.OptionIndex ?? 0];
    [MenuButton("Air Brake Strength", ButtonType.Option, Category = "Settings", Order = 154, Options = new[] { "6", "3", "9", "12" }, Notify = false)]
    public static void SetBrake() => brakeStrength = new[] { 6f, 3f, 9f, 12f }[ButtonManager.GetButton("Air Brake Strength")?.OptionIndex ?? 0];

    [MenuButton("Ender Pearl", ButtonType.Toggle, Category = "Movement", Order = 133, runOption = RunOption.FixedUpdate, Description = "Hold either grip, then release that hand to throw")]
    public static void EnderPearl()
    {
        if (!PortTools.Ready || !ReferenceEquals(pearlRoom, PhotonNetwork.CurrentRoom) && pearl != null) { StopPearl(); return; }
        PortTools.Register(Cleanup);
        bool right = InputLib.RightGrab, left = InputLib.LeftGrab;
        bool rightPressed = right && !previousRightGrip, leftPressed = left && !previousLeftGrip;
        previousRightGrip = right;
        previousLeftGrip = left;
        if (pearl == null && (rightPressed || leftPressed))
        {
            rightPearl = rightPressed;
            pearlLayers = NoClipState.CollisionLayers;
            pearlSampleIndex = pearlSampleCount = 0;
            CreatePearl();
            holding = pearl != null;
            flying = false;
        }
        if (pearl == null) return;
        if (holding)
        {
            var hand = rightPearl ? GTPlayer.Instance.RightHand.controllerTransform : GTPlayer.Instance.LeftHand.controllerTransform;
            if (hand == null || pearl == null) { StopPearl(); return; }
            pearl.transform.position = hand.position;
            SamplePearlHand(hand.position);
            if (rightPearl ? right : left) return;
            holding = false;
            flying = true;
            velocity = PearlThrowVelocity(hand);
            launchPosition = pearl.transform.position;
            expires = Time.unscaledTime + 10f;
        }
        if (!flying) return;
        if (!Finite(velocity) || Time.unscaledTime >= expires || (pearl.transform.position - launchPosition).sqrMagnitude > 6400f) { StopPearl(); return; }
        velocity += Physics.gravity * Time.fixedDeltaTime;
        Vector3 step = velocity * Time.fixedDeltaTime;
        if (step.sqrMagnitude > 0.000001f && Physics.SphereCast(pearl.transform.position, 0.04f, step.normalized, out RaycastHit hit, step.magnitude, pearlLayers, QueryTriggerInteraction.Ignore))
        {
            Vector3 destination = hit.point + hit.normal * 0.4f;
            if (Finite(destination) && !Physics.CheckSphere(destination, 0.15f, pearlLayers, QueryTriggerInteraction.Ignore)) PortTools.Teleport(destination);
            StopPearl();
            return;
        }
        pearl.transform.position += step;
    }

    private static void CreatePearl()
    {
        pearlRoom = PhotonNetwork.CurrentRoom;
        pearl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pearl.name = "ChudOS Ender Pearl";
        pearl.layer = 2;
        var collider = pearl.GetComponent<Collider>();
        if (collider != null) { collider.enabled = false; Object.Destroy(collider); }
        pearl.transform.localScale = Vector3.one * 0.08f;
        var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
        if (shader == null) { StopPearl(); return; }
        pearlMaterial = new Material(shader) { color = ChudOS.Core.Main.PrimaryColorValue };
        pearl.GetComponent<Renderer>().sharedMaterial = pearlMaterial;
    }

    private static void SamplePearlHand(Vector3 position)
    {
        pearlSamples[pearlSampleIndex] = position;
        pearlSampleTimes[pearlSampleIndex] = Time.unscaledTime;
        pearlSampleIndex = (pearlSampleIndex + 1) % pearlSamples.Length;
        pearlSampleCount = Mathf.Min(pearlSampleCount + 1, pearlSamples.Length);
    }
    private static Vector3 SampledPearlVelocity()
    {
        if (pearlSampleCount < 2) return Vector3.zero;
        int newest = (pearlSampleIndex + pearlSamples.Length - 1) % pearlSamples.Length;
        for (int offset = pearlSampleCount - 1; offset >= 1; offset--)
        {
            int older = (newest - offset + pearlSamples.Length) % pearlSamples.Length;
            float duration = pearlSampleTimes[newest] - pearlSampleTimes[older];
            if (duration <= 0.0001f || duration > 0.12f) continue;
            return (pearlSamples[newest] - pearlSamples[older]) / duration;
        }
        return Vector3.zero;
    }
    private static Vector3 PearlThrowVelocity(Transform hand)
    {
        Vector3 result = SampledPearlVelocity();
        var tracker = rightPearl ? GTPlayer.Instance.RightHand.velocityTracker : GTPlayer.Instance.LeftHand.velocityTracker;
        if (tracker != null)
        {
            Vector3 tracked = tracker.GetAverageVelocity(true, 0.08f);
            if (Finite(tracked) && (!Finite(result) || tracked.sqrMagnitude > result.sqrMagnitude)) result = tracked;
        }
        if (!Finite(result) || result.sqrMagnitude < 0.04f) result = hand.forward * 3f;
        return Vector3.ClampMagnitude(result, 20f);
    }

    [MenuDisable("Ender Pearl")]
    public static void StopPearl()
    {
        if (pearl != null) Object.Destroy(pearl);
        if (pearlMaterial != null) Object.Destroy(pearlMaterial);
        pearl = null;
        pearlMaterial = null;
        pearlRoom = null;
        holding = flying = false;
        previousRightGrip = previousLeftGrip = false;
        pearlSampleIndex = pearlSampleCount = 0;
        velocity = Vector3.zero;
    }

    private static bool Finite(Vector3 value) => !float.IsNaN(value.x) && !float.IsInfinity(value.x) && !float.IsNaN(value.y) && !float.IsInfinity(value.y) && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
    private static void Cleanup() { StopCar(); StopReverse(); StopPearl(); }
}
