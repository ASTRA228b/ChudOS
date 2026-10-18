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

using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaLocomotion;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object; // I'm so smart guys!

namespace ChudOS.Mods.Movement;

public static class NewMovementMods
{
    private static readonly Dictionary<string, Action> restore = new();
    private static readonly Dictionary<GliderHoldable, (float lift, float drag)> gliders = new();
    private static readonly Dictionary<SphereCollider, float> ropes = new();
    private static readonly Vector3[] anchors = new Vector3[2];
    private static readonly bool[] grappling = new bool[2];
    private static readonly LineRenderer?[] grappleLines = new LineRenderer?[2];
    private static Vector3 wallNormal;
    private static LineRenderer? checkpointMarker;
    private static Vector3 checkpoint;
    private static bool hasCheckpoint, wasTeleporting, wasReturning;
    private static float size = 1f, nextGliderScan;
    private static Frame[]? history;
    private static int historyHead, historyCount;

    private struct Frame
    {
        internal Vector3 Body, Left, Right, Velocity;
        internal Quaternion LeftRotation, RightRotation;
    }

    private static void Capture(string key, Action action)
    {
        if (restore.ContainsKey(key)) return;
        PortTools.Register(Cleanup);
        restore.Add(key, action);
    }

    private static void Restore(string key)
    {
        if (!restore.TryGetValue(key, out Action? action)) return;
        restore.Remove(key);
        action();
    }

    [MenuButton("Wall Walk", ButtonType.Toggle, Category = "Movement", Order = 100, runOption = RunOption.FixedUpdate, Description = "Hold right grip against a surface to walk along it")]
    public static void WallWalk()
    {
        if (!PortTools.Ready) return;
        PortTools.Register(Cleanup);
        GTPlayer p = GTPlayer.Instance;
        if (p.IsHandTouching(true)) wallNormal = p.GetTouchHitInfo(true).normal;
        else if (p.IsHandTouching(false)) wallNormal = p.GetTouchHitInfo(false).normal;
        if (InputLib.RightGrab && wallNormal.sqrMagnitude > 0.1f)
            GorillaTagger.Instance.rigidbody.AddForce(-Physics.gravity - wallNormal * 9.81f, ForceMode.Acceleration);
    }
    [MenuDisable("Wall Walk")]
    public static void StopWallWalk() => wallNormal = Vector3.zero;

    [MenuButton("Grappling Hooks", ButtonType.Toggle, Category = "Movement", Order = 101, runOption = RunOption.FixedUpdate, Description = "Hold either grip to pull toward a hand anchor")]
    public static void GrapplingHooks()
    {
        if (!PortTools.Ready) return;
        PortTools.Register(Cleanup);
        Grapple(0, GorillaTagger.Instance.leftHandTransform, InputLib.LeftGrab);
        Grapple(1, GorillaTagger.Instance.rightHandTransform, InputLib.RightGrab);
    }
    private static void Grapple(int index, Transform hand, bool held)
    {
        if (!held)
        {
            grappling[index] = false;
            if (grappleLines[index] != null) grappleLines[index]!.enabled = false;
            return;
        }
        var body = GorillaTagger.Instance.rigidbody;
        if (!grappling[index])
        {
            anchors[index] = hand.position + hand.forward * 16f;
            body.linearVelocity += hand.forward * 5f;
            grappling[index] = true;
        }
        else body.linearVelocity += (anchors[index] - hand.position).normalized * (0.5f * Time.fixedDeltaTime * 90f);
        LineRenderer line = grappleLines[index] ??= PortTools.Line("ChudOS Grapple", 0.025f);

        if (line == null) grappleLines[index] = line = PortTools.Line("ChudOS Grapple", 0.025f);
        line.enabled = true;
        line.SetPosition(0, hand.position);
        line.SetPosition(1, anchors[index]);
    }
    [MenuDisable("Grappling Hooks")]
    public static void StopGrappling()
    {
        for (int i = 0; i < 2; i++)
        {
            grappling[i] = false;
            if (grappleLines[i] != null) Object.Destroy(grappleLines[i]!.gameObject);
            grappleLines[i] = null;
        }
    }

