// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using UnityEngine;

namespace ChudOS.Mods.Movement;

internal static class StickyPlatformColliders
{
    internal static void Add(GameObject platform, PhysicsMaterial material)
    {
        if (platform.transform.Find("Sticky Colliders") != null) return;
        var holder = new GameObject("Sticky Colliders");
        holder.transform.SetParent(platform.transform, false);
        Vector3 scale = platform.transform.lossyScale;
        Vector3 thickness = new(
            Mathf.Clamp(0.005f / Mathf.Max(Mathf.Abs(scale.x), 0.001f), 0.01f, 0.1f),
            Mathf.Clamp(0.005f / Mathf.Max(Mathf.Abs(scale.y), 0.001f), 0.01f, 0.1f),
            Mathf.Clamp(0.005f / Mathf.Max(Mathf.Abs(scale.z), 0.001f), 0.01f, 0.1f));
        for (int axis = 0; axis < 3; axis++)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                var wall = new GameObject($"Sticky Face {axis} {side}");
                wall.layer = platform.layer;
                wall.transform.SetParent(holder.transform, false);
                Vector3 size = Vector3.one, position = Vector3.zero;
                size[axis] = thickness[axis];
                position[axis] = side * (0.5f - thickness[axis] * 0.5f);
                wall.transform.localPosition = position;
                var collider = wall.AddComponent<BoxCollider>();
                collider.size = size;
                collider.sharedMaterial = material;
                var surface = wall.AddComponent<GorillaSurfaceOverride>();
                surface.overrideIndex = 0;
                surface.slidePercentageOverride = 0f;
            }
        }
    }
}
