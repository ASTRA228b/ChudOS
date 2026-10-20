// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

namespace ChudOS.Mods.Fun;

internal static class HeadPoseMath
{
    internal static float Wrap(float angle)
    {
        if (float.IsNaN(angle) || float.IsInfinity(angle)) return 0f;
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

    internal static float Step(float current, float target, float maxDelta)
    {
        current = Wrap(current);
        if (float.IsNaN(maxDelta) || float.IsInfinity(maxDelta) || maxDelta <= 0f) return current;
        float delta = Wrap(Wrap(target) - current);
        return Wrap(current + Math.Max(-maxDelta, Math.Min(maxDelta, delta)));
    }
}
