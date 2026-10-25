// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries;
using ChudOS.Libraries.Notifications;
using Newtonsoft.Json;
using Photon.Pun;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Core.Managers;

public static class MacroManager
{
    public sealed class Frame
    {
        public float Time;
        public float[] Body = Array.Empty<float>(), Velocity = Array.Empty<float>();
        public float[] LeftPosition = Array.Empty<float>(), LeftRotation = Array.Empty<float>();
        public float[] RightPosition = Array.Empty<float>(), RightRotation = Array.Empty<float>();
        public bool LeftGrip, RightGrip;
    }

    public sealed class RecordingData
    {
        public int Version = 1;
        public string Name = "";
        public List<Frame> Frames = new();
        [JsonIgnore] public string File = "";
    }

    public static readonly List<RecordingData> Recordings = new();
    public static string Folder => Path.Combine(FileManager.RootFolder, "Macros");
    public static RecordingData? Selected => Recordings.Count == 0 ? null : Recordings[Mathf.Clamp(selected, 0, Recordings.Count - 1)];
    public static bool Recording { get; private set; }
    public static bool IsPlaying => playing != null;
    public static bool HasDraft => draft != null;
    public static bool ControlsEnabled { get; private set; }

    private const float Step = 0.05f, MaximumDuration = 120f;
    private const int MaximumFrames = 2402;
    private const long MaximumFileSize = 8 * 1024 * 1024;
    private static readonly JsonSerializerSettings JsonSettings = new() { MaxDepth = 16, TypeNameHandling = TypeNameHandling.None };
    private static RecordingData? draft, playing;
    private static List<Frame> captured = new();
    private static LineRenderer? preview;
    private static object? room;
    private static int selected, playbackIndex;
    private static float started, nextSample;
    private static bool lastLeft, lastRight, triggerPlayback;

    public static void Reload()
    {
        string? previous = Selected?.File;
        Recordings.Clear();
        ClearPreview();
        try
        {
            Directory.CreateDirectory(Folder);
            foreach (string file in Directory.GetFiles(Folder, "*.json"))
            {
                try
                {
                    if (new FileInfo(file).Length > MaximumFileSize) continue;
                    var data = JsonConvert.DeserializeObject<RecordingData>(File.ReadAllText(file), JsonSettings);
                    if (!Valid(data)) continue;
                    data!.Name = CleanName(data.Name);
                    data.File = file;
                    Recordings.Add(data);
                }
                catch (Exception ex) { Debug.LogWarning($"[ChudOS] Skipped macro {Path.GetFileName(file)}: {ex.Message}"); }
            }
            Recordings.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            int restored = Recordings.FindIndex(data => data.File == previous);
            selected = restored >= 0 ? restored : 0;
        }
        catch (Exception ex) { Report($"Could not load macros: {ex.Message}"); }
    }

    public static void Select(int direction)
    {
        if (Recordings.Count == 0) return;
        selected = (selected + direction % Recordings.Count + Recordings.Count) % Recordings.Count;
        ClearPreview();
    }

    public static void SetControls(bool enabled)
    {
        ControlsEnabled = enabled;
        if (!enabled) Stop();
        if (PortTools.Ready) { lastLeft = InputLib.LeftTrigger; lastRight = InputLib.RightTrigger; }
        UpdateRegistration();
    }

    public static void StartRecording()
    {
        if (!PortTools.Ready || Recording || !CheckConflicts()) return;
        Stop();
        ClearPreview();
        captured = new List<Frame>(600);
        started = Time.unscaledTime;
        nextSample = started + Step;
        room = PhotonNetwork.CurrentRoom;
        Recording = true;
        Capture();
        UpdateRegistration();
        Report("Macro recording started.");
    }

    public static void Stop()
    {
        if (Recording)
        {
            Recording = false;
            if (captured.Count >= 2)
            {
                draft = new RecordingData { Name = "New Macro", Frames = captured };
                Report("Recording ready. Open Macros GUI to save it.");
            }
        }
        playing = null;
        triggerPlayback = false;
        playbackIndex = 0;
        UpdateRegistration();
    }

