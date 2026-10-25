// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Text;

namespace ChudOS.Core.Managers;

internal static class MetadataText
{
    internal static readonly Encoding UTF8 = new UTF8Encoding(false, true);
    internal static readonly Encoding UTF16 = new UnicodeEncoding(false, false, true);
    internal static readonly Encoding UTF16BE = new UnicodeEncoding(true, false, true);
    private static readonly Encoding Latin1 = Encoding.GetEncoding(28591, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
    private static readonly Encoding? Windows1252 = GetWindows1252();

    private static Encoding? GetWindows1252()
    {
        try { return Encoding.GetEncoding(1252, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback); }
        catch (ArgumentException) { return null; }
        catch (NotSupportedException) { return null; }
    }
    internal static string Clean(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        int end = value.IndexOf('\0');
        if (end >= 0) value = value.Substring(0, end);
        value = value.Trim('\uFEFF', ' ', '\r', '\n', '\t');
        for (int pass = 0; pass < 2; pass++)
        {
            int score = Suspicion(value);
            if (score == 0) break;
            string repaired = Repair(value, Latin1, score);
            if (repaired == value && Windows1252 != null) repaired = Repair(value, Windows1252, score);
            if (repaired == value) break;
            value = repaired;
        }
        return value;
    }
    private static string Repair(string value, Encoding source, int originalScore)
    {
        try
        {
            string candidate = UTF8.GetString(source.GetBytes(value));
            if (candidate == value || Suspicion(candidate) >= originalScore || candidate.IndexOf('\uFFFD') >= 0) return value;
            foreach (char character in candidate) if (char.IsControl(character)) return value;
            return source.GetString(UTF8.GetBytes(candidate)) == value ? candidate : value;
        }
        catch (EncoderFallbackException) { return value; }
        catch (DecoderFallbackException) { return value; }
    }
    private static int Suspicion(string value)
    {
        int score = 0;
        for (int i = 0; i + 1 < value.Length; i++)
            if ((value[i] == 'Ã' || value[i] == 'Â' || value[i] == 'â' || value[i] == 'ð') && value[i + 1] >= '\u0080') score++;
        return score;
    }
}
