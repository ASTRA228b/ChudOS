// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.IO;
using System.Text;

namespace ChudOS.Core.Managers;

internal static class AudioMetadata
{
    private static readonly Encoding Latin1 = Encoding.GetEncoding(28591);
    internal static (string Title, string Artist) Read(string path, string fallback)
    {
        string title = fallback, artist = "";
        if (!File.Exists(path) || !path.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)) return (title, artist);
        try
        {
            using FileStream stream = File.OpenRead(path);
            using BinaryReader reader = new(stream);
            byte[] header = reader.ReadBytes(10);
            if (header.Length == 10 && Encoding.ASCII.GetString(header, 0, 3) == "ID3" && (header[3] == 3 || header[3] == 4) && (header[5] & 0xC0) == 0)
            {
                int size = Sync(header, 6);
                long end = Math.Min(stream.Length, 10L + Math.Min(size, 1024 * 1024));
                while (stream.Position + 10 <= end)
                {
                    byte[] frame = reader.ReadBytes(10);
                    string id = Encoding.ASCII.GetString(frame, 0, 4);
                    int length = header[3] == 4 ? Sync(frame, 4) : (frame[4] << 24) | (frame[5] << 16) | (frame[6] << 8) | frame[7];
                    if (length <= 0 || length > end - stream.Position) break;
                    if ((id == "TIT2" || id == "TPE1") && frame[9] == 0 && length <= 65536)
                    {
                        string text = Decode(reader.ReadBytes(length));
                        if (!string.IsNullOrWhiteSpace(text)) { if (id == "TIT2") title = text; else artist = text; }
                    }
                    else stream.Seek(length, SeekOrigin.Current);
                }
            }
            if (stream.Length >= 128 && (title == fallback || artist.Length == 0))
            {
                stream.Position = stream.Length - 128;
                byte[] tag = reader.ReadBytes(128);
                if (Encoding.ASCII.GetString(tag, 0, 3) == "TAG")
                {
                    string oldTitle = Latin1.GetString(tag, 3, 30).Trim('\0', ' ');
                    string oldArtist = Latin1.GetString(tag, 33, 30).Trim('\0', ' ');
                    if (title == fallback && oldTitle.Length > 0) title = oldTitle;
                    if (artist.Length == 0) artist = oldArtist;
                }
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        catch (ArgumentException) { }
        return (title, artist);
    }

    private static int Sync(byte[] bytes, int offset) => ((bytes[offset] & 127) << 21) | ((bytes[offset + 1] & 127) << 14) | ((bytes[offset + 2] & 127) << 7) | (bytes[offset + 3] & 127);
    private static string Decode(byte[] data)
    {
        if (data.Length < 2) return "";
        Encoding encoding = data[0] switch { 1 => data.Length > 2 && data[1] == 0xFE ? Encoding.BigEndianUnicode : Encoding.Unicode, 2 => Encoding.BigEndianUnicode, 3 => Encoding.UTF8, _ => Latin1 };
        int offset = data[0] == 1 && data.Length > 2 && (data[1] == 0xFF && data[2] == 0xFE || data[1] == 0xFE && data[2] == 0xFF) ? 3 : 1;
        return encoding.GetString(data, offset, data.Length - offset).Trim('\0', ' ', '\uFEFF').Split('\0')[0];
    }
}
