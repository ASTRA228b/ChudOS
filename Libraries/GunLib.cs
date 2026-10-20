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

using UnityEngine;
using ChudOS.Stuff;
using GorillaLocomotion;

namespace ChudOS.Libraries;

public static class GunLib
{
    public static GameObject? Pointer;
    private static LineRenderer? Line;
    public static RaycastHit LastHit;
    public static bool HasHit;
    public static bool Enabled;
    private static RaycastHit[] Hits = new RaycastHit[64];

    public static bool UpdateGun(Transform hand, bool active, bool select, out Vector3 point, float distance = 80f)
        => UpdateGunInternal(hand, active, select, out point, distance, false);

    private static bool UpdateGunInternal(Transform hand, bool active, bool select, out Vector3 point, float distance, bool playerTargets)
    {
        point = Vector3.zero;
        if (!active || hand == null || !Finite(hand.position) || !Finite(hand.forward) || float.IsNaN(distance) || float.IsInfinity(distance) || distance <= 0f) { Hide(); return false; }
        distance = Mathf.Min(distance, 500f);
        if (Line == null) Create();
        Vector3 origin = hand.position;
        Vector3 direction = hand.forward;
        HasHit = RaycastNearest(origin, direction, distance, playerTargets, out LastHit);
        point = HasHit ? LastHit.point : origin + direction * distance;
        if (Pointer != null) { Pointer.SetActive(true); Pointer.transform.position = point; }
        if (Line != null) { Line.enabled = true; Line.SetPosition(0, origin); Line.SetPosition(1, point); }
        Enabled = true;
        return select && HasHit;
    }

    public static bool UpdateRigGun(Transform hand, bool active, bool select, ref VRRig? target, float distance = 80f)
    {
        if (NetworkSystem.Instance == null || !NetworkSystem.Instance.InRoom)
        {
            target = null;
            Hide();
            return false;
        }

        bool selected = UpdateGunInternal(hand, active, select, out _, distance, true);
        if (!active || !select || hand == null || !Finite(hand.position) || !Finite(hand.forward) || float.IsNaN(distance) || float.IsInfinity(distance) || distance <= 0f)
        {
            target = null;
            return false;
        }

        if (!IsRemoteRig(target) || (target!.transform.position - hand.position).sqrMagnitude > distance * distance)
        {
            target = null;
            if (selected && LastHit.collider != null)
            {
                VRRig candidate = LastHit.collider.GetComponentInParent<VRRig>();
                if (IsRemoteRig(candidate)) target = candidate;
            }
        }

        if (target == null) return false;
        Vector3 point = target.head != null && target.head.rigTarget != null ? target.head.rigTarget.position : target.transform.position;
        if (Pointer != null) Pointer.transform.position = point;
        if (Line != null) Line.SetPosition(1, point);
        return true;
    }

    private static bool IsRemoteRig(VRRig? rig) => rig != null && rig != VRRig.LocalRig && rig.gameObject.activeInHierarchy &&
        !rig.isLocal && !rig.isOfflineVRRig && Finite(rig.transform.position) &&
        rig.head.rigTarget != null && rig.leftHand.rigTarget != null && rig.rightHand.rigTarget != null &&
        VRRigCache.ActiveRigs != null && VRRigCache.ActiveRigs.Contains(rig);

    private static bool Finite(Vector3 value) => !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
        !float.IsNaN(value.y) && !float.IsInfinity(value.y) && !float.IsNaN(value.z) && !float.IsInfinity(value.z);

    private static bool RaycastNearest(Vector3 origin, Vector3 direction, float distance, bool playerTargets, out RaycastHit hit)
    {
        hit = default;
        int count;
        while (true)
        {
            count = Physics.RaycastNonAlloc(origin, direction, Hits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
            if (count < Hits.Length) break;
            if (Hits.Length >= 512) return false;
            Hits = new RaycastHit[Hits.Length * 2];
        }
        float closest = distance;
        bool found = false;
        for (int i = 0; i < count; i++)
        {
            Collider collider = Hits[i].collider;
            if (collider == null || collider.gameObject == Pointer) continue;
            VRRig rig = collider.GetComponentInParent<VRRig>();
            if (rig != null && !IsRemoteRig(rig)) continue;
            if (GTPlayer.Instance != null && collider.transform.IsChildOf(GTPlayer.Instance.transform)) continue;
            if (collider.isTrigger && (!playerTargets || rig == null)) continue;
            if (Hits[i].distance > closest) continue;
            closest = Hits[i].distance;
            hit = Hits[i];
            found = true;
        }
        return found;
    }

    private static void Create()
    {
        GameObject root = new GameObject(Constantss.GUID + ".GunLine");
        Line = root.AddComponent<LineRenderer>();
        Line.positionCount = 2;
        Line.startWidth = 0.008f;
        Line.endWidth = 0.004f;
        Line.useWorldSpace = true;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) Line.material = new Material(shader);
        Line.startColor = Color.magenta;
        Line.endColor = Color.white;
        Pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Pointer.name = Constantss.GUID + ".GunPointer";
        Pointer.layer = 2;
        Pointer.transform.localScale = Vector3.one * 0.035f;
        Collider collider = Pointer.GetComponent<Collider>();
        if (collider != null) { collider.enabled = false; UnityEngine.Object.Destroy(collider); }
    }
    public static void Hide() { Enabled = false; HasHit = false; LastHit = default; if (Line != null) Line.enabled = false; if (Pointer != null) Pointer.SetActive(false); }
    public static void Cleanup()
    {
        if (Line != null) { if (Line.material != null) UnityEngine.Object.Destroy(Line.material); UnityEngine.Object.Destroy(Line.gameObject); }
        if (Pointer != null) UnityEngine.Object.Destroy(Pointer);
        Line = null; Pointer = null; HasHit = false; Enabled = false;
    }
}
