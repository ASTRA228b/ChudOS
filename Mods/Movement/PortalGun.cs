// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Movement;

public static class PortalGun
{
    private sealed class Portal
    {
        internal Transform? Surface;
        internal Vector3 LocalPoint, LocalNormal;
        internal LineRenderer? Ring;
        internal bool Pose(out Vector3 point, out Vector3 normal)
        {
            point = normal = Vector3.zero;
            if (Surface == null) return false;
            point = Surface.TransformPoint(LocalPoint);
            normal = Surface.TransformDirection(LocalNormal).normalized;
            return CreativeTools.Finite(point) && CreativeTools.Finite(normal) && normal.sqrMagnitude > 0.9f;
        }
        internal void Clear()
        {
            if (Ring != null) Object.Destroy(Ring.gameObject);
            Ring = null;
            Surface = null;
        }
    }
    private static readonly Portal[] Portals = { new(), new() };
    private static readonly Collider[] ExitHits = new Collider[32];
    private static readonly CreativeTools.Session Session = new();
    private static bool selectedInput, switchInput, sampled;
    private static int selectedPortal;
    private static Vector3 previous;
    private static float placeAfter, travelAfter;
    private static Vector3 Center => GTPlayer.Instance.bodyCollider.transform.TransformPoint(GTPlayer.Instance.bodyCollider.center);

