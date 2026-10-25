// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Mods.Movement;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Settings;

public static class PredictionSettings
{
    private static readonly float[] Strengths = { 0.001f, 0.01f, 0.025f, 0.05f, 0.1f, 0.15f, 0.2f };
    private static readonly float[] Thresholds = { 0.01f, 0.03f, 0.05f, 0.1f, 0.2f, 0.3f };
    private static readonly float[] Smoothing = { 0.05f, 0.01f, 0.1f, 0.2f, 0.3f, 0.5f };
    private static readonly float[] Reaches = { 1f, 1.25f, 1.5f, 1.75f, 2f, 2.5f };
    private static readonly float[,] Presets = { { 0.001f, 0.01f, 0.05f, 1f }, { 0.01f, 0.01f, 0.05f, 1f }, { 0.05f, 0.01f, 0.1f, 1.5f }, { 0.1f, 0.01f, 0.2f, 2f } };
    private static bool applying;
    public static float Strength { get; private set; } = 0.001f;
    public static float Threshold { get; private set; } = 0.01f;
    public static float Smoothness { get; private set; } = 0.05f;
    public static float Reach { get; private set; } = 1f;
    private static int Index(string name, int count) => Math.Clamp(ButtonManager.GetButton(name)?.OptionIndex ?? 0, 0, count - 1);

    [MenuButton("Prediction Preset", ButtonType.Option, Category = "Settings", Order = 190, Options = new[] { "Default", "Light", "Medium", "Strong", "Custom" }, Notify = false)]
    public static void SetPreset()
    {
        int index = Index("Prediction Preset", 5);
        if (index == 4) return;
        applying = true;
        try
        {
            Select("Prediction Strength", Strengths, Presets[index, 0]);
            Select("Prediction Threshold", Thresholds, Presets[index, 1]);
            Select("Prediction Smoothness", Smoothing, Presets[index, 2]);
            Select("Prediction Reach", Reaches, Presets[index, 3]);
        }
        finally { applying = false; }
        Changed();
    }
    private static void Select(string name, float[] values, float value)
    {
        var button = ButtonManager.GetButton(name);
        if (button == null) return;
        button.OptionIndex = Math.Max(0, Array.IndexOf(values, value));
        button.Run();
    }

    [MenuButton("Prediction Strength", ButtonType.Option, Category = "Settings", Order = 191, Options = new[] { "0.001", "0.01", "0.025", "0.05", "0.1", "0.15", "0.2" }, Notify = false)]
    public static void SetStrength() { Strength = Strengths[Index("Prediction Strength", Strengths.Length)]; Changed(); }
    [MenuButton("Prediction Threshold", ButtonType.Option, Category = "Settings", Order = 192, Options = new[] { "0.01", "0.03", "0.05", "0.1", "0.2", "0.3" }, Notify = false)]
    public static void SetThreshold() { Threshold = Thresholds[Index("Prediction Threshold", Thresholds.Length)]; Changed(); }
    [MenuButton("Prediction Smoothness", ButtonType.Option, Category = "Settings", Order = 193, Options = new[] { "0.05", "0.01", "0.1", "0.2", "0.3", "0.5" }, Notify = false)]
    public static void SetSmoothness() { Smoothness = Smoothing[Index("Prediction Smoothness", Smoothing.Length)]; Changed(); }
    [MenuButton("Prediction Reach", ButtonType.Option, Category = "Settings", Order = 194, Options = new[] { "1", "1.25", "1.5", "1.75", "2", "2.5" }, Notify = false)]
    public static void SetReach() { Reach = Reaches[Index("Prediction Reach", Reaches.Length)]; Changed(); }
    [MenuButton("Reset Predictions", ButtonType.Press, Category = "Settings", Order = 195)]
    public static void Reset()
    {
        var preset = ButtonManager.GetButton("Prediction Preset");
        if (preset != null) preset.OptionIndex = 0;
        SetPreset();
    }
    private static void Changed()
    {
        if (applying) return;
        int match = 4;
        for (int i = 0; i < 4; i++)
            if (Mathf.Approximately(Strength, Presets[i, 0]) && Mathf.Approximately(Threshold, Presets[i, 1]) && Mathf.Approximately(Smoothness, Presets[i, 2]) && Mathf.Approximately(Reach, Presets[i, 3])) { match = i; break; }
        var preset = ButtonManager.GetButton("Prediction Preset");
        if (preset != null) preset.OptionIndex = match;
        Predictions.ResetSamples();
    }
}
