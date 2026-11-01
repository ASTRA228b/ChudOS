// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.OP;

internal sealed class LucyControls : BetaFeature
{
    private static readonly string[] Controllers = { "Grab Lucy", "Move Lucy Gun", "Freeze Lucy", "Pet Lucy", "Lucy Physics" };
    private HalloweenGhostChaser? ghost;
    private Rigidbody? proxy;
    private PhysicsMaterial? bounce;
    private float speed, catchDistance;
    private HalloweenGhostChaser.ChaseState state;
    private Transform? follow;
    private NetPlayer? target;
    private Vector3 originalPosition, previousHand, throwVelocity;
    private Quaternion originalRotation;
    private bool grabbing, captured, lastBounce;

    private static HalloweenGhostChaser? OwnedGhost()
    {
        if (!CreativeTools.Ready) { CreativeTools.Notice("Player tracking is not ready."); return null; }
        HalloweenGhostChaser? closest = null;
        float distance = float.MaxValue;
        foreach (var candidate in Object.FindObjectsByType<HalloweenGhostChaser>(FindObjectsSortMode.None))
        {
            float current = (candidate.transform.position - CreativeTools.Position).sqrMagnitude;
            if (current < distance) { distance = current; closest = candidate; }
        }
        if (closest == null) CreativeTools.Notice("No active Lucy is loaded in this map.");
        else if (!closest.IsLocallyOwned) { CreativeTools.Notice("Lucy is not locally owned."); return null; }
        return closest;
    }
    private static void StartController(string name)
    {
        foreach (string other in Controllers) if (other != name) ButtonManager.SetEnabled(other, false);
        Enable<LucyControls>(name);
    }
    [MenuButton("Summon Lucy", ButtonType.Press, Category = "OP", Order = 10, Description = "Summon the loaded ghost when its network ownership is local.")]
    public static void Summon()
    {
        var ghost = OwnedGhost();
        if (ghost == null) return;
        ghost.timeGongStarted = Time.time; ghost.currentState = HalloweenGhostChaser.ChaseState.Gong; ghost.isSummoned = false;
    }
    [MenuButton("Despawn Lucy", ButtonType.Press, Category = "OP", Order = 11)]
    public static void Despawn()
    {
        foreach (string name in Controllers) ButtonManager.SetEnabled(name, false);
        var ghost = OwnedGhost();
        if (ghost == null) return;
        ghost.currentState = HalloweenGhostChaser.ChaseState.Dormant; ghost.isSummoned = false; ghost.ResetPath();
    }
    [MenuButton("Grab Lucy", ButtonType.Toggle, Category = "OP", Order = 12, runOption = RunOption.Once, Controls = "(RG)/(RMB)", Description = "Hold right grip to position locally owned Lucy at your hand.")]
    public static void EnableGrab() => StartController("Grab Lucy");
    [MenuDisable("Grab Lucy")]
    public static void DisableGrab() => Disable("Grab Lucy");
    [MenuButton("Move Lucy Gun", ButtonType.Toggle, Category = "OP", Order = 13, runOption = RunOption.Once, Controls = "(RG)(RT)")]
    public static void EnableMove() => StartController("Move Lucy Gun");
    [MenuDisable("Move Lucy Gun")]
    public static void DisableMove() => Disable("Move Lucy Gun");
    [MenuButton("Freeze Lucy", ButtonType.Toggle, Category = "OP", Order = 14, runOption = RunOption.Once)]
    public static void EnableFreeze() => StartController("Freeze Lucy");
    [MenuDisable("Freeze Lucy")]
    public static void DisableFreeze() => Disable("Freeze Lucy");
    [MenuButton("Pet Lucy", ButtonType.Toggle, Category = "OP", Order = 15, runOption = RunOption.Once, Description = "Have locally owned Lucy follow you without catching you.")]
    public static void EnablePet() => StartController("Pet Lucy");
    [MenuDisable("Pet Lucy")]
    public static void DisablePet() => Disable("Pet Lucy");
    [MenuButton("Lucy Physics", ButtonType.Toggle, Category = "OP", Order = 16, runOption = RunOption.Once, Controls = "(RG)/(RMB)", Description = "Local physics controller for an owned ghost. Grab nearby, then release to throw.")]
    public static void EnablePhysics() => StartController("Lucy Physics");
    [MenuDisable("Lucy Physics")]
    public static void DisablePhysics() => Disable("Lucy Physics");

