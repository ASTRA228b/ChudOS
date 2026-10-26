// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Fun;

public static class FpsMods
{
    private static bool captured;
    private static int originalLimit, originalVsync;

    private static void Set(int fps)
    {
        if (!captured)
        {
            originalLimit = Application.targetFrameRate;
            originalVsync = QualitySettings.vSyncCount;
            captured = true;
            PortTools.Register(Reset);
        }
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
    }

    [MenuButton("Unlock FPS", ButtonType.Press, Category = "Fun", Order = 100)]
    public static void Unlock() => Set(-1);
    [MenuButton("30 FPS", ButtonType.Press, Category = "Fun", Order = 101)]
    public static void Fps30() => Set(30);
    [MenuButton("60 FPS", ButtonType.Press, Category = "Fun", Order = 102)]
    public static void Fps60() => Set(60);
    [MenuButton("72 FPS", ButtonType.Press, Category = "Fun", Order = 103)]
    public static void Fps72() => Set(72);
    [MenuButton("90 FPS", ButtonType.Press, Category = "Fun", Order = 104)]
    public static void Fps90() => Set(90);
    [MenuButton("120 FPS", ButtonType.Press, Category = "Fun", Order = 105)]
    public static void Fps120() => Set(120);
    [MenuButton("144 FPS", ButtonType.Press, Category = "Fun", Order = 106)]
    public static void Fps144() => Set(144);
    [MenuButton("165 FPS", ButtonType.Press, Category = "Fun", Order = 107)]
    public static void Fps165() => Set(165);
    [MenuButton("240 FPS", ButtonType.Press, Category = "Fun", Order = 108)]
    public static void Fps240() => Set(240);
    [MenuButton("360 FPS", ButtonType.Press, Category = "Fun", Order = 109)]
    public static void Fps360() => Set(360);

    [MenuButton("Reset FPS", ButtonType.Press, Category = "Fun", Order = 110)]
    public static void Reset()
    {
        if (!captured) return;
        Application.targetFrameRate = originalLimit;
        QualitySettings.vSyncCount = originalVsync;
        captured = false;
    }
}
