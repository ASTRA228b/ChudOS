// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ChudOS.Stuff;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChudOS.Core.Managers.CustomBoards;

internal static class BoardTextStore
{
    private static readonly Regex VersionText = new(@"(?<=V:)\d+\.\d+\.\d+|(?<=Version: )\d+\.\d+\.\d+", RegexOptions.CultureInvariant);
    private static readonly UTF8Encoding Utf8 = new(false);

    internal static Dictionary<string, string> Load(string path, Dictionary<string, string> defaults)
    {
        bool exists = File.Exists(path);
        string original = exists ? File.ReadAllText(path, Utf8) : "";
        JObject data = exists ? JObject.Parse(original) : new JObject();
        JObject? previous = data["Defaults"] as JObject;
        Dictionary<string, string> result = new(StringComparer.Ordinal);

        foreach (var entry in defaults)
        {
            JToken? token = data[entry.Key];
            if (token != null && token.Type != JTokenType.Null && token.Type != JTokenType.String)
                throw new InvalidDataException("Invalid board text field: " + entry.Key);

            string? saved = token?.Type == JTokenType.String ? token.Value<string>() : null;
            string? oldDefault = previous?[entry.Key]?.Type == JTokenType.String ? previous[entry.Key]!.Value<string>() : null;
            bool unchanged = saved == null || saved == oldDefault;
            if (previous == null && saved != null) unchanged = Normalize(saved) == Normalize(entry.Value);

            string value = unchanged ? entry.Value : saved!;
            result.Add(entry.Key, value);
            data[entry.Key] = value;
        }

        data["DefaultsVersion"] = Constantss.Version;
        data["Defaults"] = JObject.FromObject(defaults);
        string updated = data.ToString(Formatting.Indented);
        if (!exists || !string.Equals(original, updated, StringComparison.Ordinal)) Save(path, updated, exists);
        return result;
    }

    private static string Normalize(string text) => VersionText.Replace(text, "{version}");

    private static void Save(string path, string json, bool exists)
    {
        string? directory = Path.GetDirectoryName(path);
        if (directory != null) Directory.CreateDirectory(directory);
        string temporary = path + ".tmp", backup = path + ".pre-012.bak";
        try
        {
            if (exists && !File.Exists(backup)) File.Copy(path, backup);
            File.WriteAllText(temporary, json, Utf8);
            if (exists) File.Replace(temporary, path, null);
            else File.Move(temporary, path);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
