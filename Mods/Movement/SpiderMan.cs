// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Movement;

public static class SpiderMan
{
    private static readonly Transform?[] surfaces = new Transform?[2];
    private static readonly Vector3[] anchors = new Vector3[2];
    private static readonly float[] lengths = new float[2];
    private static readonly LineRenderer?[] webs = new LineRenderer?[2];
    private static GTPlayer? owner;
    private static Rigidbody? body;

    [MenuButton("Spider Man", ButtonType.Toggle, Category = "Movement", Order = 165, runOption = RunOption.FixedUpdate, Description = "Hold either trigger to attach a surface web and swing; release to keep your momentum")]
    public static void Tick()
    {
        if (!PortTools.Ready || GTPlayer.Instance.bodyCollider == null) { Stop(); return; }
        var player = GTPlayer.Instance;
        var currentBody = GorillaTagger.Instance.rigidbody;
        if (owner != player || body != currentBody) { Stop(); owner = player; body = currentBody; }
        if (!Finite(player.bodyCollider.transform.position) || !Finite(body.linearVelocity)) { Stop(); return; }
        Swing(0, player.LeftHand.controllerTransform, InputLib.LeftTrigger);
        Swing(1, player.RightHand.controllerTransform, InputLib.RightTrigger);
    }

    [MenuEnable("Spider Man")]
    public static void Enable() => PortTools.Register(Stop);

    private static void Swing(int index, Transform hand, bool held)
    {
        if (!held || hand == null || !Finite(hand.position) || !Finite(hand.forward)) { Release(index); return; }
        float scale = Mathf.Clamp(owner!.scale, 0.2f, 3f);
        Vector3 position = owner.bodyCollider.transform.position;
        if (surfaces[index] == null)
        {
            Release(index);
            if (!Physics.Raycast(hand.position, hand.forward, out RaycastHit hit, 45f * scale, NoClipState.CollisionLayers, QueryTriggerInteraction.Ignore)) return;
            if (hit.collider == null || hit.collider.GetComponentInParent<VRRig>() != null || hit.transform == null || !Finite(hit.point)) return;
            surfaces[index] = hit.transform;
            anchors[index] = hit.transform.InverseTransformPoint(hit.point);
            lengths[index] = Mathf.Max(0.5f * scale, Vector3.Distance(position, hit.point) * 0.95f);
        }

        Vector3 anchor = surfaces[index]!.TransformPoint(anchors[index]);
        if (!Finite(anchor)) { Release(index); return; }
        body!.AddForce(Tension(anchor - position, body.linearVelocity, lengths[index], scale), ForceMode.Acceleration);
        LineRenderer line = webs[index] ??= PortTools.Line("ChudOS Spider Web", 0.012f);
        if (line == null) webs[index] = line = PortTools.Line("ChudOS Spider Web", 0.012f);
        line.enabled = true;
        line.startColor = line.endColor = Color.white;
        line.SetPosition(0, hand.position);
        line.SetPosition(1, anchor);
    }

    internal static Vector3 Tension(Vector3 offset, Vector3 velocity, float length, float scale)
    {
        float distance = offset.magnitude;
        if (distance <= length || distance < 0.001f) return Vector3.zero;
        Vector3 direction = offset / distance;
        float stretch = (distance - length) / Mathf.Max(scale, 0.2f);
        float pull = Mathf.Clamp(stretch * 14f - Vector3.Dot(velocity, direction) * 2f, 0f, 45f);
        return direction * (pull * scale);
    }

    private static void Release(int index)
    {
        surfaces[index] = null;
        anchors[index] = Vector3.zero;
        lengths[index] = 0f;
        if (webs[index] != null) webs[index]!.enabled = false;
    }
    [MenuDisable("Spider Man")]
    public static void Stop()
    {
        for (int i = 0; i < 2; i++)
        {
            Release(i);
            if (webs[i] != null) Object.Destroy(webs[i]!.gameObject);
            webs[i] = null;
        }
        owner = null;
        body = null;
    }

    private static bool Finite(Vector3 value) => !float.IsNaN(value.x) && !float.IsInfinity(value.x) && !float.IsNaN(value.y) && !float.IsInfinity(value.y) && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
}