    public static void Play(bool requireTrigger = false)
    {
        var data = Selected;
        if (!PortTools.Ready || !Valid(data) || !CheckConflicts()) return;
        if (Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, Vector(data!.Frames[0].Body)) > 1f)
        {
            Report("Move within 1 metre of the macro starting point.");
            return;
        }
        Stop();
        ClearPreview();
        playing = data;
        triggerPlayback = requireTrigger;
        started = Time.unscaledTime;
        room = PhotonNetwork.CurrentRoom;
        UpdateRegistration();
    }

    public static void SaveDraft(string name)
    {
        if (draft == null || Recording) return;
        var data = draft;
        data.Name = CleanName(name);
        if (!Write(data, Path.Combine(Folder, $"{Guid.NewGuid():N}.json"))) return;
        draft = null;
        Reload();
        selected = Math.Max(0, Recordings.FindIndex(item => item.File == data.File));
        Report("Macro saved.");
    }

    public static void Rename(string name)
    {
        var data = Selected;
        if (data == null || IsPlaying || Recording) return;
        string previous = data.Name;
        data.Name = CleanName(name);
        if (!Write(data, data.File)) data.Name = previous;
    }

    public static void Delete()
    {
        var data = Selected;
        if (data == null || IsPlaying || Recording) return;
        try { File.Delete(data.File); Reload(); }
        catch (Exception ex) { Report($"Could not delete macro: {ex.Message}"); }
    }

    public static void Preview()
    {
        ClearPreview();
        var data = Selected;
        if (data == null || !PortTools.Ready) return;
        preview = PortTools.Line("ChudOS Macro Preview", 0.015f, data.Frames.Count);
        preview.startColor = preview.endColor = ChudOS.Core.Main.PrimaryColorValue;
        for (int i = 0; i < data.Frames.Count; i++) preview.SetPosition(i, Vector(data.Frames[i].Body));
        PortTools.Register(Shutdown);
    }

    public static void ClearPreview()
    {
        if (preview != null) Object.Destroy(preview.gameObject);
        preview = null;
    }

    private static void Tick()
    {
        try
        {
            if (!PortTools.Ready || ((Recording || IsPlaying) && !ReferenceEquals(room, PhotonNetwork.CurrentRoom)))
            {
                Stop();
                ClearPreview();
                return;
            }
            bool left = InputLib.LeftTrigger, right = InputLib.RightTrigger;
            if (ControlsEnabled)
            {
                if (left && !lastLeft) StartRecording();
                if (!left && lastLeft && Recording) Stop();
                if (right && !lastRight && !Recording) Play(true);
            }
            lastLeft = left;
            lastRight = right;

            if (Recording)
            {
                if (Time.unscaledTime - started >= MaximumDuration || captured.Count >= MaximumFrames) Stop();
                else if (Time.unscaledTime >= nextSample) { Capture(); nextSample = Time.unscaledTime + Step; }
            }
            if (playing == null) return;
            if (triggerPlayback && !right) { Stop(); return; }

            float elapsed = Time.unscaledTime - started;
            var frames = playing.Frames;
            while (playbackIndex + 1 < frames.Count && frames[playbackIndex + 1].Time <= elapsed) playbackIndex++;
            if (playbackIndex + 1 >= frames.Count) { Apply(frames[^1], frames[^1], 0f); Stop(); return; }
            Frame a = frames[playbackIndex], b = frames[playbackIndex + 1];
            Apply(a, b, Mathf.InverseLerp(a.Time, b.Time, elapsed));
        }
        catch (Exception ex) { Stop(); ClearPreview(); Report($"Macro stopped: {ex.Message}"); }
    }

    private static void Capture()
    {
        var tagger = GorillaTagger.Instance;
        captured.Add(new Frame
        {
            Time = captured.Count == 0 ? 0f : Time.unscaledTime - started,
            Body = Values(tagger.bodyCollider.transform.position), Velocity = Values(tagger.rigidbody.linearVelocity),
            LeftPosition = Values(tagger.leftHandTransform.position), LeftRotation = Values(tagger.leftHandTransform.rotation),
            RightPosition = Values(tagger.rightHandTransform.position), RightRotation = Values(tagger.rightHandTransform.rotation),
            LeftGrip = InputLib.LeftGrab, RightGrip = InputLib.RightGrab
        });
    }

    private static void Apply(Frame a, Frame b, float t)
    {
        var tagger = GorillaTagger.Instance;
        PortTools.Teleport(Vector3.Lerp(Vector(a.Body), Vector(b.Body), t));
        tagger.rigidbody.linearVelocity = Vector3.Lerp(Vector(a.Velocity), Vector(b.Velocity), t);
        tagger.leftHandTransform.SetPositionAndRotation(Vector3.Lerp(Vector(a.LeftPosition), Vector(b.LeftPosition), t), Quaternion.Slerp(Rotation(a.LeftRotation), Rotation(b.LeftRotation), t));
        tagger.rightHandTransform.SetPositionAndRotation(Vector3.Lerp(Vector(a.RightPosition), Vector(b.RightPosition), t), Quaternion.Slerp(Rotation(a.RightRotation), Rotation(b.RightRotation), t));
    }

    internal static bool Conflicts(ButtonManager.ButtonData button) => button.Type == ChudOS.Stuff.ButtonType.Toggle && (button.Category == "Rig" || button.Category == "Detected" || button.Category == "Movement" && !button.Name.Contains("Platforms") && button.Name != "Platform Gun");
    private static bool CheckConflicts()
    {
        foreach (var button in ButtonManager.Buttons)
            if (button.Enabled && Conflicts(button)) { Report($"Disable {button.Name} before using a macro."); return false; }
        return true;
    }

    internal static bool Valid(RecordingData? data)
    {
        if (data?.Frames == null || data.Version != 1 || data.Frames.Count < 2 || data.Frames.Count > MaximumFrames) return false;
        float previous = -1f;
        foreach (var frame in data.Frames)
        {
            if (frame == null || !Finite(frame.Time) || frame.Time <= previous || frame.Time > MaximumDuration) return false;
            if (!ValidValues(frame.Body, 3) || !ValidValues(frame.Velocity, 3) || !ValidValues(frame.LeftPosition, 3) || !ValidValues(frame.RightPosition, 3)) return false;
            if (!ValidRotation(frame.LeftRotation) || !ValidRotation(frame.RightRotation)) return false;
            previous = frame.Time;
        }
        return data.Frames[0].Time == 0f;
    }

    private static bool ValidValues(float[]? values, int count) => values != null && values.Length == count && values.All(value => Finite(value) && Mathf.Abs(value) <= 100000f);
    private static bool ValidRotation(float[]? values) => ValidValues(values, 4) && values!.Sum(value => value * value) is > 0.001f and < 4f;
    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    private static float[] Values(Vector3 value) => new[] { value.x, value.y, value.z };
    private static float[] Values(Quaternion value) => new[] { value.x, value.y, value.z, value.w };
    private static Vector3 Vector(float[] value) => new(value[0], value[1], value[2]);
    private static Quaternion Rotation(float[] value) => new Quaternion(value[0], value[1], value[2], value[3]).normalized;
    private static string CleanName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "New Macro";
        string clean = new(name!.Trim().Where(c => !char.IsControl(c) && c != '<' && c != '>').Take(60).ToArray());
        return clean.Length == 0 ? "New Macro" : clean;
    }

    private static bool Write(RecordingData data, string path)
    {
        string temporary = path + ".tmp";
        try
        {
            if (!Valid(data)) throw new InvalidDataException("The recording contains invalid frames.");
            Directory.CreateDirectory(Folder);
            File.WriteAllText(temporary, JsonConvert.SerializeObject(data, JsonSettings));
            if (File.Exists(path)) File.Replace(temporary, path, null);
            else File.Move(temporary, path);
            data.File = path;
            return true;
        }
        catch (Exception ex) { Report($"Could not save macro: {ex.Message}"); return false; }
        finally
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch (Exception ex) { Debug.LogWarning($"[ChudOS] Macro cleanup: {ex.Message}"); }
        }
    }

    private static void UpdateRegistration()
    {
        if (ControlsEnabled || Recording || IsPlaying) PortTools.Register(Shutdown, Tick);
        else PortTools.RemoveTick(Tick);
    }
    private static void Report(string message) => NotifyLib.SendNotificationALL(message, 3500);
    public static void Shutdown() { ControlsEnabled = false; Stop(); ClearPreview(); }
}
