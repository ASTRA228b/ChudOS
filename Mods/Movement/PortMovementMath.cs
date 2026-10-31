// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using UnityEngine;

namespace ChudOS.Mods.Movement;

internal static class PortMovementMath
{
    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    private static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);
    internal static Vector3 ChargeImpulse(Vector3 offset, float power, float radius, float scale)
    {
        float distance = offset.magnitude;
        radius *= scale;
        if (!Finite(offset) || !Finite(power) || !Finite(radius) || !Finite(scale) || radius <= 0f || scale <= 0f || power < 0f || distance <= 0.01f || distance >= radius) return Vector3.zero;
        Vector3 impulse = offset / distance * power * scale * (1f - Mathf.Clamp01(distance / radius));
        return Finite(impulse) ? impulse : Vector3.zero;
    }
    internal static bool Crossed(Vector3 previous, Vector3 position, Vector3 point, Vector3 normal, float support, float radius)
    {
        if (!Finite(previous) || !Finite(position) || !Finite(point) || !Finite(normal) || !Finite(support) || !Finite(radius) || radius <= 0f || normal.sqrMagnitude < 0.9f || normal.sqrMagnitude > 1.1f) return false;
        float before = Vector3.Dot(previous - point, normal), now = Vector3.Dot(position - point, normal);
        if (!Finite(before) || !Finite(now) || before <= support || now > support) return false;
        float blend = (before - support) / Mathf.Max(before - now, 0.0001f);
        Vector3 crossing = Vector3.Lerp(previous, position, blend);
        return Vector3.ProjectOnPlane(crossing - point, normal).sqrMagnitude <= radius * radius;
    }
    internal static Vector3 TransferVelocity(Vector3 velocity, Vector3 startNormal, Vector3 endNormal)
    {
        if (!Finite(velocity) || !Finite(startNormal) || !Finite(endNormal) || startNormal.sqrMagnitude < 0.001f || endNormal.sqrMagnitude < 0.001f) return Vector3.zero;
        startNormal.Normalize(); endNormal.Normalize();
        Basis(startNormal, out Vector3 startUp, out _);
        Basis(endNormal, out Vector3 endUp, out _);
        return Quaternion.LookRotation(endNormal, endUp) * Quaternion.Inverse(Quaternion.LookRotation(-startNormal, startUp)) * velocity;
    }
    internal static void Basis(Vector3 normal, out Vector3 up, out Vector3 right)
    {
        up = Vector3.ProjectOnPlane(Vector3.up, normal);
        if (up.sqrMagnitude < 0.01f) up = Vector3.ProjectOnPlane(Vector3.forward, normal);
        up.Normalize();
        right = Vector3.Cross(up, normal).normalized;
    }
}