    protected override void Setup()
    {
        ghost = OwnedGhost();
        if (ghost == null) { Unavailable("an active, locally owned Lucy is required."); return; }
        speed = ghost.currentSpeed; catchDistance = ghost.catchDistance; state = ghost.currentState;
        follow = ghost.followTarget; target = ghost.targetPlayer; originalPosition = ghost.transform.position; originalRotation = ghost.transform.rotation;
        previousHand = CreativeTools.Right.position; captured = true;
        if (Mode == "Lucy Physics")
        {
            var obj = Shape("Lucy Physics Proxy", PrimitiveType.Sphere, ghost.transform.position, Vector3.one * 1.2f, true, true);
            obj.GetComponent<Renderer>().enabled = false; proxy = obj.GetComponent<Rigidbody>();
            bounce = new PhysicsMaterial("ChudOS Lucy Bounce") { bounceCombine = PhysicsMaterialCombine.Maximum };
            obj.GetComponent<Collider>().sharedMaterial = bounce;
            lastBounce = ExpansionPreferences.LucyBounce; bounce.bounciness = lastBounce ? 0.7f : 0f;
        }
    }
    protected override void Tick()
    {
        if (ghost == null || !ghost.IsLocallyOwned) { Unavailable("Lucy ownership changed."); return; }
        if (Mode == "Pet Lucy") { ghost.catchDistance = 0f; ghost.targetPlayer = NetworkSystem.Instance.LocalPlayer; ghost.followTarget = CreativeTools.Head; ghost.currentState = HalloweenGhostChaser.ChaseState.Chasing; }
        else if (Mode == "Freeze Lucy") ghost.currentSpeed = 0f;
        else if (Mode == "Move Lucy Gun" && GunLib.UpdateGun(GunLib.AimHand, GunLib.AimPressed, GunLib.SelectPressed, out var point)) { ghost.currentSpeed = 0f; ghost.catchDistance = 0f; ghost.transform.position = point + Vector3.up; }
        if (bounce != null && lastBounce != ExpansionPreferences.LucyBounce) { lastBounce = ExpansionPreferences.LucyBounce; bounce.bounciness = lastBounce ? 0.7f : 0f; }
    }
    private void LateUpdate()
    {
        if (!enabled || !captured || ghost == null || !ghost.IsLocallyOwned || !CreativeTools.Ready) return;
        if (Mode == "Freeze Lucy") ghost.currentSpeed = 0f;
        if (Mode == "Grab Lucy")
        {
            ghost.currentSpeed = RG ? 0f : speed; ghost.catchDistance = RG ? 0f : catchDistance;
            if (RG) ghost.transform.SetPositionAndRotation(CreativeTools.Right.position, CreativeTools.Right.rotation);
        }
        if (Mode == "Lucy Physics" && proxy != null) { ghost.currentSpeed = 0f; ghost.catchDistance = 0f; ghost.transform.SetPositionAndRotation(proxy.position, proxy.rotation); }
    }
    protected override void PhysicsTick()
    {
        if (proxy == null || ghost == null || !ghost.IsLocallyOwned) return;
        Vector3 hand = CreativeTools.Right.position;
        Vector3 velocity = Vector3.ClampMagnitude((hand - previousHand) / Mathf.Max(Time.fixedDeltaTime, 0.001f), 15f);
        previousHand = hand;
        if (!grabbing && RG && Vector3.Distance(hand, proxy.position) < 1f) grabbing = true;
        if (!grabbing) return;
        if (RG) { throwVelocity = velocity; proxy.isKinematic = true; proxy.position = hand; proxy.rotation = CreativeTools.Right.rotation; }
        else { proxy.isKinematic = false; proxy.linearVelocity = throwVelocity * ExpansionPreferences.ThrowPower; grabbing = false; }
    }
    protected override void Cleanup()
    {
        if (captured && ghost != null && ghost.IsLocallyOwned)
        {
            ghost.currentSpeed = speed; ghost.catchDistance = catchDistance; ghost.followTarget = follow!; ghost.targetPlayer = target!; ghost.currentState = state;
            if (Mode is "Grab Lucy" or "Move Lucy Gun" or "Lucy Physics") ghost.transform.SetPositionAndRotation(originalPosition, originalRotation);
            ghost.ResetPath();
        }
        if (bounce != null) Object.Destroy(bounce);
        if (Mode == "Move Lucy Gun") GunLib.Hide();
    }
}
