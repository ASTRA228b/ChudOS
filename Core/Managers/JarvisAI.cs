// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;

namespace ChudOS.Core.Managers;

internal sealed class JarvisAI : IDisposable
{
    internal static readonly string[] ModelIds = { "public", "music", "buggy", "700", "1300", "1500", "1600", "ultimate", "plus", "pro", "code", "mega", "plato", "plato-mini" };
    private readonly Dictionary<string, string> sessions = new();
    private readonly Dictionary<string, List<JObject>> histories = new();
    private UnityWebRequest? request;
    private static readonly Regex CommandPattern = new(@"^\s*<\s*\[\s*(toggle|enable|disable|increment|decrement|press)\s*\]\s*([a-z0-9]+)\s*>\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    internal static string Endpoint(int model)
    {
        string id = ModelIds[model];
        string root = model < 2 ? "https://chudgpt-public.vercel.app/api/models/" : model < 12 ? "https://chudgpt-xi.vercel.app/api/models/" : "https://chudgpt-xi.vercel.app/api/plato/models/";
        return root + id + "/chat";
    }
    private string Session(string id)
    {
        if (!sessions.TryGetValue(id, out string session)) sessions[id] = session = "chudos_" + Guid.NewGuid().ToString("N");
        return session;
    }
    internal void Cancel() => request?.Abort();
    public void Dispose() { request?.Abort(); request?.Dispose(); request = null; }

    internal IEnumerator Clear(Action<string> done)
    {
        if (JarvisSettings.Provider != 2) { done("GPT compatibility requests are stateless. Local conversation cleared."); yield break; }
        int model = JarvisSettings.Model;
        string id = ModelIds[model];
        histories.Remove(id);
        if (model >= 12) { done("Plato conversation cleared."); yield break; }
        string session = Session(id);
        string? error = null;
        yield return Post(Endpoint(model).Replace("/chat", "/clear"), new JObject { ["session_id"] = session }, (_, e) => error = e);
        if (error == null) { sessions.Remove(id); done(id + " conversation cleared."); }
        else done("Couldn't clear the server conversation. " + error);
    }
    internal IEnumerator Ask(string input, Action<string> done)
    {
        int provider = JarvisSettings.Provider, model = JarvisSettings.Model;
        var candidates = provider == 1 ? Candidates(input) : Enumerable.Empty<ButtonManager.ButtonData>();
        var byId = new Dictionary<string, ButtonManager.ButtonData>(StringComparer.OrdinalIgnoreCase);
        JArray menu = new();
        int index = 0;
        foreach (var b in candidates)
        {
            string id = "b" + index++;
            byId[id] = b;
            menu.Add(new JObject { ["id"] = id, ["name"] = b.Name, ["category"] = b.Category, ["description"] = b.Attribute.Description.Length > 100 ? b.Attribute.Description.Substring(0, 100) : b.Attribute.Description,
                ["type"] = b.Type == ButtonType.Toggle ? "toggleable" : b.Type == ButtonType.Option ? "incremental" : "fixed", ["state"] = b.Type == ButtonType.Toggle ? b.Enabled ? "enabled" : "disabled" : b.Type == ButtonType.Option ? b.Option : "" });
        }
        JObject body;
        string endpoint;
        string idModel = ModelIds[model];
        List<JObject>? history = null;
        if (provider == 1)
        {
            endpoint = "https://chat.hamburbur.org/api/chat";
            body = new JObject { ["message"] = input, ["menu"] = menu };
        }
        else
        {
            endpoint = Endpoint(model);
            if (model < 12) body = new JObject { ["message"] = input, ["session_id"] = Session(idModel) };
            else
            {
                if (!histories.TryGetValue(idModel, out history)) histories[idModel] = history = new();
                while (history.Count > 10 || history.Sum(m => ((string?)m["content"] ?? "").Length) + input.Length > 4000)
                {
                    if (history.Count == 0) break;
                    history.RemoveRange(0, Math.Min(2, history.Count));
                }
                JArray messages = new(history.Select(m => m.DeepClone()));
                messages.Add(new JObject { ["role"] = "user", ["content"] = input });
                body = new JObject { ["messages"] = messages, ["max_new_tokens"] = 128 };
            }
        }
        JObject? response = null; string? error = null;
        yield return Post(endpoint, body, (data, e) => { response = data; error = e; });
        if (error != null) { done(provider == 2 ? "ChudGPT couldn't answer. " + error + " Switch Jarvis Provider to GPT Jarvis to try that service instead." : error); yield break; }
        string? reply = (string?)response?[provider == 1 ? "response" : "reply"];
        if (string.IsNullOrWhiteSpace(reply) || reply.Length > 16000) { done("The service returned an empty or oversized reply." + (provider == 2 ? " Try switching to GPT Jarvis." : "")); yield break; }
        if (provider == 2 && model >= 12 && ((string?)response?["model"] != idModel || (string?)response?["source"] != "neural")) { done("The service returned a different model or an invalid Plato response."); yield break; }
        if (history != null)
        {
            history.Add(new JObject { ["role"] = "user", ["content"] = input });
            history.Add(new JObject { ["role"] = "assistant", ["content"] = reply.Length > 1000 ? reply.Substring(0, 1000) : reply });
        }
        if (provider == 2) { done(reply.Trim()); yield break; }
        Match command = CommandPattern.Match(reply);
        if (command.Success)
        {
            if (reply.Substring(command.Length).Contains("<[")) { done("The AI returned multiple commands. Nothing changed. Ask for one action at a time."); yield break; }
            if (!byId.TryGetValue(command.Groups[2].Value, out var target)) { done("The AI selected an unknown menu button. Nothing changed."); yield break; }
            string action = command.Groups[1].Value.ToLowerInvariant();
            bool valid = action is "enable" or "disable" or "toggle" ? target.Type == ButtonType.Toggle : action is "increment" or "decrement" ? target.Type == ButtonType.Option : target.Type == ButtonType.Press;
            if (!valid) { done("The AI selected an invalid action for that button. Nothing changed."); yield break; }
            JarvisCommands.Execute(new(action, target.Name), out string outcome);
            // Speak the actual local result, not an unverified AI claim that an action succeeded.
            done(outcome);
            yield break;
        }
        if (reply.Contains("<[") || Regex.IsMatch(reply, @"<\s*(enable|disable|toggle|press)\s*>", RegexOptions.IgnoreCase)) { done("The AI returned an unrecognized command. Nothing changed."); yield break; }
        done(reply.Trim());
    }
    private static IEnumerable<ButtonManager.ButtonData> Candidates(string input)
    {
        string[] words = Regex.Split(input.ToLowerInvariant(), @"\W+").Where(w => w.Length > 2 && w is not "the" and not "please" and not "jarvis").ToArray();
        return ButtonManager.Buttons.Select(b => (Button: b, Score: words.Count(w => (b.Name + " " + b.Category).IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0)))
            .Where(x => x.Score > 0).OrderByDescending(x => x.Score).ThenBy(x => x.Button.Name, StringComparer.OrdinalIgnoreCase).Take(6).Select(x => x.Button);
    }
    private IEnumerator Post(string url, JObject body, Action<JObject?, string?> done)
    {
        using UnityWebRequest active = new(url, "POST");
        request = active;
        active.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body.ToString(Formatting.None)));
        active.downloadHandler = new DownloadHandlerBuffer();
        active.timeout = 90;
        active.SetRequestHeader("Content-Type", "application/json");
        try
        {
            var operation = active.SendWebRequest();
            while (!operation.isDone)
            {
                if (active.downloadedBytes > 262144) { active.Abort(); done(null, "The service response was too large."); yield break; }
                yield return null;
            }
            if (active.result != UnityWebRequest.Result.Success) { done(null, active.responseCode == 429 ? "The selected service is busy. Try again shortly." : "The selected service is unavailable (HTTP " + active.responseCode + ")."); yield break; }
            string type = active.GetResponseHeader("Content-Type") ?? "";
            if (!type.Contains("application/json")) { done(null, "The service returned a web page instead of a JSON response."); yield break; }
            JObject? data = null; string? error = null;
            try { data = JObject.Parse(active.downloadHandler.text); } catch (JsonException) { error = "The service returned invalid JSON."; }
            if (data?["error"] != null || data?["detail"] != null) error = "The selected service rejected the request.";
            done(data, error);
        }
        finally { if (request == active) request = null; }
    }
}
