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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ChudOS.Stuff;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace ChudOS.Core.Managers;

public static class SoundManager
{
    public enum PlaybackMode { Local, Microphone, Both }

    private static readonly Dictionary<string, AudioClip> Cache = new();
    private static AudioSource? Source;
    private static Recorder? MicRecorder;
    private static AudioClip? MicClip;
    private static Recorder.InputSourceType OriginalSource;
    private static AudioClip? OriginalClip;
    private static bool OriginalLoop;
    private static bool Paused;
    private static bool WasPlaying;
    private static bool ManualStop;

    public static AudioClip? CurrentClip => Source != null ? Source.clip : null;
    public static bool IsPlaying => Source != null && Source.isPlaying;
    public static bool IsPaused => Paused;
    public static float CurrentTime => Source != null && Source.clip != null ? Source.time : 0f;
    public static float CurrentLength => CurrentClip != null ? CurrentClip.length : 0f;
    public static float Volume { get; private set; } = 0.75f;
    public static float MicrophoneVolume { get; private set; } = 1f;
    public static PlaybackMode Mode { get; private set; } = PlaybackMode.Local;
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

    public static void SetVolume(float volume)
    {
        Volume = Mathf.Clamp01(volume);
        if (Source != null) Source.volume = Mode == PlaybackMode.Microphone ? 0f : Volume;
        RestartMicIfPlaying();
    }

    public static void SetMicrophoneVolume(float volume)
    {
        MicrophoneVolume = Mathf.Clamp01(volume);
        RestartMicIfPlaying();
    }

    public static void SetPlaybackMode(PlaybackMode mode)
    {
        if (Mode == mode) return;
        Mode = mode;
        if (Source == null) return;
        AudioClip? clip = Source.clip;
        bool playing = IsPlaying || Paused;
        if (playing && clip != null) Play(clip);
        else Source.volume = Mode == PlaybackMode.Microphone ? 0f : Volume;
    }

    public static void SetRepeat(bool repeat)
    {
        Repeat = repeat;
        if (Source != null) Source.loop = repeat;
        if (MicRecorder != null) MicRecorder.LoopAudioClip = repeat;
    }

    public static void Play(AudioClip clip)
    {
        if (clip == null) return;
        Init();
        if (Source == null) return;

        StopMic();
        Source.Stop();
        Source.clip = clip;
        Source.loop = Repeat;
        Source.volume = Mode == PlaybackMode.Microphone ? 0f : Volume;
        Source.Play();

        Paused = false;
        WasPlaying = true;
        ManualStop = false;

        if (Mode != PlaybackMode.Local && !StartMic(clip))
        {
            Source.volume = Volume;
            Debug.LogWarning($"[{Constantss.GUID}] Mic playback unavailable; using local audio.");
        }
    }

    public static void Pause()
    {
        if (Source == null || !Source.isPlaying) return;
        Source.Pause();
        StopMic();
        Paused = true;
    }

    public static void Resume()
    {
        if (Source == null || !Paused) return;
        if (Mode != PlaybackMode.Local && Source.clip != null)
        {
            // Photon Voice AudioClip input starts at the beginning on restart.
            Play(Source.clip);
            return;
        }
        Source.UnPause();
        Paused = false;
        WasPlaying = true;
    }

    public static void Stop()
    {
        ManualStop = true;
        Paused = false;
        WasPlaying = false;
        if (Source != null) Source.Stop();
        StopMic();
    }

    public static void Tick()
    {
        if (Source == null) return;
        bool playing = Source.isPlaying;
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
        if (recorder == null) return false;

        try
        {
            MicRecorder = recorder;
            OriginalSource = recorder.SourceType;
            OriginalClip = recorder.AudioClip;
            OriginalLoop = recorder.LoopAudioClip;

            float volume = Mathf.Clamp01(Volume * MicrophoneVolume);
            MicClip = CreateMicrophoneClip(clip, volume);
            recorder.SourceType = Recorder.InputSourceType.AudioClip;
            recorder.AudioClip = MicClip;
            recorder.LoopAudioClip = Repeat;
            recorder.RestartRecording(true);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{Constantss.GUID}] Mic playback failed: {ex}");
            StopMic();
            return false;
        }
    }

    private static AudioClip CreateMicrophoneClip(AudioClip clip, float volume)
    {
        if (Mathf.Approximately(volume, 1f)) return clip;

        float[] samples = new float[checked(clip.samples * clip.channels)];
        if (!clip.GetData(samples, 0)) throw new InvalidOperationException("Unable to read the sound samples.");
        for (int i = 0; i < samples.Length; i++) samples[i] = Mathf.Clamp(samples[i] * volume, -1f, 1f);

        AudioClip copy = AudioClip.Create(clip.name + "_Mic", clip.samples, clip.channels, clip.frequency, false);
        copy.SetData(samples, 0);
        return copy;
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
                recorder.SourceType = OriginalSource;
                recorder.AudioClip = OriginalClip;
                recorder.LoopAudioClip = OriginalLoop;
                recorder.StartRecording();
            }
            catch (Exception ex) { Debug.LogWarning($"[{Constantss.GUID}] Could not restore microphone: {ex.Message}"); }
        }

        if (MicClip != null && MicClip != CurrentClip) UnityEngine.Object.Destroy(MicClip);
        MicClip = null;
    }

    private static void RestartMicIfPlaying()
    {
        if (MicRecorder == null || Source == null || !Source.isPlaying || Source.clip == null) return;
        // Restart both outputs together so microphone and local audio stay in sync.
        Play(Source.clip);
    }

    public static IEnumerator Load(string path, Action<AudioClip?> result)
    {
        if (Cache.TryGetValue(path, out AudioClip cached)) { result(cached); yield break; }
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
            Debug.LogError($"[{Constantss.GUID}] Could not load sound: {request.error}");
            result(null);
            yield break;
        }
        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
        if (clip != null) Cache[path] = clip;
        result(clip);
    }

    public static void Shutdown()
    {
        Stop();
        if (Source != null) UnityEngine.Object.Destroy(Source.gameObject);
        Source = null;
        foreach (AudioClip clip in Cache.Values) if (clip != null) UnityEngine.Object.Destroy(clip);
        Cache.Clear();
    }
}
