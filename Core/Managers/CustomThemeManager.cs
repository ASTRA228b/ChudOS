// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ChudOS.Core.AGUI;
using ChudOS.Mods.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace ChudOS.Core.Managers;

internal sealed class CustomThemeDocument
{
    public int Version = 1;
    public string Name = "My Theme";
    public Dictionary<string, string> Colors = Defaults();
    public CustomThemeGradient Gradient = new();

    internal static Dictionary<string, string> Defaults() => new(StringComparer.Ordinal)
    {
        ["background"] = "#0C0B10", ["face"] = "#0E0D11", ["button"] = "#1E1E21", ["header"] = "#1F1F22",
        ["pressed"] = "#9B12FE", ["accent"] = "#9B12FE", ["border"] = "#A41EFF", ["divider"] = "#9B12FE",
        ["searchBorder"] = "#45464B", ["text"] = "#FFFFFF", ["secondaryText"] = "#8E919A"
    };
}

internal sealed class CustomThemeGradient
{
    public bool Enabled, Animated;
    public string Start = "#9B12FE", End = "#328CFF", PressedStart = "#9B12FE", PressedEnd = "#FF66CC";
    public string Direction = "Vertical";
    public float Speed = 0.15f;
}

internal sealed class CompiledCustomTheme
{
    internal MenuThemes.Theme Theme = new();
    internal bool Gradient, Animated;
    internal Color Start, End, PressedStart, PressedEnd;
    internal int Direction;
    internal float Speed;
}

