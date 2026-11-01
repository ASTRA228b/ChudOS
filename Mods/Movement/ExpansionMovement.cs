// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using GorillaLocomotion.Swimming;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Movement;

internal sealed class ExpansionMovement : BetaFeature
{
    private readonly List<Transform> anchors = new();
    private Vector3 previousLeft, previousRight, anchor, first;
    private float nextScan;
    private bool heldLeft, heldRight, hasFirst, attached;
    private GameObject? volume, vehicle;
    private LineRenderer? tether;
    private Vector3[] branches = Array.Empty<Vector3>();
    protected override void Setup()
    {
        previousLeft = CreativeTools.Left.position; previousRight = CreativeTools.Right.position;
        if (Mode == "Bubble Locomotion")
        {
            vehicle = Shape("Bubble", PrimitiveType.Sphere, CreativeTools.Head.position, Vector3.one * 1.8f);
            vehicle.GetComponent<Renderer>().enabled = false;
            tether = Line("Bubble Rim", 48, true);
        }
        if (Mode == "Rideable Rockets")
        {
            vehicle = Shape("Left Rocket", PrimitiveType.Capsule, CreativeTools.Left.position, new Vector3(0.12f, 0.3f, 0.12f));
            volume = Shape("Right Rocket", PrimitiveType.Capsule, CreativeTools.Right.position, new Vector3(0.12f, 0.3f, 0.12f));
        }
        if (Mode == "Air Swim")
        {
            var template = Object.FindObjectsByType<WaterVolume>(FindObjectsSortMode.None).FirstOrDefault(v => !v.transform.IsChildOf(transform));
            if (template == null) { Unavailable("load a map containing water before enabling this feature."); return; }
            volume = Object.Instantiate(template.gameObject);
            volume.name = "ChudOS Personal Swim Volume";
            volume.transform.SetParent(transform, true); Objects.Add(volume);
            foreach (Renderer renderer in volume.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
            foreach (AudioSource audio in volume.GetComponentsInChildren<AudioSource>(true)) audio.enabled = false;
            volume.transform.position = CreativeTools.Position;
            // Scale relative to its current collider bounds rather than assuming the prefab's units.
            var collider = volume.GetComponentInChildren<Collider>();
            if (collider == null) { Unavailable("the native water volume has no collider."); return; }
            Vector3 size = collider.bounds.size;
            float factor = 8f / Mathf.Max(0.1f, Mathf.Min(size.x, Mathf.Min(size.y, size.z)));
            volume.transform.localScale *= Mathf.Clamp(factor, 0.1f, 100f);
        }
    }
    protected override void Tick()
    {
        if (Mode == "Rideable Rockets")
        {
            if (vehicle != null) { vehicle.SetActive(LG); vehicle.transform.SetPositionAndRotation(CreativeTools.Left.position, CreativeTools.Left.rotation); }
            if (volume != null) { volume.SetActive(RG); volume.transform.SetPositionAndRotation(CreativeTools.Right.position, CreativeTools.Right.rotation); }
        }
        if (Mode == "Air Swim" && volume != null) volume.transform.position = CreativeTools.Position;
        if (Mode == "Bubble Locomotion" && tether != null) CreativeTools.Circle(tether, CreativeTools.Head.position - Vector3.up * 0.2f, Forward, 0.9f * CreativeTools.Scale);
        if (Mode is "Zipline Launcher" or "Nailgun Climbing" or "Wind Tunnel Builder" or "Moving Walkway" or "Bungee Anchor")
        {
            if (Place(out var point, out var normal))
            {
                if (Mode == "Nailgun Climbing")
                {
                    if (anchors.Count >= ExpansionSettings.GeometryLimit) { Objects.Remove(anchors[0].gameObject); Object.Destroy(anchors[0].gameObject); anchors.RemoveAt(0); }
                    var peg = Shape("Climbing Peg", PrimitiveType.Cube, point + normal * 0.10f, new Vector3(0.3f, 0.12f, 0.3f), true);
                    peg.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal); anchors.Add(peg.transform);
                }
                else if (Mode == "Zipline Launcher")
                {
                    if (!hasFirst) { first = point; hasFirst = true; }
                    else if (Vector3.Distance(first, point) > 1f)
                    {
                        anchor = point; hasFirst = false; attached = false;
                        if (tether == null) tether = Line("Zipline");
                        tether.SetPosition(0, first); tether.SetPosition(1, anchor);
                    }
                }
                else if (Mode == "Bungee Anchor")
                {
                    anchor = point; attached = true;
                    if (tether == null) tether = Line("Bungee");
                    tether.enabled = true;
                }
                else
                {
                    if (vehicle != null) { Objects.Remove(vehicle); Object.Destroy(vehicle); }
                    anchor = point;
                    vehicle = Shape(Mode, PrimitiveType.Cube, point + Vector3.up * (Mode == "Moving Walkway" ? 0.06f : 1f), Mode == "Moving Walkway" ? new Vector3(1.4f, 0.12f, 4f) : new Vector3(2f, 2f, 4f), Mode == "Moving Walkway");
                    vehicle.transform.rotation = Quaternion.LookRotation(Forward);
                    if (Mode == "Wind Tunnel Builder") vehicle.GetComponent<Renderer>().enabled = false;
                    if (tether == null) tether = Line("Direction");
                    tether.SetPosition(0, point + Vector3.up * 0.2f); tether.SetPosition(1, point + Forward * 3f + Vector3.up * 0.2f);
                }
            }
        }
        if (Mode == "Auto Branch" && Time.unscaledTime >= nextScan)
        {
            nextScan = Time.unscaledTime + 4f;
            branches = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t != null && t.gameObject.activeInHierarchy && t.name.IndexOf("branch", StringComparison.OrdinalIgnoreCase) >= 0 && !t.IsChildOf(transform)).Take(256).Select(t => t.position).ToArray();
        }
    }
    protected override void PhysicsTick()
    {
        Rigidbody body = CreativeTools.Body;
        Vector3 left = CreativeTools.Left.position, right = CreativeTools.Right.position;
        float dt = Mathf.Max(Time.fixedDeltaTime, 0.001f), power = ExpansionSettings.MovementPower;
        Vector3 lv = Vector3.ClampMagnitude((left - previousLeft) / dt - body.linearVelocity, 15f), rv = Vector3.ClampMagnitude((right - previousRight) / dt - body.linearVelocity, 15f);
        previousLeft = left; previousRight = right;
        bool ground = CreativeTools.Ground(out _);
        switch (Mode)
        {
            case "Airplane":
                if (!ground && Vector3.Distance(left, right) > 1.1f * CreativeTools.Scale && !LG && !RG)
                {
                    Vector3 direction = (CreativeTools.Left.forward + CreativeTools.Right.forward).normalized;
                    body.linearVelocity = Vector3.Lerp(body.linearVelocity, direction * power, 1f - Mathf.Exp(-2f * dt));
                }
                break;
            case "Bird Fly":
                if (!ground && Time.unscaledTime >= nextAction && lv.y < -1.2f && rv.y < -1.2f && Vector3.Distance(left, right) > 0.8f)
                { body.AddForce(Vector3.up * Mathf.Min(power, -(lv.y + rv.y)), ForceMode.VelocityChange); nextAction = Time.unscaledTime + 0.4f; }
                break;
            case "Bubble Locomotion":
                body.AddForce(-Physics.gravity, ForceMode.Acceleration);
                Vector3 center = CreativeTools.Head.position - Vector3.up * 0.2f;
                if (LG && Vector3.Distance(left, center) > 0.65f * CreativeTools.Scale) body.AddForce((left - center).normalized * power, ForceMode.Acceleration);
                if (RG && Vector3.Distance(right, center) > 0.65f * CreativeTools.Scale) body.AddForce((right - center).normalized * power, ForceMode.Acceleration);
                break;
            case "Rideable Rockets":
                if (LG) body.AddForce(CreativeTools.Left.up * power, ForceMode.Acceleration);
                if (RG) body.AddForce(CreativeTools.Right.up * power, ForceMode.Acceleration);
                break;
            case "Auto Walk":
                Vector3 desired = Forward * power * 0.5f; desired.y = body.linearVelocity.y;
                body.linearVelocity = Vector3.MoveTowards(body.linearVelocity, desired, power * dt);
                break;
            case "Auto Branch":
                if (!RG) break;
                float nearest = ExpansionSettings.MovementRange * ExpansionSettings.MovementRange; bool found = false;
                foreach (Vector3 branch in branches)
                {
                    float d = (branch - CreativeTools.Position).sqrMagnitude;
                    if (d < nearest && Vector3.Dot(branch - CreativeTools.Position, Forward) > 0f) { nearest = d; anchor = branch; found = true; }
                }
                if (found) body.linearVelocity = Vector3.ClampMagnitude((anchor + Vector3.up * 0.5f - CreativeTools.Position) * 3f, power);
                break;
            case "Controller Flick":
                // Keep the released hand's inertial velocity for a short, bounded locomotion stroke.
                if (CreativeTools.Pressed(X, ref heldLeft)) { anchor = lv; nextAction = Time.unscaledTime + 0.2f; }
                if (Time.unscaledTime < nextAction) body.AddForce(-anchor * power * 0.2f, ForceMode.Acceleration);
                break;
            case "Wall Kick":
                if (CreativeTools.Pressed(A, ref heldRight) && Physics.SphereCast(CreativeTools.Position, 0.18f, Forward, out var wall, 0.7f, NoClipState.CollisionLayers, QueryTriggerInteraction.Ignore) && Mathf.Abs(wall.normal.y) < 0.6f)
                    body.linearVelocity = wall.normal * power + Vector3.up * power * 0.65f;
                break;
            case "Ledge Hang":
                if (!RG) { attached = false; break; }
                if (!attached && Physics.Raycast(right + Vector3.up * 0.3f + Forward * 0.15f, Vector3.down, out var ledge, 0.6f, NoClipState.CollisionLayers, QueryTriggerInteraction.Ignore) && ledge.normal.y > 0.6f) { anchor = ledge.point; attached = true; }
                if (attached) body.linearVelocity = Vector3.ClampMagnitude((anchor - right) * 15f - Physics.gravity * dt, power);
                break;
            case "Parkour Vault":
                if (CreativeTools.Pressed(A, ref heldRight) && ground && Physics.Raycast(CreativeTools.Position, Forward, out var low, 0.8f, NoClipState.CollisionLayers, QueryTriggerInteraction.Ignore) && !Physics.Raycast(CreativeTools.Head.position, Forward, 1f, NoClipState.CollisionLayers, QueryTriggerInteraction.Ignore))
                    body.linearVelocity = Forward * Mathf.Min(power, 8f) + Vector3.up * 4f;
                break;
            case "Nailgun Climbing":
                foreach (Transform peg in anchors)
                {
                    Vector3 hand = LG ? left : right;
                    if ((LG || RG) && Vector3.Distance(hand, peg.position) < 0.3f) { body.linearVelocity = Vector3.ClampMagnitude((peg.position - hand) * 18f - Physics.gravity * dt, power); break; }
                }
                break;
            case "Zipline Launcher":
                if (tether == null) break;
                Vector3 segment = anchor - first;
                float progress = Vector3.Dot(right - first, segment) / Mathf.Max(0.01f, segment.sqrMagnitude);
                Vector3 near = first + segment * Mathf.Clamp01(progress);
                if (progress >= 0.98f) { attached = false; break; }
                if (RG && (attached || Vector3.Distance(right, near) < 0.4f)) { attached = true; body.linearVelocity = segment.normalized * power + Vector3.ClampMagnitude((near - right) * 12f, power); }
                else attached = false;
                break;
            case "Bungee Anchor":
                if (!attached || tether == null) break;
                tether.SetPosition(0, anchor); tether.SetPosition(1, CreativeTools.Position);
                Vector3 offset = anchor - CreativeTools.Position;
                float stretch = offset.magnitude - ExpansionSettings.MovementRange;
                if (stretch > 0f) body.AddForce(offset.normalized * Mathf.Min(stretch * power, 60f) - body.linearVelocity * 0.3f, ForceMode.Acceleration);
                if (B) { attached = false; tether.enabled = false; }
                break;
            case "Moving Walkway":
                if (vehicle != null && vehicle.GetComponent<Collider>().bounds.SqrDistance(CreativeTools.Position - Vector3.up * 0.4f) < 0.3f && ground) body.AddForce(vehicle.transform.forward * power, ForceMode.Acceleration);
                break;
            case "Wind Tunnel Builder":
                if (vehicle != null)
                {
                    Vector3 local = vehicle.transform.InverseTransformPoint(CreativeTools.Position);
                    if (ExpansionMath.InsideUnitBox(local)) body.AddForce(vehicle.transform.forward * power, ForceMode.Acceleration);
                }
                break;
        }
    }
    protected override void Cleanup() => GunLib.Hide();
}
