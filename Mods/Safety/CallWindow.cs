// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

namespace ChudOS.Mods.Safety;

internal sealed class CallWindow
{
    private readonly float[] calls;
    private readonly float duration;
    private int head, count;
    private float lastTime;

    internal CallWindow(int limit, float seconds)
    {
        if (limit < 1 || seconds <= 0f || float.IsNaN(seconds) || float.IsInfinity(seconds))
            throw new ArgumentOutOfRangeException();
        calls = new float[limit];
        duration = seconds;
    }

    internal bool TryTake(float now)
    {
        if (float.IsNaN(now) || float.IsInfinity(now)) return false;
        if (now < lastTime) Reset();
        lastTime = now;
        while (count > 0 && now - calls[head] >= duration)
        {
            head = (head + 1) % calls.Length;
            count--;
        }
        if (count == calls.Length) return false;
        calls[(head + count) % calls.Length] = now;
        count++;
        return true;
    }

    internal void Reset() { head = count = 0; lastTime = 0f; }
}
