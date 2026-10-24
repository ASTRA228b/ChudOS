// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using Photon.Voice;

namespace ChudOS.Core.Managers;

internal sealed class SoundPcmReader : IAudioReader<float>
{
    private readonly float[] samples;
    private int position;
    private volatile bool disposed;
    internal volatile float Gain;
    internal volatile bool Loop;
    public int SamplingRate { get; }
    public int Channels { get; }
    public string Error => "";

    internal SoundPcmReader(float[] data, int rate, int channels, int startFrame, float gain, bool loop)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (rate <= 0 || channels <= 0 || data.Length % channels != 0) throw new ArgumentOutOfRangeException(nameof(channels));
        samples = data;
        SamplingRate = rate;
        Channels = channels;
        position = Math.Max(0, Math.Min(startFrame, data.Length / channels)) * channels;
        Gain = gain;
        Loop = loop;
    }

    public bool Read(float[] buffer)
    {
        float gain = Gain;
        if (float.IsNaN(gain) || float.IsInfinity(gain)) gain = 0f;
        gain = Math.Max(0f, Math.Min(1f, gain));
        for (int i = 0; i < buffer.Length; i++)
        {
            if (disposed || samples.Length == 0) { buffer[i] = 0f; continue; }
            if (position >= samples.Length)
            {
                if (Loop) position = 0;
                else { buffer[i] = 0f; continue; }
            }
            float sample = samples[position++] * gain;
            buffer[i] = float.IsNaN(sample) || float.IsInfinity(sample) ? 0f : Math.Max(-1f, Math.Min(1f, sample));
        }
        return true;
    }

    public void Dispose() => disposed = true;
}