    [MenuButton("Portal Gun", ButtonType.Toggle, Category = "Movement", Order = 500, Controls = "(RG)(RT)(A)", runOption = RunOption.LateUpdate, Description = "Grip aims; trigger places the selected portal; A switches portals. Desktop: RMB, LMB and Q")]
    public static void Tick()
    {
        if (Session.Changed()) Clear();
        if (!CreativeTools.Ready) { Clear(); return; }
        PortTools.Register(Clear);
        if (!PortBindings.DesktopBlocked)
        {
            bool change = XRSettings.isDeviceActive ? InputLib.RightControllerAButton : Keyboard.current?.qKey.isPressed == true;
            if (CreativeTools.Pressed(change, ref switchInput)) selectedPortal ^= 1;
            bool shooting = GunLib.SelectPressed;
            bool pressed = CreativeTools.Pressed(shooting, ref selectedInput);
            bool hit = GunLib.UpdateGun(GunLib.AimHand, GunLib.AimPressed, shooting, out _);
            if (pressed && hit && Time.unscaledTime >= placeAfter) Place(GunLib.LastHit);
        }
        else { selectedInput = switchInput = false; GunLib.Hide(); }
        float radius = PortSettings.PortalRadius * CreativeTools.Scale;
        for (int i = 0; i < Portals.Length; i++)
        {
            Portal portal = Portals[i];
            bool valid = portal.Pose(out Vector3 point, out Vector3 normal);
            if (portal.Ring != null) portal.Ring.enabled = valid;
            if (!valid) continue;
            if (portal.Ring == null) portal.Ring = CreativeTools.Line("Portal", 40, 0.025f, true);
            portal.Ring.startColor = portal.Ring.endColor = i == 0 ? new Color(0.1f, 0.65f, 1f) : new Color(1f, 0.5f, 0.05f);
            CreativeTools.Circle(portal.Ring, point + normal * 0.015f, normal, radius);
        }
        Vector3 position = Center;
        if (sampled && Time.unscaledTime >= travelAfter)
            if (!TryTravel(0, position, radius)) TryTravel(1, position, radius);
        previous = Center;
        sampled = true;
    }
    [MenuEnable("Portal Gun")]
    public static void Enable()
    {
        Clear();
        PortTools.ClaimPointer("Portal Gun");
        PortBindings.Refresh();
        PortTools.Register(Clear);
    }
    private static void Place(RaycastHit hit)
    {
        Collider collider = hit.collider;
        if (collider == null || collider.isTrigger || collider.GetComponentInParent<VRRig>() != null || collider.GetComponentInParent<GTPlayer>() != null) return;
        if (ChudOS.Core.Main.MenuMain != null && collider.transform.IsChildOf(ChudOS.Core.Main.MenuMain.transform)) return;
        if (!CreativeTools.Finite(hit.point) || !CreativeTools.Finite(hit.normal)) return;
        float radius = PortSettings.PortalRadius * CreativeTools.Scale;
        if (!SurfaceFits(hit, radius)) return;
        Portal other = Portals[selectedPortal ^ 1];
        if (other.Pose(out Vector3 otherPoint, out _) && Vector3.Distance(otherPoint, hit.point) < radius * 2.2f) return;
        Portal portal = Portals[selectedPortal];
        portal.Surface = collider.transform;
        portal.LocalPoint = collider.transform.InverseTransformPoint(hit.point);
        portal.LocalNormal = collider.transform.InverseTransformDirection(hit.normal);
        placeAfter = Time.unscaledTime + PortSettings.PortalCooldown;
        previous = Center;
        sampled = true;
    }
    private static bool SurfaceFits(RaycastHit hit, float radius)
    {
        PortMovementMath.Basis(hit.normal, out Vector3 up, out Vector3 right);
        for (int i = 0; i < 4; i++)
        {
            Vector3 offset = i == 0 ? up : i == 1 ? -up : i == 2 ? right : -right;
            Vector3 origin = hit.point + offset * radius + hit.normal * 0.1f;
            if (!Physics.Raycast(origin, -hit.normal, out RaycastHit edge, 0.25f, NoClipState.CollisionLayers, QueryTriggerInteraction.Ignore)) return false;
            if (edge.collider != hit.collider || Vector3.Dot(edge.normal, hit.normal) < 0.95f) return false;
        }
        return true;
    }
    private static bool TryTravel(int index, Vector3 position, float radius)
    {
        Portal entry = Portals[index], exit = Portals[index ^ 1];
        if (!entry.Pose(out Vector3 start, out Vector3 startNormal) || !exit.Pose(out Vector3 end, out Vector3 endNormal)) return false;
        CapsuleCollider capsule = GTPlayer.Instance.bodyCollider;
        Vector3 scale = capsule.transform.lossyScale;
        if (!CreativeTools.Finite(scale)) return false;
        float bodyRadius = capsule.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        float height = Mathf.Max(bodyRadius * 2f, capsule.height * Mathf.Abs(scale.y));
        Vector3 axis = capsule.transform.up;
        float half = height * 0.5f - bodyRadius;
        float entrySupport = bodyRadius + Mathf.Abs(Vector3.Dot(axis, startNormal)) * half + 0.06f;
        if (!PortMovementMath.Crossed(previous, position, start, startNormal, entrySupport, radius)) return false;
        float exitSupport = bodyRadius + Mathf.Abs(Vector3.Dot(axis, endNormal)) * half + 0.08f;
        Vector3 destination = end + endNormal * exitSupport;
        if (!ExitClear(destination, axis, half, bodyRadius)) return false;
        Vector3 velocity = PortMovementMath.TransferVelocity(CreativeTools.Body.linearVelocity, startNormal, endNormal);
        if (!CreativeTools.Finite(velocity)) return false;
        Vector3 centerOffset = Center - CreativeTools.Position;
        PortTools.Teleport(destination - centerOffset);
        CreativeTools.Body.linearVelocity = velocity;
        travelAfter = Time.unscaledTime + 0.5f;
        return true;
    }
    private static bool ExitClear(Vector3 center, Vector3 axis, float half, float radius)
    {
        if (!CreativeTools.Finite(center) || radius <= 0f) return false;
        int count = Physics.OverlapCapsuleNonAlloc(center + axis * half, center - axis * half, radius * 0.95f, ExitHits, NoClipState.CollisionLayers, QueryTriggerInteraction.Ignore);
        if (count == ExitHits.Length) return false;
        for (int i = 0; i < count; i++)
        {
            Collider collider = ExitHits[i];
            if (collider == null || collider.GetComponentInParent<GTPlayer>() == GTPlayer.Instance) continue;
            VRRig rig = collider.GetComponentInParent<VRRig>();
            if (rig != null && rig == VRRig.LocalRig) continue;
            return false;
        }
        return true;
    }
    [MenuDisable("Portal Gun")]
    public static void Clear()
    {
        foreach (Portal portal in Portals) portal.Clear();
        selectedInput = switchInput = sampled = false;
        selectedPortal = 0;
        placeAfter = travelAfter = 0f;
        GunLib.Hide();
    }
}
