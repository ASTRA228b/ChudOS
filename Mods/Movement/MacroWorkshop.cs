// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.IO;
using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Movement;

internal sealed class MacroWorkshop : BetaFeature
{
    private MacroManager.RecordingData? data, comparison;
    private LineRenderer? route, other;
    private Transform? marker;
    private TextMesh? label;
    private int start, end, scrub, compareIndex, nearest;
    private float nextCheck, nextNotice;
    private bool align = true, dirty = true;
    private string summary = "";
    private Rect window = new(170, 100, 440, 430);
    protected override void Setup() { route = Line("Macro Workshop Route", 2); marker = CreativeTools.Marker("Macro Scrubber", 0.12f); Objects.Add(marker.gameObject); label = Label("Select a macro", CreativeTools.Head.position + Forward); }
    protected override void Tick()
    {
        if (data != MacroManager.Selected)
        {
            data = MacroManager.Selected; start = scrub = nearest = 0; end = data == null ? 0 : data.Frames.Count - 1; dirty = true;
            if (label != null) label.text = data == null ? "Select a macro" : Mode + "\n" + data.Name;
        }
        if (data == null || data.Frames.Count < 2) return;
        if (dirty) { DrawRoutes(); dirty = false; }
        if (Mode == "Macro Editor" && marker != null) marker.position = Position(data.Frames[Mathf.Clamp(scrub, 0, end)]);
        if (Mode == "Route Deviation Coach" && Time.unscaledTime >= nextCheck)
        {
            nextCheck = Time.unscaledTime + 0.12f;
            Vector3 body = data.HasRigPose && VRRig.LocalRig != null ? VRRig.LocalRig.transform.position : CreativeTools.Position;
            float best = float.PositiveInfinity;
            for (int i = 0; i < data.Frames.Count; i++)
            {
                float distance = (body - Position(data.Frames[i])).sqrMagnitude;
                if (distance < best) { best = distance; nearest = i; }
            }
            float error = Mathf.Sqrt(best);
            if (label != null) { label.transform.position = CreativeTools.Head.position + Forward * 0.8f; label.text = "Route offset: " + error.ToString("F2") + "m\nFrame " + nearest; label.transform.rotation = Quaternion.LookRotation(label.transform.position - CreativeTools.Head.position); }
            if (error > 1.5f && Time.unscaledTime >= nextNotice) { nextNotice = Time.unscaledTime + 5f; CreativeTools.Notice("Route deviation: " + error.ToString("F1") + "m"); }
        }
    }
    private static Vector3 Position(MacroManager.Frame frame) => CreativeTools.Vector(frame.Body, Vector3.zero);
    private void DrawRoutes()
    {
        if (data == null || route == null) return;
        route.positionCount = data.Frames.Count;
        for (int i = 0; i < data.Frames.Count; i++) route.SetPosition(i, Position(data.Frames[i]));
        if (Mode != "Macro Compare" || MacroManager.Recordings.Count == 0) return;
        compareIndex = Mathf.Clamp(compareIndex, 0, MacroManager.Recordings.Count - 1); comparison = MacroManager.Recordings[compareIndex];
        if (other == null) other = Line("Comparison Route");
        other.startColor = other.endColor = Color.white; other.positionCount = comparison.Frames.Count;
        Vector3 offset = align ? Position(data.Frames[0]) - Position(comparison.Frames[0]) : Vector3.zero;
        for (int i = 0; i < comparison.Frames.Count; i++) other.SetPosition(i, Position(comparison.Frames[i]) + offset);
        double sum = 0; int samples = Math.Min(128, Math.Min(data.Frames.Count, comparison.Frames.Count));
        for (int i = 0; i < samples; i++)
        {
            int a = ExpansionMath.RouteIndex(i, samples, data.Frames.Count), b = ExpansionMath.RouteIndex(i, samples, comparison.Frames.Count);
            sum += (Position(data.Frames[a]) - Position(comparison.Frames[b]) - offset).sqrMagnitude;
        }
        summary = "Normalized-route RMS distance: " + Math.Sqrt(sum / samples).ToString("F2") + "m\nDuration difference: " + Mathf.Abs(data.Frames[^1].Time - comparison.Frames[^1].Time).ToString("F2") + "s";
    }
    private void OnGUI()
    {
        if (Mode is "Macro Editor" or "Macro Compare") window = MenuHelper.MakeWindow(855100 + (Mode == "Macro Editor" ? 1 : 2), window, Contents, Mode);
    }
    private void Contents(int id)
    {
        MenuHelper.Label("Selected: " + (data?.Name ?? "No macro selected"));
        if (MenuHelper.Button("Next Selected Macro")) MacroManager.Select(1);
        if (data != null && data.Frames.Count > 1)
        {
            if (Mode == "Macro Editor")
            {
                start = Mathf.RoundToInt(MenuHelper.Slider(start, 0, data.Frames.Count - 2));
                end = Mathf.RoundToInt(MenuHelper.Slider(Mathf.Max(end, start + 1), start + 1, data.Frames.Count - 1));
                scrub = Mathf.RoundToInt(MenuHelper.Slider(scrub, start, end));
                MenuHelper.Label("Trim: " + data.Frames[start].Time.ToString("F2") + "–" + data.Frames[end].Time.ToString("F2") + "s | Scrub: " + scrub);
                if (MenuHelper.Button("Save Trimmed Copy")) SaveTrim();
                MenuHelper.Label("Saves a new native JSON copy; the original recording is preserved. Scrubbing moves only the marker.");
            }
            else
            {
                MenuHelper.Label("Compare: " + (comparison?.Name ?? "None"));
                if (MenuHelper.Button("Next Comparison")) { compareIndex = (compareIndex + 1) % Math.Max(1, MacroManager.Recordings.Count); dirty = true; }
                bool changed = MenuHelper.Toggle("Align starting points", align); if (changed != align) { align = changed; dirty = true; }
                MenuHelper.Label(summary);
            }
        }
        GUI.DragWindow();
    }
    private void SaveTrim()
    {
        if (data == null || MacroManager.IsPlaying || MacroManager.Recording) { CreativeTools.Notice("Stop macro recording/playback before saving a trim."); return; }
        try
        {
            var clone = JsonConvert.DeserializeObject<MacroManager.RecordingData>(JsonConvert.SerializeObject(data))!;
            clone.Frames = clone.Frames.Skip(start).Take(end - start + 1).ToList();
            float origin = clone.Frames[0].Time; foreach (var frame in clone.Frames) frame.Time -= origin;
            clone.Name = data.Name + " (Trimmed)";
            if (!MacroManager.Valid(clone)) { CreativeTools.Notice("That trim does not produce a valid recording."); return; }
            Directory.CreateDirectory(MacroManager.Folder);
            string path = Path.Combine(MacroManager.Folder, Guid.NewGuid().ToString("N") + ".json");
            File.WriteAllText(path, JsonConvert.SerializeObject(clone, Formatting.Indented)); MacroManager.Reload();
            CreativeTools.Notice("Trimmed macro saved as a new copy.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { CreativeTools.Notice("Could not save the trimmed macro."); }
    }
}
