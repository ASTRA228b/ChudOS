// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using Photon.Voice;

namespace ChudOS.Core.Managers;

internal sealed class SoundVoiceMixer : IProcessor<float>, IProcessor<short>
{
    private sealed class Playback
    {
        internal readonly float[] Samples;
        internal readonly int Channels, Frames;
        internal readonly double Step;
        internal double Position;
        internal Playback(float[] samples, int channels, double step, int start)
        {
            Samples = samples;
            Channels = channels;
            Frames = samples.Length / channels;
            Step = step;
            Position = Math.Max(0, Math.Min(start, Frames));
        }
    }

    private readonly int channels, rate;
    private volatile Playback? playback;
    internal volatile float Gain = 1f;
    internal volatile bool Loop;
    internal bool Active => playback != null;

    internal SoundVoiceMixer(int samplingRate, int channels)
    {
        if (samplingRate <= 0 || channels <= 0) throw new ArgumentOutOfRangeException(nameof(channels));
        rate = samplingRate;
        this.channels = channels;
    }

    internal void Play(float[] samples, int sourceRate, int sourceChannels, int startFrame, float gain, bool loop)
    {
        if (samples == null || sourceRate <= 0 || sourceChannels <= 0 || samples.Length % sourceChannels != 0) throw new ArgumentException("Invalid soundboard samples.");
        Gain = gain;
        Loop = loop;
        playback = new Playback(samples, sourceChannels, (double)sourceRate / rate, startFrame);
    }

    private float Sample(Playback state, int channel)
    {
        if (state.Frames == 0 || state.Position >= state.Frames) return 0f;
        int frame = (int)state.Position;
        int next = frame + 1 < state.Frames ? frame + 1 : Loop ? 0 : frame;
        float fraction = (float)(state.Position - frame);
        if (channels == 1 && state.Channels > 1)
        {
            float sum = 0f;
            for (int c = 0; c < state.Channels; c++)
            {
                float a = state.Samples[frame * state.Channels + c];
                float b = state.Samples[next * state.Channels + c];
                sum += a + (b - a) * fraction;
            }
            return Finite(sum / state.Channels);
        }
        int sourceChannel = state.Channels == 1 ? 0 : Math.Min(channel, state.Channels - 1);
        float first = state.Samples[frame * state.Channels + sourceChannel];
        float second = state.Samples[next * state.Channels + sourceChannel];
        return Finite(first + (second - first) * fraction);
    }

    private void Advance(Playback state)
    {
        state.Position += state.Step;
        if (Loop && state.Frames > 0 && state.Position >= state.Frames) state.Position %= state.Frames;
    }

    public float[] Process(float[] buffer)
    {
        Playback? state = playback;
        if (state == null) return buffer;
        float gain = Math.Max(0f, Math.Min(1.1f, Finite(Gain)));
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = Limit(Finite(buffer[i]) + Sample(state, i % channels) * gain);
            if ((i + 1) % channels == 0) Advance(state);
        }
        return buffer;
    }

    public short[] Process(short[] buffer)
    {
        Playback? state = playback;
        if (state == null) return buffer;
        float gain = Math.Max(0f, Math.Min(1.1f, Finite(Gain)));
        for (int i = 0; i < buffer.Length; i++)
        {
            float value = Limit(buffer[i] / 32768f + Sample(state, i % channels) * gain);
            buffer[i] = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, (int)(value * 32768f)));
            if ((i + 1) % channels == 0) Advance(state);
        }
        return buffer;
    }

    private static float Finite(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
    private static float Limit(float value) => Math.Max(-1f, Math.Min(1f, Finite(value)));
    internal void Stop() => playback = null;
    public void Dispose() => Stop();
}
