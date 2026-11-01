// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ChudOS.Mods.Settings;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ChudOS.Core.Managers;

internal sealed class JarvisSpeech : IDisposable
{
    private AudioSource? source;
    private UnityWebRequest? request;
    private Process? synthesizer;
    private AudioClip? readyTone;
    private readonly Dictionary<string, AudioClip> clips = new(StringComparer.Ordinal);
    private readonly Queue<string> clipOrder = new();
    private string Folder => Path.Combine(FileManager.RootFolder, "Jarvis", "SpeechCache");
    internal string? Error { get; private set; }

    internal JarvisSpeech(GameObject host)
    {
        source = host.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }
    internal void Stop()
    {
        request?.Abort();
        if (source != null) source.Stop();
        if (synthesizer != null)
        {
            try { if (!synthesizer.HasExited) synthesizer.Kill(); } catch (InvalidOperationException) { }
            synthesizer.Dispose(); synthesizer = null;
        }
    }
    public void Dispose()
    {
        Stop();
        foreach (AudioClip clip in clips.Values) if (clip != null) UnityEngine.Object.Destroy(clip);
        clips.Clear(); clipOrder.Clear();
        if (readyTone != null) UnityEngine.Object.Destroy(readyTone);
        readyTone = null;
        if (source != null) UnityEngine.Object.Destroy(source);
        source = null;
    }
    internal void SignalListening()
    {
        if (!JarvisSettings.Spoken || source == null) return;
        if (readyTone == null)
        {
            const int samples = 2205;
            var data = new float[samples];
            for (int i = 0; i < samples; i++) data[i] = Mathf.Sin(i * Mathf.PI * 2f * 880f / 22050f) * Mathf.Sin(i * Mathf.PI / (samples - 1)) * 0.2f;
            readyTone = AudioClip.Create("Jarvis Ready", samples, 1, 22050, false);
            readyTone.SetData(data, 0);
        }
        source.volume = JarvisSettings.Volume;
        source.PlayOneShot(readyTone);
    }
    internal IEnumerator Speak(string text)
    {
        Error = null;
        if (!JarvisSettings.Spoken || source == null || string.IsNullOrWhiteSpace(text)) yield break;
        text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]*>|[*_`]", "").Trim();
        if (text.Length > 550) text = text.Substring(0, 550);
        string key;
        using (var sha = SHA256.Create()) key = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(JarvisSettings.Voice + ":" + text))).Replace("-", "");
        if (!clips.TryGetValue(key, out AudioClip clip) || clip == null)
        {
            bool folderReady = true;
            try { Directory.CreateDirectory(Folder); TrimCache(); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { Error = "Speech cache is unavailable."; folderReady = false; }
            if (!folderReady) yield break;
            string path = Path.Combine(Folder, key + ".mp3");
            if (JarvisSettings.Voice != 0 && !File.Exists(path)) yield return DownloadSpeech(text, path);
            if (JarvisSettings.Voice == 0 || !File.Exists(path))
            {
                path = Path.Combine(Folder, key + ".wav");
                if (!File.Exists(path))
                {
                    if (!StartWindowsVoice(text, path)) yield break;
                    float deadline = Time.realtimeSinceStartup + 20f;
                    while (synthesizer != null && !synthesizer.HasExited && Time.realtimeSinceStartup < deadline) yield return null;
                    if (synthesizer == null) yield break;
                    bool success = synthesizer.HasExited && synthesizer.ExitCode == 0 && File.Exists(path);
                    StopProcess();
                    if (!success) { Error = "Windows voice synthesis failed. Check installed Windows speech voices."; DeleteFile(path); yield break; }
                }
            }
            using UnityWebRequest audio = UnityWebRequestMultimedia.GetAudioClip(new Uri(path).AbsoluteUri, path.EndsWith(".wav", StringComparison.Ordinal) ? AudioType.WAV : AudioType.MPEG);
            request = audio; audio.timeout = 15;
            try
            {
                yield return audio.SendWebRequest();
                if (audio.result != UnityWebRequest.Result.Success) { Error = "Couldn't decode the spoken reply."; DeleteFile(path); yield break; }
                clip = DownloadHandlerAudioClip.GetContent(audio);
                if (clip == null) { Error = "The spoken reply was empty."; yield break; }
                clips[key] = clip; clipOrder.Enqueue(key);
                while (clipOrder.Count > 8)
                {
                    string old = clipOrder.Dequeue();
                    if (clips.TryGetValue(old, out var cached)) { UnityEngine.Object.Destroy(cached); clips.Remove(old); }
                }
            }
            finally { if (request == audio) request = null; }
        }
        if (source == null) yield break;
        source.clip = clip;
        source.volume = JarvisSettings.Volume;
        source.Play();
        while (source != null && source.isPlaying) yield return null;
    }
    private IEnumerator DownloadSpeech(string text, string path)
    {
        string voice = JarvisSettings.Voices[JarvisSettings.Voice];
        string url = "https://lazypy.ro/tts/request_tts.php?service=Streamlabs&voice=" + UnityWebRequest.EscapeURL(voice) + "&text=" + UnityWebRequest.EscapeURL(text);
        using UnityWebRequest metadata = new(url, "POST") { downloadHandler = new DownloadHandlerBuffer(), timeout = 15 };
        request = metadata;
        try
        {
            yield return metadata.SendWebRequest();
            if (metadata.result != UnityWebRequest.Result.Success || metadata.downloadedBytes > 65536) yield break;
            string? audioUrl = null;
            try { audioUrl = (string?)JObject.Parse(metadata.downloadHandler.text)["audio_url"]; } catch (Newtonsoft.Json.JsonException) { }
            if (!Uri.TryCreate(audioUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https" || uri.IsLoopback || uri.HostNameType != UriHostNameType.Dns) yield break;
            using UnityWebRequest audio = UnityWebRequest.Get(uri.AbsoluteUri);
            request = audio; audio.timeout = 20;
            try
            {
                var operation = audio.SendWebRequest();
                while (!operation.isDone)
                {
                    if (audio.downloadedBytes > 8388608) { audio.Abort(); yield break; }
                    yield return null;
                }
                if (audio.result == UnityWebRequest.Result.Success && audio.downloadHandler.data.Length is > 0 and <= 8388608)
                {
                    try { File.WriteAllBytes(path, audio.downloadHandler.data); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
                }
            }
            finally { if (request == audio) request = null; }
        }
        finally { if (request == metadata) request = null; }
    }
    private bool StartWindowsVoice(string text, string path)
    {
        string text64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        string path64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(path));
        string script = "$ErrorActionPreference='Stop'; Add-Type -AssemblyName System.Speech; $s=New-Object System.Speech.Synthesis.SpeechSynthesizer; try { $s.SetOutputToWaveFile([Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('" + path64 + "'))); $s.Speak([Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('" + text64 + "'))) } finally { $s.Dispose() }";
        try
        {
            string executable = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe");
            synthesizer = Process.Start(new ProcessStartInfo(executable, "-NoLogo -NoProfile -NonInteractive -EncodedCommand " + Convert.ToBase64String(Encoding.Unicode.GetBytes(script))) { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden });
            return synthesizer != null;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or IOException) { Error = "Windows speech synthesis is unavailable."; return false; }
    }
    private void StopProcess()
    {
        if (synthesizer == null) return;
        try { if (!synthesizer.HasExited) synthesizer.Kill(); } catch (InvalidOperationException) { }
        synthesizer.Dispose(); synthesizer = null;
    }
    private void TrimCache()
    {
        var files = new DirectoryInfo(Folder).GetFiles().Where(f => f.Extension is ".wav" or ".mp3").OrderByDescending(f => f.LastWriteTimeUtc).ToArray();
        long bytes = 0;
        for (int i = 0; i < files.Length; i++) { bytes += files[i].Length; if (i >= 64 || bytes > 33554432) DeleteFile(files[i].FullName); }
    }
    private static void DeleteFile(string path) { try { File.Delete(path); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { } }
}
