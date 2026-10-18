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
using UnityEngine;
using ChudOS.Stuff;

namespace ChudOS.Core.Managers;

public static class SoundboardManager
{
    public sealed class SoundData
    {
        public string Name;
        public string Path;
        public SoundData(string name, string path) { Name = name; Path = path; }
    }
    public static readonly List<SoundData> Sounds = new();
    public static readonly float[] Presets = { 0.25f, 0.99f, 7.5f, 10f };
    public static int SelectedIndex;
    public static int VolumeIndex = 2;
    public static int MicVolumeIndex = 3;
    public static bool AutoPlay;
    public static bool Shuffle;
    public static bool Repeat;
    public static string CurrentName => Sounds.Count == 0 ? "No Sounds" : Sounds[Mathf.Clamp(SelectedIndex, 0, Sounds.Count - 1)].Name;
    public static bool IsPlaying => SoundManager.IsPlaying;
    public static event Action? Changed;
    private static MonoBehaviour? Host;
    private static Coroutine? Loading;
    private static int Generation;
    private static bool Started;
    private static bool LoadingClip;

    public static void Initialize(MonoBehaviour host)
    {
        if (Started) return;
        Host = host;
        Started = true;
        SoundManager.Init();
        SoundManager.ClipFinished += Finished;
        SetVolume(VolumeIndex);
        SetMicVolume(MicVolumeIndex);
        Reload();
    }
    public static void Reload()
    {
        Stop();
        Sounds.Clear();
        foreach (var sound in FileManager.GetSounds()) Sounds.Add(new SoundData(sound.Name, sound.Source));
        SelectedIndex = Mathf.Clamp(SelectedIndex, 0, Mathf.Max(0, Sounds.Count - 1));
        Debug.Log($"[{Constantss.GUID}] Loaded {Sounds.Count} sounds.");
        Changed?.Invoke();
    }
    public static void SetVolume(int index)
    {
        VolumeIndex = Mathf.Clamp(index, 0, Presets.Length - 1);
        SoundManager.SetVolume(Presets[VolumeIndex]);
        var button = ButtonManager.GetButton("Volume");
        if (button != null) button.OptionIndex = VolumeIndex;
        Changed?.Invoke();
    }
    public static void SetMicVolume(int index)
    {
        MicVolumeIndex = Mathf.Clamp(index, 0, Presets.Length - 1);
        SoundManager.SetMicrophoneVolume(Presets[MicVolumeIndex]);
        var button = ButtonManager.GetButton("Mic Volume");
        if (button != null) button.OptionIndex = MicVolumeIndex;
        Changed?.Invoke();
    }
    public static void SetPlaybackMode(SoundManager.PlaybackMode mode)
    {
        SoundManager.SetPlaybackMode(mode);
        var button = ButtonManager.GetButton("Sound Output");
        if (button != null) button.OptionIndex = (int)mode;
        Changed?.Invoke();
    }
    public static void Select(int index)
    {
        if (Sounds.Count == 0) return;
        SelectedIndex = (index % Sounds.Count + Sounds.Count) % Sounds.Count;
        Changed?.Invoke();
    }
    public static void Next() => Select(SelectedIndex + 1);
    public static void Previous() => Select(SelectedIndex - 1);
   
    public static void SetRepeat(bool value)
    {
        if (Repeat == value) return;
        Repeat = value;
        SoundManager.SetRepeat(value);
        ButtonManager.SetEnabled("Repeat", value);
        Changed?.Invoke();
    }
    public static void SetAutoPlay(bool value)
    {
        if (AutoPlay == value) return;
        AutoPlay = value;
        ButtonManager.SetEnabled("Autoplay", value);
        Changed?.Invoke();
    }
    public static void SetShuffle(bool value)
    {
        if (Shuffle == value) return;
        Shuffle = value;
        ButtonManager.SetEnabled("Shuffle", value);
        Changed?.Invoke();
    }
    public static void Play()
    {
        if (Host == null || Sounds.Count == 0) return;
        Stop();
        int generation = Generation;
        string path = Sounds[SelectedIndex].Path;
        Loading = Host.StartCoroutine(PlayRoutine(path, generation));
    }
    private static IEnumerator PlayRoutine(string path, int generation)
    {
        LoadingClip = true;
        AudioClip? loaded = null;
        yield return SoundManager.Load(path, clip => loaded = clip);
        LoadingClip = false;
        Loading = null;
        if (generation != Generation || loaded == null) yield break;
        SoundManager.Play(loaded);
        Changed?.Invoke();
    }
    public static void Stop()
    {
        Generation++;
        if (Loading != null && Host != null) Host.StopCoroutine(Loading);
        Loading = null;
        LoadingClip = false;
        SoundManager.Stop();
        Changed?.Invoke();
    }
    public static void TogglePlay()
    {
        if (SoundManager.IsPlaying || LoadingClip || SoundManager.IsPaused) Stop();
        else Play();
    }
    public static void Tick() => SoundManager.Tick();
    private static void Finished()
    {
        if (!AutoPlay || Repeat || Sounds.Count == 0) return;
        if (Shuffle && Sounds.Count > 1)
        {
            int next = UnityEngine.Random.Range(0, Sounds.Count - 1);
            if (next >= SelectedIndex) next++;
            Select(next);
        }
        else Next();
        Play();
    }
    public static void Shutdown()
    {
        if (!Started) return;
        Stop();
        SoundManager.ClipFinished -= Finished;
        SoundManager.Shutdown();
        Host = null;
        Started = false;
    }
}