    [MenuButton("Rewind", ButtonType.Toggle, Category = "Movement", Order = 102, runOption = RunOption.FixedUpdate, Description = "Hold right trigger to rewind your recorded movement")]
    public static void Rewind()
    {
        if (!PortTools.Ready) return;
        PortTools.Register(Cleanup);
        history ??= new Frame[8640];
        var tagger = GorillaTagger.Instance;
        Transform left = tagger.leftHandTransform, right = tagger.rightHandTransform;
        if (InputLib.RightTrigger)
        {
            if (historyCount == 0) return;
            historyHead = (historyHead + history.Length - 1) % history.Length;
            Frame frame = history[historyHead];
            historyCount--;
            PortTools.Teleport(frame.Body);
            left.SetPositionAndRotation(frame.Left, frame.LeftRotation);
            right.SetPositionAndRotation(frame.Right, frame.RightRotation);
            tagger.rigidbody.linearVelocity = -frame.Velocity;
        }
        else
        {
            history[historyHead] = new Frame
            {
                Body = GTPlayer.Instance.bodyCollider.transform.position,
                Left = left.position, Right = right.position,
                LeftRotation = left.rotation, RightRotation = right.rotation,
                Velocity = tagger.rigidbody.linearVelocity
            };
            historyHead = (historyHead + 1) % history.Length;
            historyCount = Math.Min(historyCount + 1, history.Length);
        }
    }
    [MenuDisable("Rewind")]
    public static void StopRewind() { historyHead = historyCount = 0; history = null; }

    [MenuEnable("Teleport Gun")]
    public static void StartTeleportGun() { PortTools.Register(Cleanup); PortTools.ClaimPointer("Teleport Gun"); wasTeleporting = false; }

    [MenuButton("Teleport Gun", ButtonType.Toggle, Category = "Movement", Order = 103, Description = "Right grip aims; press right trigger to teleport")]
    public static void TeleportGun()
    {
        if (!PortTools.Ready) return;
        bool held = InputLib.RightGrab, trigger = InputLib.RightTrigger;
        bool selected = GunLib.UpdateGun(GorillaTagger.Instance.rightHandTransform, held, trigger, out Vector3 point);
        if (selected && !wasTeleporting) PortTools.Teleport(point + Vector3.up * GTPlayer.Instance.scale);
        wasTeleporting = held && trigger;
    }
    [MenuDisable("Teleport Gun")]
    public static void StopTeleportGun() { wasTeleporting = false; GunLib.Hide(); }

    [MenuButton("Checkpoint", ButtonType.Toggle, Category = "Movement", Order = 104, Description = "Right grip places a checkpoint; press A to return")]
    public static void Checkpoint()
    {
        if (!PortTools.Ready) return;
        PortTools.Register(Cleanup);
        if (InputLib.RightGrab)
        {
            checkpoint = GorillaTagger.Instance.rightHandTransform.position;
            hasCheckpoint = true;
            if (checkpointMarker == null) checkpointMarker = PortTools.Line("ChudOS Checkpoint", 0.03f, 17);
            for (int i = 0; i < 17; i++)
            {
                float a = i * Mathf.PI / 8f;
                checkpointMarker.SetPosition(i, checkpoint + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.15f);
            }
        }
        bool pressed = InputLib.RightControllerAButton;
        if (hasCheckpoint && pressed && !wasReturning) PortTools.Teleport(checkpoint);
        wasReturning = pressed;
    }
    [MenuDisable("Checkpoint")]
    public static void StopCheckpoint()
    {
        hasCheckpoint = wasReturning = false;
        if (checkpointMarker != null) Object.Destroy(checkpointMarker.gameObject);
        checkpointMarker = null;
    }

