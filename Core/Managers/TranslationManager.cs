// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Collections;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ChudOS.Stuff;
using UnityEngine;
using UnityEngine.Networking;

namespace ChudOS.Core.Managers;

public static class TranslationManager
{
    private static readonly string[] Codes = { "en", "es", "fr", "de", "pt", "it" };
    private static readonly Dictionary<string, Dictionary<string, string>> Dictionaries = new();
    private static readonly Dictionary<string, Dictionary<string, string>> Custom = new();
    private static readonly Queue<(string Language, string Text)> Pending = new();
    private static readonly HashSet<(string Language, string Text)> Waiting = new();
    private static readonly Dictionary<(string Language, string Text), float> RetryAfter = new();
    private static MonoBehaviour? host;
    private static Coroutine? worker;
    private static UnityWebRequest? activeRequest;
    private static bool translateBoards;
    public static string Language { get; private set; } = "en";
    public static event Action? Changed;

    public static void Initialize(MonoBehaviour runtime) => host = runtime;

    [MenuButton("Menu Language", ButtonType.Option, Category = "Settings", Order = 43, Options = new[] { "English", "Español", "Français", "Deutsch", "Português", "Italiano" }, Notify = false)]
    public static void SetLanguage()
    {
        string code = Codes[Mathf.Clamp(ButtonManager.GetButton("Menu Language")?.OptionIndex ?? 0, 0, Codes.Length - 1)];
        if (Language == code) return;
        CancelRequests();
        Load(code);
        Language = code;
        Changed?.Invoke();
    }

    [MenuButton("Auto Translate Boards", ButtonType.Toggle, Category = "Settings", Order = 44, runOption = RunOption.Once, Notify = false, Description = "Translate custom board text using Google; sends board text to the translation service")]
    public static void EnableBoardTranslation()
    {
        translateBoards = true;
        ChudOS.Libraries.Notifications.NotifyLib.SendNotificationALL("Auto Translate Boards sends custom board text to Google for translation.", 6000);
        Changed?.Invoke();
    }
    [MenuDisable("Auto Translate Boards")]
    public static void DisableBoardTranslation() { translateBoards = false; CancelRequests(); Changed?.Invoke(); }

    private static void Load(string code)
    {
        if (Dictionaries.ContainsKey(code)) return;
        Dictionary<string, string> entries = new(StringComparer.Ordinal);
        try
        {
            using Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("ChudOS.Languages." + code + ".json");
            if (resource != null)
            {
                using StreamReader reader = new(resource);
                entries = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd()) ?? entries;
            }
        }
        catch (Exception ex) { Debug.LogWarning("[ChudOS] Language dictionary: " + ex.Message); }
        Dictionaries[code] = entries;
        Dictionary<string, string> custom = new(StringComparer.Ordinal);
        try
        {
            string path = CachePath(code);
            if (File.Exists(path)) custom = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path)) ?? custom;
        }
        catch (Exception ex) { Debug.LogWarning("[ChudOS] Translation cache: " + ex.Message); }
        Custom[code] = custom;
    }

    public static string T(string text)
    {
        if (Language == "en" || string.IsNullOrEmpty(text)) return text;
        return Dictionaries.TryGetValue(Language, out var entries) && entries.TryGetValue(text, out string translated) ? translated : text;
    }

    public static string UI(string text)
    {
        string translated = T(text);
        if (translated != text || Language == "en") return translated;
        if (text.StartsWith("[Playing] ", StringComparison.Ordinal)) return "[" + T("Playing") + "] " + text.Substring(10);
        if (text.StartsWith("[Selected] ", StringComparison.Ordinal)) return "[" + T("Selected") + "] " + text.Substring(11);
        int split = text.IndexOf(':');
        if (split > 0)
        {
            string prefix = text.Substring(0, split);
            string suffix = text.Substring(split + 1).TrimStart();
            if (prefix == "Selected" || prefix == "Playing") return T(prefix) + ": " + (suffix == "No Sounds" || suffix == "None" ? T(suffix) : suffix);
            return T(prefix) + ": " + T(suffix);
        }
        return text;
    }

    public static string Board(string original)
    {
        if (Language == "en" || string.IsNullOrEmpty(original)) return original;
        string code = Language;
        Load(code);
        string[] lines = original.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd('\r');
            if (line.Length == 0) continue;
            string template = line.Replace(Constantss.Version, "{version}");
            string translated = UI(template);
            if (translated != template) { lines[i] = translated.Replace("{version}", Constantss.Version); continue; }
            if (!translateBoards || line.Length > 2000) continue;
            if (Custom[code].TryGetValue(line, out string cached)) { lines[i] = cached; continue; }
            if (RetryAfter.TryGetValue((code, line), out float retry) && Time.unscaledTime < retry) continue;
            if (host != null && Pending.Count < 64 && Waiting.Add((code, line))) Pending.Enqueue((code, line));
        }
        if (worker == null && host != null && Pending.Count > 0) worker = host.StartCoroutine(Process());
        return string.Join("\n", lines);
    }

    private static IEnumerator Process()
    {
        while (translateBoards && Pending.Count > 0)
        {
            var item = Pending.Dequeue();
            string url = "https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=" + item.Language + "&dt=t&q=" + UnityWebRequest.EscapeURL(item.Text);
            string? translation = null;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                activeRequest = request;
                request.timeout = 10;
                yield return request.SendWebRequest();
                activeRequest = null;
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        JArray result = JArray.Parse(request.downloadHandler.text);
                        translation = string.Concat(result[0]!.Select(part => (string?)part[0] ?? ""));
                    }
                    catch (Exception ex) { Debug.LogWarning("[ChudOS] Translation response: " + ex.Message); }
                }
            }
            Waiting.Remove(item);
            if (string.IsNullOrWhiteSpace(translation)) RetryAfter[item] = Time.unscaledTime + 60f;
            else
            {
                Custom[item.Language][item.Text] = translation!;
                RetryAfter.Remove(item);
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(CachePath(item.Language))!);
                    File.WriteAllText(CachePath(item.Language), JsonConvert.SerializeObject(Custom[item.Language], Formatting.Indented));
                }
                catch (Exception ex) { Debug.LogWarning("[ChudOS] Save translation: " + ex.Message); }
                if (Language == item.Language && translateBoards) Changed?.Invoke();
            }
            yield return new WaitForSecondsRealtime(0.3f);
        }
        worker = null;
    }

    private static string CachePath(string code) => Path.Combine(FileManager.RootFolder, "Translations", code + ".json");
    private static void CancelRequests()
    {
        activeRequest?.Abort();
        if (host != null && worker != null) host.StopCoroutine(worker);
        activeRequest?.Dispose();
        activeRequest = null;
        worker = null;
        Pending.Clear();
        Waiting.Clear();
    }
    public static void Shutdown()
    {
        CancelRequests();
        host = null;
        RetryAfter.Clear();
    }
}
