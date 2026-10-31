// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Stuff;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Fun;

public static class EchoRace
{
    private static readonly CreativeTools.Session Session = new();
    private static readonly Transform?[] Ghost = new Transform?[4];
    private static MacroManager.RecordingData? recording;
    private static Vector3 offset, finish;
    private static float started;
    private static int frame;
    private static bool startInput, running, leftFinish;
    [MenuButton("Echo Race", ButtonType.Toggle, Category = "Fun", Order = 400, Controls = "(A)", runOption = RunOption.LateUpdate, Description = "Select a saved macro, then press A to race a local ghost of its route; press A again to cancel")]
    public static void Tick()
    {
        if (Session.Changed()) Clear();
        if (!CreativeTools.Ready) { Clear(); return; }
        PortTools.Register(Clear);
        if (CreativeTools.Pressed(InputLib.RightControllerAButton, ref startInput)) { if (running) { StopRace(); return; } Begin(); }
        if (!running || recording == null) return;
        if (MacroManager.IsPlaying || MacroManager.Recording) { StopRace(); return; }
        var frames = recording.Frames;
        float elapsed = Time.unscaledTime - started + frames[0].Time;
        while (frame + 1 < frames.Count && frames[frame + 1].Time <= elapsed) frame++;
        int next = Mathf.Min(frame + 1, frames.Count - 1);
        var a = frames[frame]; var b = frames[next];
        float blend = a == b ? 0f : Mathf.InverseLerp(a.Time, b.Time, elapsed);
        Vector3 body = Vector3.Lerp(CreativeTools.Vector(a.Body, Vector3.zero), CreativeTools.Vector(b.Body, Vector3.zero), blend) + offset;
        Ghost[0]!.position = body;
        Ghost[1]!.position = Point(a.HeadPosition, b.HeadPosition, body - offset + Vector3.up * 0.35f, blend);
        Ghost[2]!.position = Point(a.LeftPosition, b.LeftPosition, body - offset + Vector3.left * 0.3f, blend);
        Ghost[3]!.position = Point(a.RightPosition, b.RightPosition, body - offset + Vector3.right * 0.3f, blend);
        float distance = Vector3.Distance(CreativeTools.Position, finish);
        if (distance > CreativeTools.Scale) leftFinish = true;
        if (leftFinish && elapsed - frames[0].Time > 0.5f && distance < 0.45f * CreativeTools.Scale)
        {
            float time = elapsed - frames[0].Time, difference = elapsed - frames[^1].Time;
            CreativeTools.Notice($"Echo Race: {time:0.00}s ({difference:+0.00;-0.00;0.00}s against the ghost)");
            StopRace();
        }
        else if (elapsed > frames[^1].Time + 30f) StopRace();
    }
    private static Vector3 Point(float[] a, float[] b, Vector3 fallback, float blend) => Vector3.Lerp(CreativeTools.Vector(a, fallback), CreativeTools.Vector(b, fallback), blend) + offset;
    private static void Begin()
    {
        recording = MacroManager.Selected;
        if (!MacroManager.Valid(recording) || recording == null || MacroManager.IsPlaying || MacroManager.Recording)
        {
            CreativeTools.Notice("Select a valid saved macro and stop macro playback first.");
            recording = null;
            return;
        }
        Vector3 first = CreativeTools.Vector(recording.Frames[0].Body, CreativeTools.Position);
        Vector3 reference = recording.HasRigPose && VRRig.LocalRig != null ? VRRig.LocalRig.transform.position : CreativeTools.Position;
        offset = reference - first;
        finish = CreativeTools.Vector(recording.Frames[^1].Body, first) + offset + CreativeTools.Position - reference;
        for (int i = 0; i < Ghost.Length; i++)
        {
            if (Ghost[i] == null) Ghost[i] = CreativeTools.Marker("Race Ghost", (i == 0 ? 0.25f : i == 1 ? 0.14f : 0.08f) * CreativeTools.Scale);
            Ghost[i]!.gameObject.SetActive(true);
            CreativeTools.Paint(Ghost[i]!.GetComponent<Renderer>(), CreativeTools.Theme);
        }
        started = Time.unscaledTime; frame = 0; leftFinish = Vector3.Distance(CreativeTools.Position, finish) > CreativeTools.Scale; running = true;
    }
    private static void StopRace()
    {
        running = false; recording = null;
        foreach (Transform? marker in Ghost) if (marker != null) marker.gameObject.SetActive(false);
    }
    [MenuDisable("Echo Race")]
    public static void Clear()
    {
        StopRace();
        for (int i = 0; i < Ghost.Length; i++) { if (Ghost[i] != null) Object.Destroy(Ghost[i]!.gameObject); Ghost[i] = null; }
        startInput = false;
    }
}
