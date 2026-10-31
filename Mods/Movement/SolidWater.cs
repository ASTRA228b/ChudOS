// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaLocomotion;
using GorillaLocomotion.Swimming;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Movement;

public static class SolidWater
{
    private static readonly CreativeTools.Session Session = new();
    private static WaterVolume[] volumes = Array.Empty<WaterVolume>();
    private static GameObject? support;
    private static float nextScan;
    [MenuButton("Solid Water", ButtonType.Toggle, Category = "Movement", Order = 506, runOption = RunOption.FixedUpdate, Description = "Support the player at a detected water surface")]
    public static void Tick()
    {
        if (Session.Changed()) Stop();
        if (!CreativeTools.Ready) { Stop(); return; }
        PortTools.Register(Stop);
        if (Time.unscaledTime >= nextScan)
        {
            nextScan = Time.unscaledTime + 1f;
            volumes = Object.FindObjectsByType<WaterVolume>(FindObjectsSortMode.None);
        }
        Vector3 position = CreativeTools.Position;
        float feet = GTPlayer.Instance.bodyCollider.bounds.min.y, scale = CreativeTools.Scale;
        foreach (WaterVolume water in volumes)
        {
            if (water == null || !water.isActiveAndEnabled) continue;
            if (!water.GetSurfaceQueryForPoint(position, out WaterVolume.SurfaceQuery surface, false)) continue;
            if (!CreativeTools.Finite(surface.surfacePoint) || !CreativeTools.Finite(surface.surfaceNormal)) continue;
            if (Vector3.Dot(surface.surfaceNormal, Vector3.up) < 0.95f) continue;
            if (feet < surface.surfacePoint.y - 0.1f * scale || feet > surface.surfacePoint.y + 0.45f * scale) continue;
            if (!Inside(water, surface.surfacePoint - surface.surfaceNormal * 0.08f * scale)) continue;
            if (support == null)
            {
                support = new GameObject("ChudOS Solid Water");
                support.layer = SolidPlayers.CollisionLayer();
                support.AddComponent<BoxCollider>();
            }
            support.transform.SetPositionAndRotation(surface.surfacePoint - surface.surfaceNormal * 0.035f * scale, Quaternion.FromToRotation(Vector3.up, surface.surfaceNormal));
            support.transform.localScale = new Vector3(1.1f, 0.06f, 1.1f) * scale;
            support.SetActive(true);
            return;
        }
        if (support != null) support.SetActive(false);
    }
    private static bool Inside(WaterVolume water, Vector3 point)
    {
        if (water.volumeColliders == null) return false;
        foreach (Collider collider in water.volumeColliders)
        {
            if (collider == null || !collider.enabled || !collider.bounds.Contains(point)) continue;
            if (collider is MeshCollider mesh && !mesh.convex) continue;
            if ((collider.ClosestPoint(point) - point).sqrMagnitude < 0.0001f) return true;
        }
        return false;
    }
    [MenuDisable("Solid Water")]
    public static void Stop()
    {
        if (support != null) Object.Destroy(support);
        support = null; volumes = Array.Empty<WaterVolume>(); nextScan = 0f;
    }
}
