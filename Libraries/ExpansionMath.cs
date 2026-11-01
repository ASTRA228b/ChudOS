// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using UnityEngine;

namespace ChudOS.Libraries;

internal static class ExpansionMath
{
    internal static float DistanceToSegment(Vector3 point, Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        float length = delta.sqrMagnitude;
        if (length < 0.000001f) return Vector3.Distance(point, from);
        return Vector3.Distance(point, from + delta * Mathf.Clamp01(Vector3.Dot(point - from, delta) / length));
    }
    internal static bool InsideUnitBox(Vector3 local) => Mathf.Abs(local.x) <= 0.5f && Mathf.Abs(local.y) <= 0.5f && Mathf.Abs(local.z) <= 0.5f;
    internal static float Cross(Vector2 a, Vector2 b, Vector2 c) => (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    internal static int RouteIndex(int sample, int samples, int frames) => samples <= 1 || frames <= 1 ? 0 : Mathf.Clamp(sample, 0, samples - 1) * (frames - 1) / (samples - 1);
}
