// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ChudOS.Stuff;

namespace ChudOS.Core.Managers;

internal static class JarvisCommands
{
    internal readonly struct Command
    {
        internal readonly string Action, Target, Value;
        internal Command(string action, string target, string value = "") { Action = action; Target = target; Value = value; }
    }
    private static readonly Regex ActionPattern = new(@"^(enable|disable|toggle|increase|decrease|increment|decrement|press|open|set|change)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly string[] NumberWords = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fly"] = "Fly", ["freecam"] = "Free Camera", ["speed boost"] = "Speed Boost", ["theme"] = "Menu Theme",
        ["language"] = "Menu Language", ["speed boost amount"] = "Speed Boost Jump Speed", ["voice"] = "Jarvis Voice", ["jarvis model"] = "ChudGPT Model"
    };

    internal static string Normalize(string text)
    {
        StringBuilder result = new(text.Length);
        foreach (char c in text.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(c)) result.Append(char.ToLowerInvariant(c));
        return result.ToString();
    }
    internal static string CleanInput(string input)
    {
        string text = input.Trim().TrimEnd('.', '!', '?').Trim();
        string wake = Mods.Settings.JarvisSettings.WakeWords[Mods.Settings.JarvisSettings.WakeWord];
        if (text.StartsWith(wake + " ", StringComparison.OrdinalIgnoreCase) || text.StartsWith(wake + ",", StringComparison.OrdinalIgnoreCase)) text = text.Substring(wake.Length).TrimStart(' ', ',');
        if (text.StartsWith("please ", StringComparison.OrdinalIgnoreCase)) text = text.Substring(7);
        foreach (string prefix in new[] { "can you ", "could you ", "would you " })
            if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) { text = text.Substring(prefix.Length); break; }
        foreach (var pair in new[] { ("turn on ", "enable "), ("turn off ", "disable "), ("switch on ", "enable "), ("switch off ", "disable ") })
            if (text.StartsWith(pair.Item1, StringComparison.OrdinalIgnoreCase)) { text = pair.Item2 + text.Substring(pair.Item1.Length); break; }
        return text;
    }
    internal static bool TryParse(string input, out Command command, out string problem)
    {
        command = default; problem = "";
        string text = CleanInput(input);
        string lower = text.ToLowerInvariant();
        string special = lower switch
        {
            "pause" or "pause music" or "pause the soundboard" => "pause",
            "play" or "resume" or "resume music" or "play the soundboard" => "play",
            "next song" or "next track" or "skip" => "next",
            "previous song" or "previous track" => "previous",
            "save menu state" or "save settings" => "save",
            "disconnect" or "leave room" => "disconnect",
            _ => ""
        };
        if (special.Length > 0) { command = new(special, ""); return true; }
        if (lower.StartsWith("join room ") || lower.StartsWith("join code "))
        {
            string code = text.Substring(10).Replace(" ", "").ToUpperInvariant();
            if (code.Length is < 1 or > 12 || code.Any(c => !(c is >= 'A' and <= 'Z' or >= '0' and <= '9'))) problem = "Use a room code with one to twelve letters or numbers.";
            else command = new("join", code);
            return true;
        }
        Match match = ActionPattern.Match(text);
        if (!match.Success) return false;
        string action = match.Groups[1].Value.ToLowerInvariant(), target = match.Groups[2].Value;
        if (action is "open")
        {
            string[] categories = ButtonManager.GetCategories().Concat(new[] { "Home", "Enabled Mods", "Favorites", "Detected" }).ToArray();
            string[] matches = categories.Where(c => Normalize(c) == Normalize(target)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (matches.Length == 1) { command = new("category", matches[0]); return true; }
        }
        if (action is "set" or "change")
        {
            var candidates = new List<Command>();
            foreach (var button in ButtonManager.Buttons.Where(b => b.Type is ButtonType.Option or ButtonType.Toggle))
            {
                foreach (string name in Names(button))
                {
                    if (!target.StartsWith(name + " ", StringComparison.OrdinalIgnoreCase)) continue;
                    string value = target.Substring(name.Length).Trim();
                    if (value.StartsWith("to ", StringComparison.OrdinalIgnoreCase)) value = value.Substring(3).Trim();
                    candidates.Add(new("set", button.Name, value));
                }
            }
            var unique = candidates.GroupBy(c => c.Target).Select(g => g.First()).OrderByDescending(c => c.Target.Length).ToArray();
            if (unique.Length > 0 && (unique.Length == 1 || unique[0].Target.Length > unique[1].Target.Length)) command = unique[0];
            else problem = unique.Length == 0 ? "Name a setting and its value, for example set menu theme to OG Blue." : "That matches multiple settings. Say the full setting name.";
            return true;
        }
        var found = Resolve(target);
        if (found.Length != 1) { problem = found.Length == 0 ? "I couldn't find that button. Say its full menu name." : "That matches multiple buttons. Say the full menu name."; return true; }
        command = new(action == "open" ? "press" : action, found[0].Name);
        return true;
    }
    internal static IEnumerable<string> Names(ButtonManager.ButtonData button)
    {
        yield return button.Name;
        foreach (var alias in Aliases) if (alias.Value.Equals(button.Name, StringComparison.OrdinalIgnoreCase)) yield return alias.Key;
    }
    private static ButtonManager.ButtonData[] Resolve(string target)
    {
        string normalized = Normalize(target);
        string? name = Aliases.FirstOrDefault(pair => Normalize(pair.Key) == normalized).Value;
        if (name != null && ButtonManager.GetButton(name) is { } exactAlias) return new[] { exactAlias };
        var exact = ButtonManager.Buttons.Where(b => Normalize(b.Name) == normalized).ToArray();
        if (exact.Length > 0) return exact;
        return ButtonManager.Buttons.Where(b => Normalize(b.Name).Contains(normalized)).Take(3).ToArray();
    }
    private static string OptionKey(string value)
    {
        value = value.ToLowerInvariant().Trim();
        string[] tens = { "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
        for (int i = 0; i < tens.Length; i++) value = Regex.Replace(value, @"\b" + tens[i] + @"\b", ((i + 2) * 10).ToString(CultureInfo.InvariantCulture));
        value = value.Replace("one hundred", "100");
        for (int i = 0; i < NumberWords.Length; i++) value = Regex.Replace(value, @"\b" + NumberWords[i] + @"\b", i.ToString(CultureInfo.InvariantCulture));
        return value.Replace(" point ", ".").Replace(" percent", "%").Replace(" per second", "/s").Replace(" ", "");
    }
    internal static bool Execute(Command command, out string result)
    {
        result = "Menu controls are turned off in Jarvis settings.";
        if (!Mods.Settings.JarvisSettings.Controls) return false;
        switch (command.Action)
        {
            case "category": CategoryManager.Open(command.Target); result = "Opened " + command.Target + "."; return true;
            case "save": FileManager.SaveMenuState(); result = "Save menu state requested."; return true;
            case "pause": if (SoundManager.IsPlaying) SoundboardManager.TogglePause(); result = "Soundboard paused."; return true;
            case "play": if (!SoundManager.IsPlaying) SoundboardManager.TogglePause(); result = "Soundboard playback requested."; return true;
            case "next": SoundboardManager.Next(); result = "Next sound selected."; return true;
            case "previous": SoundboardManager.Previous(); result = "Previous sound selected."; return true;
            case "join":
                if (GorillaNetworking.PhotonNetworkController.Instance == null) { result = "The room controller is not ready."; return false; }
                GorillaNetworking.PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(command.Target, GorillaNetworking.JoinType.Solo);
                result = "Requested room " + command.Target + "."; return true;
            case "disconnect": if (Photon.Pun.PhotonNetwork.IsConnected) Photon.Pun.PhotonNetwork.Disconnect(); result = "Disconnect requested."; return true;
        }
        var button = ButtonManager.GetButton(command.Target);
        if (button == null) { result = "That button is no longer available."; return false; }
        string action = command.Action;
        if (action == "set" && button.Type == ButtonType.Toggle)
        {
            string value = Normalize(command.Value);
            action = value is "on" or "enabled" or "true" ? "enable" : value is "off" or "disabled" or "false" ? "disable" : "invalid";
        }
        if (action is "enable" or "disable" or "toggle")
        {
            if (button.Type != ButtonType.Toggle) { result = "That button is not a toggle."; return false; }
            bool wanted = action == "toggle" ? !button.Enabled : action == "enable";
            JarvisManager.ExecuteLocal(() => ButtonManager.SetEnabled(button, wanted));
            result = button.Name + (button.Enabled ? " enabled." : " disabled.");
            return button.Enabled == wanted;
        }
        if (action == "set" && button.Type == ButtonType.Option)
        {
            string key = OptionKey(command.Value);
            int[] choices = button.Attribute.Options.Select((v, i) => (v, i)).Where(x => OptionKey(x.v) == key || Normalize(x.v) == Normalize(command.Value)).Select(x => x.i).ToArray();
            if (choices.Length != 1)
            {
                // Accept a spoken number only when exactly one existing numeric preset matches.
                choices = button.Attribute.Options.Select((v, i) => (v, i)).Where(x => Regex.Match(OptionKey(x.v), @"^-?\d+(?:\.\d+)?").Value == key && key.Length > 0).Select(x => x.i).ToArray();
            }
            if (choices.Length != 1) { result = "Choose an existing value for " + button.Name + ": " + string.Join(", ", button.Attribute.Options.Take(8)) + "."; return false; }
            bool success = false;
            JarvisManager.ExecuteLocal(() => success = ButtonManager.SetOption(button, choices[0]));
            result = success ? button.Name + " set to " + button.Option + "." : "Couldn't apply " + button.Name + "."; return success;
        }
        if (action is "increment" or "increase" or "decrement" or "decrease")
        {
            if (button.Type != ButtonType.Option) { result = "That button has no selectable values."; return false; }
            JarvisManager.ExecuteLocal(() => ButtonManager.ChangeOption(button, action is "increment" or "increase" ? 1 : -1));
            result = button.Name + " set to " + button.Option + "."; return true;
        }
        if (action == "press") { JarvisManager.ExecuteLocal(() => ButtonManager.Press(button)); result = "Requested " + button.Name + "."; return true; }
        result = "I couldn't apply that command to this button."; return false;
    }
}