    [MenuButton("Size Changer", ButtonType.Toggle, Category = "Movement", Order = 105, runOption = RunOption.FixedUpdate, Description = "Trigger grows; grip shrinks; A resets; left trigger adjusts faster")]
    public static void SizeChanger()
    {
        if (!PortTools.Ready || VRRig.LocalRig == null) return;
        var p = GTPlayer.Instance;
        var rig = VRRig.LocalRig;
        if (!restore.ContainsKey("Size"))
        {
            float multiplier = p.ScaleMultiplier, native = rig.NativeScale;
            Vector3 local = rig.transform.localScale;
            size = native;
            Capture("Size", () => { if (p != null) p.SetScaleMultiplier(multiplier); if (rig != null) { rig.NativeScale = native; rig.transform.localScale = local; } size = 1f; });
        }
        float step = (InputLib.LeftTrigger ? 0.2f : InputLib.LeftGrab ? 0.01f : 0.05f) * Time.fixedDeltaTime * 90f;
        if (InputLib.RightTrigger) size += step;
        if (InputLib.RightGrab) size -= step;
        if (InputLib.RightControllerAButton) size = 1f;
        size = Mathf.Clamp(size, 0.05f, 10f);
        rig.NativeScale = size;
        rig.transform.localScale = Vector3.one * size;
        p.SetScaleMultiplier(size);
    }
    [MenuDisable("Size Changer")]
    public static void StopSizeChanger() => Restore("Size");

    [MenuButton("Bouncy", ButtonType.Toggle, Category = "Movement", Order = 106, runOption = RunOption.FixedUpdate, Description = "Makes body collisions bounce")]
    public static void Bouncy()
    {
        if (!PortTools.Ready || restore.ContainsKey("Bounce")) return;
        Collider collider = GorillaTagger.Instance.bodyCollider;
        var original = collider.sharedMaterial;
        var material = original != null ? new PhysicsMaterial(original.name + " ChudOS Bounce") : new PhysicsMaterial("ChudOS Bounce");
        if (original != null) { material.staticFriction = original.staticFriction; material.frictionCombine = original.frictionCombine; }
        material.bounciness = 1f;
        material.bounceCombine = PhysicsMaterialCombine.Maximum;
        material.dynamicFriction = 0f;
        Capture("Bounce", () => { if (collider != null) collider.sharedMaterial = original; Object.Destroy(material); });
        collider.sharedMaterial = material;
    }
    [MenuDisable("Bouncy")]
    public static void StopBouncy() => Restore("Bounce");

    [MenuButton("Slide Control", ButtonType.Toggle, Category = "Movement", Order = 107, runOption = RunOption.FixedUpdate, Description = "Full steering control while sliding")]
    public static void SlideControl()
    {
        if (!PortTools.Ready) return;
        var player = GTPlayer.Instance;
        if (!restore.ContainsKey("Slide"))
        {
            float original = player.slideControl;
            Capture("Slide", () => { if (player != null) player.slideControl = original; });
        }
        player.slideControl = 1f;
    }
    [MenuDisable("Slide Control")]
    public static void StopSlideControl() => Restore("Slide");

    [MenuButton("Swim Momentum", ButtonType.Toggle, Category = "Movement", Order = 108, runOption = RunOption.FixedUpdate, Description = "Poison swimming velocity drain adjustment")]
    public static void SwimMomentum()
    {
        if (!PortTools.Ready) return;
        if (!restore.ContainsKey("Swim"))
        {
            var resets = new List<Action>();
            foreach (var item in GTPlayer.Instance.swimmingParamsList)
            {
                var captured = item;
                float original = item.swimmingVelocityOutOfWaterDrainRate;
                resets.Add(() => captured.swimmingVelocityOutOfWaterDrainRate = original);
            }
            if (resets.Count == 0) return;
            Capture("Swim", () => { foreach (Action reset in resets) reset(); });
        }
        foreach (var item in GTPlayer.Instance.swimmingParamsList) item.swimmingVelocityOutOfWaterDrainRate = 3f;
    }
    [MenuDisable("Swim Momentum")]
    public static void StopSwimMomentum() => Restore("Swim");

