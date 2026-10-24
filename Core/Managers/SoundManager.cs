// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra
//
// This file is part of ChudOS.
// ChudOS is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 3,
// as published by the Free Software Foundation.
//
// ChudOS is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ChudOS. If not, see <https://www.gnu.org/licenses/>.

using System.Collections;
using System.IO;
using ChudOS.Stuff;
using Photon.Voice;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace ChudOS.Core.Managers;

public static class SoundManager
{
    public enum PlaybackMode { Local, Microphone, Both }
    private static readonly Dictionary<string, AudioClip> Cache = new(StringComparer.Ordinal);
    private static readonly Dictionary<AudioClip, float[]> Pcm = new();
    private static AudioSource? Source;
    private static Recorder? MicRecorder;
    private static SoundPcmReader? MicReader;
    private static Recorder.InputSourceType OriginalSource;
    private static Func<IAudioDesc>? OriginalFactory;
    private static AudioClip? OriginalClip;
    private static bool OriginalLoop, OriginalRecording;
    private static bool Paused, WasPlaying, ManualStop;
    private static int CacheGeneration;
    private static float NextMicRetry;

    public static AudioClip? CurrentClip => Source != null ? Source.clip : null;
    public static bool IsPlaying => Source != null && Source.isPlaying;
    public static bool IsPaused => Paused;
    public static float CurrentTime => Source != null && Source.clip != null ? Source.time : 0f;
    public static float CurrentLength => CurrentClip != null ? CurrentClip.length : 0f;
    public static float Volume { get; private set; } = 0.75f;
    public static float MicrophoneVolume { get; private set; } = 1f;
    public static PlaybackMode Mode { get; private set; }
    public static bool Repeat { get; private set; }
    public static event Action? ClipFinished;

    public static void Init()
    {
        if (Source != null) return;
        GameObject obj = new(Constantss.GUID + ".SoundObject");
        UnityEngine.Object.DontDestroyOnLoad(obj);
        Source = obj.AddComponent<AudioSource>();
        Source.playOnAwake = false;
        Source.spatialBlend = 0f;
        Source.volume = Volume;
    }

    private static void UpdateVolume()
    {
        if (Source != null) Source.volume = Mode == PlaybackMode.Microphone && MicRecorder != null ? 0f : Volume;
    }

    public static void SetVolume(float volume)
    {
        if (float.IsNaN(volume) || float.IsInfinity(volume)) return;
        Volume = Mathf.Clamp01(volume);
        UpdateVolume();
    }
    public static void SetMicrophoneVolume(float volume)
    {
        if (float.IsNaN(volume) || float.IsInfinity(volume)) return;
        MicrophoneVolume = Mathf.Clamp01(volume);
        if (MicReader != null) MicReader.Gain = MicrophoneVolume;
    }

    public static void SetPlaybackMode(PlaybackMode mode)
    {
        if (!Enum.IsDefined(typeof(PlaybackMode), mode) || Mode == mode) return;
        Mode = mode;
        StopMic();
        if (Source != null && Source.isPlaying && !Paused && Mode != PlaybackMode.Local && Source.clip != null) StartMic(Source.clip);
        UpdateVolume();
    }

    public static void SetRepeat(bool repeat)
    {
        Repeat = repeat;
        if (Source != null) Source.loop = repeat;
        if (MicReader != null) MicReader.Loop = repeat;
    }

    public static void Play(AudioClip clip, bool paused = false)
    {
        if (clip == null) return;
        Init();
        if (Source == null) return;
        StopMic();
        Source.Stop();
        Source.clip = clip;
        Source.loop = Repeat;
        Source.Play();
        if (paused) Source.Pause();
        Paused = paused;
        WasPlaying = !paused;
        ManualStop = false;
        if (!paused && Mode != PlaybackMode.Local && !StartMic(clip))
            Debug.LogWarning($"[{Constantss.GUID}] Microphone output unavailable; using local audio.");
        UpdateVolume();
    }

    public static void Pause()
    {
        if (Source == null || !Source.isPlaying) return;
        Source.Pause();
        Paused = true;
        StopMic();
    }

    public static void Resume()
    {
        if (Source == null || !Paused || Source.clip == null) return;
        Source.UnPause();
        Paused = false;
        WasPlaying = true;
        ManualStop = false;
        if (Mode != PlaybackMode.Local) StartMic(Source.clip);
        UpdateVolume();
    }

    public static void Stop()
    {
        ManualStop = true;
        Paused = WasPlaying = false;
        if (Source != null) { Source.Stop(); Source.clip = null; }
        StopMic();
    }

