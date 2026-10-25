// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries.Notifications;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ChudOS.Core.Managers;

public static class MultiMP3Manager
{
    private const string BaseURL = "https://multimp3.vercel.app/api/";
    private const long MaximumBytes = 128L * 1024 * 1024;
    private static readonly HttpClient Client = new() { Timeout = Timeout.InfiniteTimeSpan };
    private static readonly string[] Qualities = { "128", "192", "256", "320", "best" };
    private static CancellationTokenSource? cancellation;
    private static bool shuttingDown;

    public static bool Busy => cancellation != null;
    public static string Status { get; private set; } = "Paste a YouTube link.";
    public static void Initialize() => shuttingDown = false;
    public static void Cancel() => cancellation?.Cancel();
    public static void Shutdown() { shuttingDown = true; Cancel(); }

    public static async void Download(string input, int quality)
    {
        if (Busy || shuttingDown) return;
        if (!SupportedURL(input, out string url)) { SetStatus("Enter a supported HTTPS YouTube link.", true); return; }
        var operation = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        cancellation = operation;
        try
        {
            string path = await DownloadFile(Client, url, quality, FileManager.SoundFolder, operation.Token, message => Status = message);
            operation.Token.ThrowIfCancellationRequested();
            if (shuttingDown) return;
            SoundboardManager.RefreshDownloaded(path);
            SetStatus($"Added {Path.GetFileNameWithoutExtension(path)}.", true);
        }
        catch (OperationCanceledException) { if (!shuttingDown) SetStatus("Download cancelled or timed out.", true); }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ChudOS] MultiMP3: {ex.Message}");
            if (!shuttingDown) SetStatus($"Download failed: {ex.Message}", true);
        }
        finally
        {
            if (ReferenceEquals(cancellation, operation)) cancellation = null;
            operation.Dispose();
        }
    }

    internal static async Task<string> DownloadFile(HttpClient client, string input, int quality, string folder, CancellationToken token, Action<string> status)
    {
        if (!SupportedURL(input, out string url)) throw new ArgumentException("Unsupported YouTube link.");
        string temporary = "";
        try
        {
            status("Reading track information...");
            using var infoRequest = Request("info", new { urls = new[] { url } });
            using var infoResponse = await client.SendAsync(infoRequest, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
            await RequireSuccess(infoResponse, token).ConfigureAwait(false);
            string infoJSON = await ReadText(infoResponse.Content, token).ConfigureAwait(false);
            var item = JObject.Parse(infoJSON)["items"]?.FirstOrDefault();
            if (item == null) throw new InvalidDataException("The service returned no track information.");
            string title = SafeTitle(item["title"]?.Value<string>() ?? "YouTube Audio");

            Directory.CreateDirectory(folder);
            temporary = Path.Combine(folder, $"{Guid.NewGuid():N}.part");
            status($"Preparing {title}...");
            using var request = Request("download", new { url, title, quality = Qualities[Math.Clamp(quality, 0, Qualities.Length - 1)] });
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
            await RequireSuccess(response, token).ConfigureAwait(false);
            if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "audio/mpeg", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The service did not return an MP3.");
            long? expected = response.Content.Headers.ContentLength;
            if (expected > MaximumBytes) throw new InvalidDataException("The download exceeds the size limit.");

            using (var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, true))
            {
                byte[] buffer = new byte[65536];
                long received = 0;
                int read;
                while ((read = await source.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0)
                {
                    received += read;
                    if (received > MaximumBytes) throw new InvalidDataException("The download exceeds the size limit.");
                    await output.WriteAsync(buffer, 0, read, token).ConfigureAwait(false);
                    status(expected > 0 ? $"Downloading: {received * 100 / expected.Value}%" : $"Downloading: {received / 1024} KB");
                }
                if (received == 0) throw new InvalidDataException("The download was empty.");
                if (expected.HasValue && received != expected.Value) throw new InvalidDataException("The download was incomplete.");
            }
            token.ThrowIfCancellationRequested();
            if (!LooksLikeMP3(temporary)) throw new InvalidDataException("The response does not contain recognised MP3 data.");
            string destination = UniquePath(folder, title);
            File.Move(temporary, destination);
            temporary = "";
            return destination;
        }
        finally
        {
            if (temporary.Length > 0)
            {
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }

    private static HttpRequestMessage Request(string endpoint, object body) => new(HttpMethod.Post, BaseURL + endpoint)
    {
        Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
    };

    private static async Task RequireSuccess(HttpResponseMessage response, CancellationToken token)
    {
        if (response.IsSuccessStatusCode) return;
        if ((int)response.StatusCode == 429) throw new HttpRequestException("Too many requests. Try again later.");
        string message = $"Service error {(int)response.StatusCode}.";
        string body = await ReadText(response.Content, token).ConfigureAwait(false);
        try
        {
            string? error = JObject.Parse(body)["error"]?.Value<string>();
            if (!string.IsNullOrWhiteSpace(error)) message = new string(error!.Where(c => !char.IsControl(c) && c != '<' && c != '>').Take(180).ToArray());
        }
        catch (JsonException) { }
        throw new HttpRequestException(message);
    }

    private static async Task<string> ReadText(HttpContent content, CancellationToken token)
    {
        using var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
        using var output = new MemoryStream();
        byte[] buffer = new byte[4096];
        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0)
        {
            if (output.Length + read > 65536) throw new InvalidDataException("The service returned an oversized response.");
            output.Write(buffer, 0, read);
        }
        token.ThrowIfCancellationRequested();
        return Encoding.UTF8.GetString(output.ToArray());
    }

    internal static bool SupportedURL(string? input, out string url)
    {
        url = "";
        if (input == null || input.Length > 2048 || !Uri.TryCreate(input.Trim(), UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme != Uri.UriSchemeHttps || uri.UserInfo.Length > 0 || !uri.IsDefaultPort) return false;
        string host = uri.Host.ToLowerInvariant();
        if (host != "youtube.com" && host != "www.youtube.com" && host != "m.youtube.com" && host != "music.youtube.com" && host != "youtu.be") return false;
        url = uri.AbsoluteUri;
        return true;
    }

    private static string SafeTitle(string title)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        string name = new(title.Select(c => invalid.Contains(c) || char.IsControl(c) ? '_' : c).ToArray());
        name = name.Trim().TrimEnd('.');
        if (name.Length == 0) name = "YouTube Audio";
        if (name.Length > 90) name = name.Substring(0, 90).TrimEnd(' ', '.');
        return "YT - " + name;
    }
    private static string UniquePath(string folder, string title)
    {
        string path = Path.Combine(folder, title + ".mp3");
        int suffix = 2;
        while (File.Exists(path)) path = Path.Combine(folder, $"{title} ({suffix++}).mp3");
        return path;
    }
    private static bool LooksLikeMP3(string path)
    {
        using var file = File.OpenRead(path);
        byte[] header = new byte[3];
        if (file.Read(header, 0, header.Length) < header.Length) return false;
        return header[0] == 'I' && header[1] == 'D' && header[2] == '3' || header[0] == 0xFF && (header[1] & 0xE0) == 0xE0;
    }
    private static void SetStatus(string message, bool notify = false)
    {
        Status = message;
        if (notify) NotifyLib.SendNotificationALL(message, 4000);
    }
}
