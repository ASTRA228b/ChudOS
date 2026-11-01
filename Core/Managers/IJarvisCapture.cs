// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using UnityEngine.Windows.Speech;

namespace ChudOS.Core.Managers;

internal interface IJarvisCapture : IDisposable
{
    string Backend { get; }
    string FinalText { get; }
    string PartialText { get; }
    string Error { get; }
    ConfidenceLevel Confidence { get; }
    DictationCompletionCause? Completion { get; }
    int Revision { get; }
    SpeechSystemStatus Status { get; }
    bool SilenceTimeout { get; }
    void Start();
    void Stop();
    void Poll();
    bool Accepts(int threshold);
    string Failure();
}