internal static class CustomThemeManager
{
    internal const int MaxBytes = 16384;
    internal static readonly string[] ColorNames = { "background", "face", "button", "header", "pressed", "accent", "border", "divider", "searchBorder", "text", "secondaryText" };
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(), TypeNameHandling = TypeNameHandling.None,
        MissingMemberHandling = MissingMemberHandling.Error, MetadataPropertyHandling = MetadataPropertyHandling.Ignore, MaxDepth = 8
    };
    private static readonly UTF8Encoding Encoding = new(false, true);
    private static readonly List<string> files = new();
    private static CustomThemeDocument current = new();
    private static CompiledCustomTheme compiled = new();
    private static int selectedFile = -1;
    internal static bool Synchronizing { get; private set; }
    internal static string Folder => Path.Combine(FileManager.RootFolder, "Themes");
    internal static IReadOnlyList<string> Files => files;
    internal static bool Active => ButtonManager.GetButton("Menu Theme")?.Option == "Custom";
    internal static CompiledCustomTheme Compiled => compiled;

    internal static void Initialize()
    {
        TryCompile(current, out compiled, out _);
        Reload();
        GUIManager.Register("Custom Theme Editor", CustomThemeGUI.SetVisible, () => CustomThemeGUI.Visible, CustomThemeGUI.CreateGUI, () => CustomThemeGUI.Window);
    }
    internal static void Cleanup()
    {
        CustomThemeGUI.SetVisible(false);
        GUIManager.Unregister("Custom Theme Editor");
        files.Clear();
    }
    internal static string Export() => Serialize(current);
    internal static CustomThemeDocument Snapshot() => Copy(current);
    internal static string Serialize(CustomThemeDocument document) => JsonConvert.SerializeObject(document, Formatting.Indented, JsonSettings);
    internal static CustomThemeDocument Copy(CustomThemeDocument document) => JsonConvert.DeserializeObject<CustomThemeDocument>(Serialize(document), JsonSettings)!;

    internal static bool TryParse(string json, out CustomThemeDocument document, out string error)
    {
        document = new(); error = "";
        if (string.IsNullOrWhiteSpace(json)) { error = "Paste a theme JSON first."; return false; }
        if (System.Text.Encoding.UTF8.GetByteCount(json) > MaxBytes) { error = "Theme JSON must be 16 KiB or smaller."; return false; }
        try
        {
            var parsed = JsonConvert.DeserializeObject<CustomThemeDocument>(json, JsonSettings);
            if (parsed == null) { error = "The theme must be a JSON object."; return false; }
            if (!TryCompile(parsed, out _, out error)) return false;
            document = parsed;
            return true;
        }
        catch (JsonException ex) { error = "Invalid theme JSON: " + ex.Message; return false; }
    }
    private static bool TryCompile(CustomThemeDocument document, out CompiledCustomTheme result, out string error)
    {
        result = new(); error = "";
        if (document.Version != 1) { error = "Unsupported theme version. Use version 1."; return false; }
        if (document.Name == null || document.Colors == null || document.Gradient == null) { error = "Name, colors, and gradient cannot be null."; return false; }
        document.Name = document.Name.Trim();
        if (!Regex.IsMatch(document.Name, @"^[\p{L}\p{N}][\p{L}\p{N} _-]{0,47}$")) { error = "Use a name of 1-48 letters, numbers, spaces, underscores, or hyphens."; return false; }
        if (Regex.IsMatch(document.Name, @"^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9])$", RegexOptions.IgnoreCase)) { error = "That theme name is reserved by Windows."; return false; }
        var defaults = CustomThemeDocument.Defaults();
        foreach (string key in document.Colors.Keys)
            if (!defaults.ContainsKey(key)) { error = "Unknown color: " + key; return false; }
        foreach (var pair in defaults)
            if (!document.Colors.ContainsKey(pair.Key)) document.Colors[pair.Key] = pair.Value;
        var colors = new Dictionary<string, Color>(StringComparer.Ordinal);
        foreach (var pair in document.Colors)
        {
            if (!TryColor(pair.Value, out Color color)) { error = pair.Key + " must be an opaque #RRGGBB color."; return false; }
            colors[pair.Key] = color;
        }
        var gradient = document.Gradient;
        if (!TryColor(gradient.Start, out result.Start) || !TryColor(gradient.End, out result.End) || !TryColor(gradient.PressedStart, out result.PressedStart) || !TryColor(gradient.PressedEnd, out result.PressedEnd))
        { error = "Gradient colors must use opaque #RRGGBB values."; return false; }
        result.Direction = gradient.Direction switch { "Vertical" => 0, "Horizontal" => 1, "Diagonal" => 2, _ => -1 };
        if (result.Direction < 0) { error = "Gradient direction must be Vertical, Horizontal, or Diagonal."; return false; }
        if (float.IsNaN(gradient.Speed) || float.IsInfinity(gradient.Speed) || gradient.Speed < 0.01f || gradient.Speed > 1f) { error = "Gradient speed must be between 0.01 and 1."; return false; }
        result.Gradient = gradient.Enabled; result.Animated = gradient.Animated; result.Speed = gradient.Speed;
        result.Theme = new MenuThemes.Theme
        {
            Background = colors["background"], Face = colors["face"], Button = colors["button"], Header = colors["header"], Pressed = colors["pressed"],
            Accent = colors["accent"], Border = colors["border"], Divider = colors["divider"], SearchBorder = colors["searchBorder"], Text = colors["text"], SecondaryText = colors["secondaryText"]
        };
        return true;
    }
    internal static bool TryColor(string text, out Color color)
    {
        color = Color.white;
        return text != null && Regex.IsMatch(text, "^#[0-9a-fA-F]{6}$") && ColorUtility.TryParseHtmlString(text, out color);
    }
    internal static bool Apply(CustomThemeDocument document, out string error, bool synchronize = true)
    {
        var candidate = Copy(document);
        if (!TryCompile(candidate, out var theme, out error)) return false;
        current = candidate; compiled = theme; ExpansionPreferences.Revision++;
        if (synchronize) SynchronizeControls();
        var button = ButtonManager.GetButton("Menu Theme");
        if (button == null) return true;
        int custom = Array.IndexOf(button.Attribute.Options, "Custom");
        if (custom < 0) return true;
        if (button.OptionIndex != custom) ButtonManager.ChangeOption(button, custom - button.OptionIndex);
        else MenuThemes.Apply();
        return true;
    }
    internal static void SynchronizeControls()
    {
        if (Synchronizing) return;
        Synchronizing = true;
        try
        {
            SetControl("Gradient Theme", compiled.Gradient ? 6 : 0);
            SetControl("Gradient Animation", compiled.Animated ? 1 : 0);
            SetControl("Gradient Direction", compiled.Direction);
        }
        finally { Synchronizing = false; }
    }
    private static void SetControl(string name, int index)
    {
        var button = ButtonManager.GetButton(name);
        if (button != null && button.OptionIndex != index) ButtonManager.ChangeOption(button, index - button.OptionIndex);
    }
    internal static void ChangeGradient(string name)
    {
        var document = Snapshot();
        var gradient = document.Gradient;
        int index = ExpansionPreferences.Index(name);
        string Hex(Color color) => "#" + ColorUtility.ToHtmlStringRGB(color);
        switch (name)
        {
            case "Gradient Theme":
                gradient.Enabled = index != 0;
                if (index is > 0 and < 6)
                {
                    ExpansionPreferences.PresetGradientColors(index, out Color first, out Color second);
                    gradient.Start = Hex(first); gradient.End = Hex(second);
                }
                break;
            case "Gradient Start Color": gradient.Start = Hex(ExpansionPreferences.Colors[index]); break;
            case "Gradient End Color": gradient.End = Hex(ExpansionPreferences.Colors[(index + 1) % ExpansionPreferences.Colors.Length]); break;
            case "Gradient Enabled Start": gradient.PressedStart = Hex(ExpansionPreferences.Colors[(index + 2) % ExpansionPreferences.Colors.Length]); break;
            case "Gradient Enabled End": gradient.PressedEnd = Hex(ExpansionPreferences.Colors[(index + 3) % ExpansionPreferences.Colors.Length]); break;
            case "Gradient Animation": gradient.Animated = index == 1; break;
            case "Gradient Direction": gradient.Direction = index switch { 1 => "Horizontal", 2 => "Diagonal", _ => "Vertical" }; break;
            case "Gradient Speed": gradient.Speed = index switch { 1 => 0.05f, 2 => 0.3f, 3 => 0.6f, _ => 0.15f }; break;
            default: return;
        }
        Apply(document, out _, false);
    }
    internal static string Reload()
    {
        files.Clear(); selectedFile = -1;
        try
        {
            Directory.CreateDirectory(Folder);
            foreach (string path in Directory.EnumerateFiles(Folder, "*.json").Take(64))
                if (new FileInfo(path).Length <= MaxBytes) files.Add(Path.GetFileName(path));
            files.Sort(StringComparer.OrdinalIgnoreCase);
            return files.Count + " saved themes.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return "The themes folder could not be read."; }
    }
    internal static bool Read(string filename, out CustomThemeDocument document, out string error)
    {
        document = new(); error = "";
        if (!files.Contains(filename) || Path.GetFileName(filename) != filename) { error = "Select a saved theme first."; return false; }
        try
        {
            using var stream = new FileStream(Path.Combine(Folder, filename), FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length > MaxBytes) { error = "That theme is too large."; return false; }
            using var reader = new StreamReader(stream, Encoding, true);
            return TryParse(reader.ReadToEnd(), out document, out error);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DecoderFallbackException) { error = "That theme could not be read."; return false; }
    }
    internal static bool Save(CustomThemeDocument document, out string error)
    {
        var candidate = Copy(document);
        if (!TryCompile(candidate, out _, out error)) return false;
        string json = Serialize(candidate);
        if (Encoding.GetByteCount(json) > MaxBytes) { error = "That theme is too large."; return false; }
        string path = Path.Combine(Folder, candidate.Name + ".json"), temporary = path + ".tmp";
        try
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(temporary, json, Encoding);
            if (File.Exists(path)) File.Replace(temporary, path, null); else File.Move(temporary, path);
            Reload();
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { error = "The theme could not be saved."; return false; }
        finally
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
    }
    internal static string Next()
    {
        if (files.Count == 0) return "No saved themes. Open Custom Theme Editor to create one.";
        selectedFile = (selectedFile + 1) % files.Count;
        if (!Read(files[selectedFile], out var document, out string error)) return error;
        return Apply(document, out error) ? "Theme: " + document.Name : error;
    }
    internal static bool Restore(string? json, out string error)
    {
        error = "";
        if (string.IsNullOrWhiteSpace(json)) return true;
        if (!TryParse(json, out var document, out error)) return false;
        TryCompile(document, out compiled, out _);
        current = document; ExpansionPreferences.Revision++;
        return true;
    }
}