    public static void Tick()
    {
        if (Source == null || (!WasPlaying && MicRecorder == null) || Paused) return;
        bool playing = Source.isPlaying;
        bool muted = ButtonManager.GetButton("Microphone Mute")?.Enabled == true;
        Recorder? recorder = GorillaTagger.Instance != null ? GorillaTagger.Instance.myRecorder : null;
        if (MicRecorder != null && muted) { StopMic(); UpdateVolume(); }
        if (playing && !Paused && !muted && Mode != PlaybackMode.Local && (MicRecorder == null || MicRecorder != recorder) && Time.unscaledTime >= NextMicRetry)
        {
            NextMicRetry = Time.unscaledTime + 1f;
            StopMic();
            if (Source.clip != null) StartMic(Source.clip);
            UpdateVolume();
        }
        if (WasPlaying && !playing && !Paused && !ManualStop && Source.clip != null)
        {
            WasPlaying = false;
            StopMic();
            ClipFinished?.Invoke();
            return;
        }
        WasPlaying = playing;
    }

    private static bool StartMic(AudioClip clip)
    {
        Recorder? recorder = GorillaTagger.Instance != null ? GorillaTagger.Instance.myRecorder : null;
        if (recorder == null || Source == null || ButtonManager.GetButton("Microphone Mute")?.Enabled == true) return false;
        try
        {
            if (!Pcm.TryGetValue(clip, out float[] data))
            {
                Pcm.Clear();
                data = new float[checked(clip.samples * clip.channels)];
                if (!clip.GetData(data, 0)) throw new InvalidOperationException("Unable to read sound samples.");
                Pcm.Add(clip, data);
            }
            MicRecorder = recorder;
            OriginalSource = recorder.SourceType;
            OriginalFactory = recorder.InputFactory;
            OriginalClip = recorder.AudioClip;
            OriginalLoop = recorder.LoopAudioClip;
            OriginalRecording = recorder.IsRecording;
            int start = Source.timeSamples;
            int rate = clip.frequency, channels = clip.channels;
            recorder.StopRecording();
            recorder.SourceType = Recorder.InputSourceType.Factory;
            recorder.InputFactory = () => MicReader = new SoundPcmReader(data, rate, channels, start, MicrophoneVolume, Repeat);
            recorder.RestartRecording(true);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[{Constantss.GUID}] Microphone playback: {ex.Message}");
            StopMic();
            return false;
        }
    }

    private static void StopMic()
    {
        Recorder? recorder = MicRecorder;
        MicRecorder = null;
        if (recorder != null)
        {
            try
            {
                recorder.StopRecording();
                recorder.InputFactory = OriginalFactory;
                recorder.SourceType = OriginalSource;
                recorder.AudioClip = OriginalClip;
                recorder.LoopAudioClip = OriginalLoop;
                if (OriginalRecording && ButtonManager.GetButton("Microphone Mute")?.Enabled != true) recorder.StartRecording();
            }
            catch (Exception ex) { Debug.LogWarning($"[{Constantss.GUID}] Restore microphone: {ex.Message}"); }
        }
        MicReader?.Dispose();
        MicReader = null;
        OriginalFactory = null;
        OriginalClip = null;
    }

    public static IEnumerator Load(string path, Action<AudioClip?> result)
    {
        if (Cache.TryGetValue(path, out AudioClip cached) && cached != null) { result(cached); yield break; }
        int generation = CacheGeneration;
        bool remote = Uri.TryCreate(path, UriKind.Absolute, out Uri uri) && (uri.Scheme == "https" || uri.Scheme == "http");
        string source = remote ? path : new Uri(Path.GetFullPath(path)).AbsoluteUri;
        string ext = remote ? Path.GetExtension(uri!.AbsolutePath).ToLowerInvariant() : Path.GetExtension(path).ToLowerInvariant();
        AudioType type = ext == ".mp3" ? AudioType.MPEG : ext == ".ogg" ? AudioType.OGGVORBIS : ext == ".wav" ? AudioType.WAV : AudioType.UNKNOWN;
        if (type == AudioType.UNKNOWN) { result(null); yield break; }
        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(source, type);
        request.timeout = 15;
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[{Constantss.GUID}] Could not load sound: {request.error}");
            result(null);
            yield break;
        }
        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
        if (generation != CacheGeneration) { if (clip != null) UnityEngine.Object.Destroy(clip); result(null); yield break; }
        if (clip != null) Cache[path] = clip;
        result(clip);
    }

    public static void ClearCache()
    {
        Stop();
        CacheGeneration++;
        Pcm.Clear();
        foreach (AudioClip clip in Cache.Values) if (clip != null) UnityEngine.Object.Destroy(clip);
        Cache.Clear();
    }

    public static void Shutdown()
    {
        ClearCache();
        if (Source != null) UnityEngine.Object.Destroy(Source.gameObject);
        Source = null;
    }
}
