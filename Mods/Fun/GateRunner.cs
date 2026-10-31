// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Fun;

public static class GateRunner
{
    private sealed class Gate { internal LineRenderer Line = null!; internal Vector3 Position, Normal; internal float Radius; }
    private static readonly CreativeTools.Session Session = new();
    private static readonly List<Gate> Gates = new();
    private static bool placeInput, startInput, clearInput, running;
    private static Vector3 previous;
    private static float started, best = float.PositiveInfinity;
    private static int current;
    [MenuButton("Gate Runner", ButtonType.Toggle, Category = "Fun", Order = 401, Controls = "(X)(A)(B)", runOption = RunOption.LateUpdate, Description = "X places a gate facing your right hand; A starts or cancels a run; B clears the course")]
    public static void Tick()
    {
        if (Session.Changed()) Clear();
        if (!CreativeTools.Ready) { Clear(); return; }
        PortTools.Register(Clear);
        bool place = CreativeTools.Pressed(InputLib.LeftControllerXButton, ref placeInput), start = CreativeTools.Pressed(InputLib.RightControllerAButton, ref startInput), clear = CreativeTools.Pressed(InputLib.RightControllerBButton, ref clearInput);
        if (clear) { ClearCourse(); return; }
        if (place && !running && Gates.Count < 32)
        {
            Gate gate = new() { Position = CreativeTools.Right.position, Normal = CreativeTools.Right.forward.normalized, Radius = CreativeSettings.GateRadius * CreativeTools.Scale, Line = CreativeTools.Line("Practice Gate", 32, 0.025f, true) };
            CreativeTools.Circle(gate.Line, gate.Position, gate.Normal, gate.Radius);
            gate.Line.startColor = gate.Line.endColor = CreativeTools.Theme;
            Gates.Add(gate); best = float.PositiveInfinity;
        }
        if (start)
        {
            if (running) { running = false; Highlight(); return; }
            if (Gates.Count < 2) { CreativeTools.Notice("Place at least two gates first."); return; }
            current = 0; started = Time.unscaledTime; previous = CreativeTools.Position; running = true; Highlight();
        }
        if (!running) return;
        Vector3 position = CreativeTools.Position;
        Gate target = Gates[current];
        float from = Vector3.Dot(previous - target.Position, target.Normal), to = Vector3.Dot(position - target.Position, target.Normal);
        if (from * to <= 0f && Mathf.Abs(from - to) > 0.0001f)
        {
            Vector3 crossing = Vector3.Lerp(previous, position, from / (from - to));
            if ((crossing - target.Position).sqrMagnitude <= target.Radius * target.Radius)
            {
                current++;
                if (current == Gates.Count)
                {
                    float elapsed = Time.unscaledTime - started;
                    best = Mathf.Min(best, elapsed); running = false;
                    CreativeTools.Notice($"Gate Runner: {elapsed:0.00}s | Best: {best:0.00}s");
                }
                Highlight();
            }
        }
        previous = position;
    }
    private static void Highlight()
    {
        for (int i = 0; i < Gates.Count; i++) { Color color = running && i == current ? Color.white : CreativeTools.Theme; Gates[i].Line.startColor = Gates[i].Line.endColor = color; }
    }
    private static void ClearCourse()
    {
        foreach (Gate gate in Gates) if (gate.Line != null) Object.Destroy(gate.Line.gameObject);
        Gates.Clear(); running = false; current = 0; best = float.PositiveInfinity;
    }
    [MenuDisable("Gate Runner")]
    public static void Clear() { ClearCourse(); placeInput = startInput = clearInput = false; }
}
