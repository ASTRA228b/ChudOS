// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using ChudOS.Stuff;
using UnityEngine.Windows.Speech;

namespace ChudOS.Core.Managers;

internal sealed class JarvisLocalRecognition : IJarvisCapture
{
    private static string[]? vocabulary;
    private readonly KeywordRecognizer recognizer;
    private readonly ConcurrentQueue<(string Text, ConfidenceLevel Confidence)> events = new();
    private bool disposed;
    public string Backend { get; }
    public string FinalText { get; private set; } = "";
    public string PartialText => "";
    public string Error => "";
    public ConfidenceLevel Confidence { get; private set; } = ConfidenceLevel.Rejected;
    public DictationCompletionCause? Completion { get; private set; }
    public int Revision { get; private set; }
    public SpeechSystemStatus Status => !disposed && recognizer.IsRunning ? SpeechSystemStatus.Running : SpeechSystemStatus.Stopped;
    public bool SilenceTimeout => false;

    internal JarvisLocalRecognition()
    {
        vocabulary ??= BuildVocabulary();
        recognizer = new KeywordRecognizer(vocabulary, ConfidenceLevel.Low);
        recognizer.OnPhraseRecognized += Recognized;
        Backend = "Local commands (" + vocabulary.Length + " phrases)";
    }
    internal static string[] BuildVocabulary()
    {
        var phrases = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "cancel", "never mind" };
        void Add(string text)
        {
            text = Regex.Replace(text, @"[^\p{L}\p{N}\s.%/-]", " ");
            text = Regex.Replace(text, @"\s+", " ").Trim();
            if (text.Length is < 2 or > 180) return;
            if (JarvisCommands.TryParse(text, out _, out string problem) && problem.Length == 0) phrases.Add(text);
        }
        foreach (string special in new[] { "pause music", "resume music", "next song", "previous song", "save menu state", "save settings", "leave room", "disconnect", "join room ASTRA", "join room CHUDOS" }) Add(special);
        foreach (string category in ButtonManager.GetCategories().Concat(new[] { "Home", "Enabled Mods", "Favorites", "Detected" }).Distinct(StringComparer.OrdinalIgnoreCase)) Add("open " + category);
        foreach (var button in ButtonManager.Buttons)
        {
            foreach (string name in JarvisCommands.Names(button))
            {
                if (button.Type == ButtonType.Toggle)
                {
                    Add("enable " + name); Add("disable " + name); Add("toggle " + name);
                    Add("turn on " + name); Add("turn off " + name);
                }
                else if (button.Type == ButtonType.Press) Add("press " + name);
                else
                {
                    Add("increase " + name); Add("decrease " + name);
                    foreach (string option in button.Attribute.Options) Add("set " + name + " to " + option);
                }
            }
        }
        return phrases.OrderBy(text => text, StringComparer.OrdinalIgnoreCase).ToArray();
    }
    public void Start() => recognizer.Start();
    public void Stop()
    {
        if (disposed) return;
        if (recognizer.IsRunning) recognizer.Stop();
        Completion ??= FinalText.Length > 0 ? DictationCompletionCause.Complete : DictationCompletionCause.Canceled;
    }
    private void Recognized(PhraseRecognizedEventArgs result) => events.Enqueue((result.text, result.confidence));
    public void Poll()
    {
        while (events.TryDequeue(out var item))
        {
            if (FinalText.Length > 0 || string.IsNullOrWhiteSpace(item.Text)) continue;
            FinalText = item.Text; Confidence = item.Confidence; Revision++;
        }
    }
    public bool Accepts(int threshold) => FinalText.Length > 0 && (Confidence == ConfidenceLevel.High || threshold != 1 && Confidence == ConfidenceLevel.Medium || threshold == 2 && Confidence == ConfidenceLevel.Low);
    public string Failure() => "No matching local command was recognized. Try enable platforms, open movement, or set menu theme to OG Blue.";
    public void Dispose()
    {
        if (disposed) return;
        Stop(); disposed = true; recognizer.OnPhraseRecognized -= Recognized; recognizer.Dispose();
        while (events.TryDequeue(out _)) { }
    }
}
