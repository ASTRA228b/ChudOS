// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Collections.Concurrent;
using UnityEngine.Windows.Speech;

namespace ChudOS.Core.Managers;

internal sealed class JarvisRecognition : IJarvisCapture
{
    private readonly DictationRecognizer recognizer;
    private readonly ConcurrentQueue<(int Kind, string Text, int Value)> events = new();
    private bool disposed;
    public string Backend => "Windows dictation";
    public string FinalText { get; private set; } = "";
    public string PartialText { get; private set; } = "";
    public string Error { get; private set; } = "";
    public ConfidenceLevel Confidence { get; private set; } = ConfidenceLevel.Rejected;
    public DictationCompletionCause? Completion { get; private set; }
    public int Revision { get; private set; }
    public SpeechSystemStatus Status => recognizer.Status;
    public bool SilenceTimeout => Completion == DictationCompletionCause.TimeoutExceeded && FinalText.Length == 0 && PartialText.Length == 0 && Error.Length == 0;

    internal JarvisRecognition(float listenSeconds, float silenceSeconds)
    {
        // Request all final results. Apply the user's confidence threshold after receiving them.
        recognizer = new DictationRecognizer(ConfidenceLevel.Low, DictationTopicConstraint.Dictation)
        {
            InitialSilenceTimeoutSeconds = listenSeconds,
            AutoSilenceTimeoutSeconds = silenceSeconds
        };
        recognizer.DictationResult += OnResult;
        recognizer.DictationHypothesis += OnHypothesis;
        recognizer.DictationComplete += OnComplete;
        recognizer.DictationError += OnError;
    }
    public void Start() => recognizer.Start();
    public void Stop() { if (!disposed && recognizer.Status == SpeechSystemStatus.Running) recognizer.Stop(); }
    private void OnResult(string text, ConfidenceLevel confidence) => events.Enqueue((0, text, (int)confidence));
    private void OnHypothesis(string text) => events.Enqueue((1, text, 0));
    private void OnComplete(DictationCompletionCause cause) => events.Enqueue((2, "", (int)cause));
    private void OnError(string error, int hresult) => events.Enqueue((3, error, hresult));
    public void Poll()
    {
        while (events.TryDequeue(out var item))
        {
            Revision++;
            if (item.Kind == 0 && FinalText.Length == 0 && !string.IsNullOrWhiteSpace(item.Text))
            {
                FinalText = item.Text.Trim();
                Confidence = (ConfidenceLevel)item.Value;
            }
            else if (item.Kind == 1) PartialText = item.Text;
            else if (item.Kind == 2) Completion = (DictationCompletionCause)item.Value;
            else if (item.Kind == 3) Error = item.Text + " (0x" + unchecked((uint)item.Value).ToString("X8") + ")";
        }
    }
    public bool Accepts(int threshold) => FinalText.Length > 0 && (Confidence == ConfidenceLevel.High || threshold != 1 && Confidence == ConfidenceLevel.Medium || threshold == 2 && Confidence == ConfidenceLevel.Low);
    public string Failure()
    {
        if (Error.Length > 0) return "Windows dictation reported an error. Check the Speech diagnostic in the Jarvis window and Windows speech settings.";
        return Completion switch
        {
            DictationCompletionCause.MicrophoneUnavailable => "Windows couldn't access its default microphone. Choose your microphone in Windows Sound Settings and allow microphone access for desktop apps.",
            DictationCompletionCause.NetworkFailure => "The Windows dictation service lost its connection. Check your internet connection and Online Speech Recognition.",
            DictationCompletionCause.AudioQualityFailure => "Windows reported poor microphone audio. Check the default input device and microphone level in Sound Settings.",
            DictationCompletionCause.UnknownError => "Windows dictation stopped with an unknown error. Restart Jarvis or check Windows speech settings.",
            _ => PartialText.Length > 0 ? "Windows heard speech but didn't deliver a final transcription. Please repeat after Listening appears in the Jarvis window." : "No speech was transcribed. Wait for Listening in the Jarvis window, then speak. Check the default Windows microphone if this keeps happening."
        };
    }
    public void Dispose()
    {
        if (disposed) return;
        Stop();
        disposed = true;
        recognizer.DictationResult -= OnResult;
        recognizer.DictationHypothesis -= OnHypothesis;
        recognizer.DictationComplete -= OnComplete;
        recognizer.DictationError -= OnError;
        recognizer.Dispose();
        while (events.TryDequeue(out _)) { }
    }
}