    [MenuButton("Rope Grab Reach", ButtonType.Toggle, Category = "Movement", Order = 109, runOption = RunOption.FixedUpdate, Description = "Increases the rope grab interaction radius")]
    public static void RopeGrabReach()
    {
        if (!PortTools.Ready || EquipmentInteractor.instance == null) return;
        PortTools.Register(Cleanup);
        Expand(EquipmentInteractor.instance.LeftClimber?.GetComponent<SphereCollider>());
        Expand(EquipmentInteractor.instance.RightClimber?.GetComponent<SphereCollider>());
    }
    private static void Expand(SphereCollider? collider)
    {
        if (collider == null) return;
        if (!ropes.ContainsKey(collider)) ropes.Add(collider, collider.radius);
        collider.radius = 0.5f;
    }
    [MenuDisable("Rope Grab Reach")]
    public static void StopRopeGrabReach()
    {
        foreach (var pair in ropes) if (pair.Key != null) pair.Key.radius = pair.Value;
        ropes.Clear();
    }

    [MenuButton("Hoverboard Boost", ButtonType.Toggle, Category = "Movement", Order = 110, runOption = RunOption.FixedUpdate, Description = "Stronger hoverboard paddle boosts")]
    public static void HoverboardBoost()
    {
        if (!PortTools.Ready) return;
        var p = GTPlayer.Instance;
        if (!restore.ContainsKey("Hoverboard"))
        {
            float max = HoverboardFields.Max(p), multiplier = HoverboardFields.Multiplier(p);
            float grace = HoverboardFields.Grace(p), tilt = HoverboardFields.Tilt(p);
            Capture("Hoverboard", () => { if (p != null) { HoverboardFields.Max(p) = max; HoverboardFields.Multiplier(p) = multiplier; HoverboardFields.Grace(p) = grace; HoverboardFields.Tilt(p) = tilt; } });
        }
        HoverboardFields.Max(p) = float.MaxValue;
        HoverboardFields.Multiplier(p) = 5f;
        HoverboardFields.Grace(p) = 0f;
        HoverboardFields.Tilt(p) = 1f;
    }
    [MenuDisable("Hoverboard Boost")]
    public static void StopHoverboardBoost() => Restore("Hoverboard");

    private static class HoverboardFields
    {
        internal static readonly AccessTools.FieldRef<GTPlayer, float> Max = AccessTools.FieldRefAccess<GTPlayer, float>("hoverboardPaddleBoostMax");
        internal static readonly AccessTools.FieldRef<GTPlayer, float> Multiplier = AccessTools.FieldRefAccess<GTPlayer, float>("hoverboardPaddleBoostMultiplier");
        internal static readonly AccessTools.FieldRef<GTPlayer, float> Grace = AccessTools.FieldRefAccess<GTPlayer, float>("hoverboardBoostGracePeriod");
        internal static readonly AccessTools.FieldRef<GTPlayer, float> Tilt = AccessTools.FieldRefAccess<GTPlayer, float>("hoverTiltAdjustsForwardFactor");
    }

    [MenuButton("Glider Tuning", ButtonType.Toggle, Category = "Movement", Order = 111, Description = "Increases glider lift and reduces drag")]
    public static void GliderTuning()
    {
        if (!PortTools.Ready || Time.unscaledTime < nextGliderScan) return;
        PortTools.Register(Cleanup);
        nextGliderScan = Time.unscaledTime + 1f;
        foreach (GliderHoldable glider in Object.FindObjectsByType<GliderHoldable>(FindObjectsSortMode.None))
        {
            if (!gliders.ContainsKey(glider)) gliders.Add(glider, (glider.pullUpLiftBonus, glider.dragVsSpeedDragFactor));
            glider.pullUpLiftBonus = 0.5f;
            glider.dragVsSpeedDragFactor = 0.05f;
        }
    }
    [MenuDisable("Glider Tuning")]
    public static void StopGliderTuning()
    {
        foreach (var pair in gliders)
            if (pair.Key != null) { pair.Key.pullUpLiftBonus = pair.Value.lift; pair.Key.dragVsSpeedDragFactor = pair.Value.drag; }
        gliders.Clear();
        nextGliderScan = 0f;
    }

    private static void Cleanup()
    {
        foreach (Action action in restore.Values) action();
        restore.Clear();
        StopWallWalk(); StopGrappling(); StopRewind(); StopTeleportGun(); StopCheckpoint();
        StopRopeGrabReach(); StopGliderTuning();
    }
}
