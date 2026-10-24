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
        public string Title;
        public string Artist = "";
        private bool metadataLoaded;
        public SoundData(string name, string path) { Name = Title = name; Path = path; }
        public void ReadMetadata()
        {
            if (metadataLoaded) return;
            metadataLoaded = true;
            (Title, Artist) = AudioMetadata.Read(Path, Name);
        }
    }
    public static readonly List<SoundData> Sounds = new();
    public static readonly float[] Presets = { 0.25f, 0.50f, 0.75f, 1f };
    public static int SelectedIndex;
    public static int VolumeIndex = 2;
    public static int MicVolumeIndex = 3;
    public static bool AutoPlay;
    public static bool Shuffle;
    public static bool Repeat;
    public static string CurrentName => Sounds.Count == 0 ? "No Sounds" : Sounds[Mathf.Clamp(SelectedIndex, 0, Sounds.Count - 1)].Name;
    public static bool IsPlaying => SoundManager.IsPlaying;
    public static SoundData? Selected => Sounds.Count == 0 ? null : Sounds[Mathf.Clamp(SelectedIndex, 0, Sounds.Count - 1)];
    public static SoundData? Playing { get; private set; }
    public static bool IsLoading => LoadingClip;
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
        FileManager.SoundsAvailable += RefreshList;
        SetVolume(VolumeIndex);
        SetMicVolume(MicVolumeIndex);
        Reload();
    }
    public static void Reload()
    {
        Stop();
        SoundManager.ClearCache();
        RefreshList();
    }
    private static void RefreshList()
    {
        string? selected = Selected?.Path;
        Sounds.Clear();
        HashSet<string> paths = new(StringComparer.Ordinal);
        foreach (var sound in FileManager.GetSounds()) if (paths.Add(sound.Source)) Sounds.Add(new SoundData(sound.Name, sound.Source));
        int previous = Sounds.FindIndex(sound => sound.Path == selected);
        if (previous >= 0) SelectedIndex = previous;
        SelectedIndex = Mathf.Clamp(SelectedIndex, 0, Mathf.Max(0, Sounds.Count - 1));
        Selected?.ReadMetadata();
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
        Selected?.ReadMetadata();
        Changed?.Invoke();
    }
    public static void Next() => Navigate(1);
    public static void Previous() => Navigate(-1);
    private static void Navigate(int direction)
    {
        bool paused = SoundManager.IsPaused;
        bool active = SoundManager.IsPlaying || paused || LoadingClip;
        int current = active && Playing != null ? Sounds.FindIndex(sound => sound.Path == Playing.Path) : SelectedIndex;
        Select((current < 0 ? SelectedIndex : current) + direction);
        if (active) Play(paused);
    }
   
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
    public static void Play(bool pauseOnStart = false)
    {
        if (Host == null || Sounds.Count == 0) return;
        Stop();
        int generation = Generation;
        Playing = Selected;
        Playing?.ReadMetadata();
        LoadingClip = true;
        Loading = Host.StartCoroutine(PlayRoutine(Playing!.Path, generation, pauseOnStart));
        Changed?.Invoke();
    }
    private static IEnumerator PlayRoutine(string path, int generation, bool pauseOnStart)
    {
        LoadingClip = true;
        AudioClip? loaded = null;
        yield return SoundManager.Load(path, clip => loaded = clip);
        if (generation != Generation) yield break;
        LoadingClip = false;
        Loading = null;
        if (loaded == null) { Playing = null; Changed?.Invoke(); yield break; }
        SoundManager.Play(loaded, pauseOnStart);
        Changed?.Invoke();
    }
    public static void Stop()
    {
        Generation++;
        if (Loading != null && Host != null) Host.StopCoroutine(Loading);
        Loading = null;
        LoadingClip = false;
        SoundManager.Stop();
        Playing = null;
        Changed?.Invoke();
    }
    public static void TogglePlay()
    {
        if (SoundManager.IsPlaying || LoadingClip || SoundManager.IsPaused) Stop();
        else Play();
    }
    public static void Tick() => SoundManager.Tick();
    public static void TogglePause()
    {
        if (LoadingClip) return;
        if (SoundManager.IsPaused) SoundManager.Resume();
        else if (SoundManager.IsPlaying) SoundManager.Pause();
        else { Play(); return; }
        Changed?.Invoke();
    }
    private static void Finished()
    {
        if (!AutoPlay || Repeat || Sounds.Count == 0) { Changed?.Invoke(); return; }
        int current = Playing == null ? SelectedIndex : Sounds.FindIndex(sound => sound.Path == Playing.Path);
        if (current >= 0) SelectedIndex = current;
        if (Shuffle && Sounds.Count > 1)
        {
            int next = UnityEngine.Random.Range(0, Sounds.Count - 1);
            if (next >= SelectedIndex) next++;
            Select(next);
        }
        else Select(SelectedIndex + 1);
        Play();
    }
    public static void Shutdown()
    {
        if (!Started) return;
        Stop();
        SoundManager.ClipFinished -= Finished;
        FileManager.SoundsAvailable -= RefreshList;
        SoundManager.Shutdown();
        Host = null;
        Started = false;
    }
}
