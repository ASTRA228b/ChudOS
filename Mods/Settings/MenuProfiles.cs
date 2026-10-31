// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Stuff;
using Newtonsoft.Json;

namespace ChudOS.Mods.Settings;

public static class MenuProfiles
{
    private sealed class Entry { public string Name = ""; public bool Enabled; public int Option; }
    private sealed class Profile { public int Version = 1; public List<Entry> Entries = new(); }
    private static readonly HashSet<string> AllowedMods = new(StringComparer.Ordinal)
    {
        "Momentum Bank", "Surface Skate", "Air Paddle", "Gravity Compass", "Landing Brake", "Advanced Checkpoints", "Climby Hands", "Auto Climb", "Bunny Hop",
        "Echo Race", "Gate Runner", "Beat Targets", "Momentum Ribbon", "Contact Sparks", "Landing Pulse", "Beat Halo", "Smooth Desktop Camera"
    };
    private static readonly string[] Slots = { "Default", "Movement", "Visuals", "Recording" };
    private static readonly JsonSerializerSettings JsonSettings = new() { MaxDepth = 12, TypeNameHandling = TypeNameHandling.None };
    private static int slot;
    private static bool loadMods;
    private static string Folder => Path.Combine(FileManager.RootFolder, "Profiles");
    private static string CurrentPath => Path.Combine(Folder, Slots[slot] + ".json");
    private static bool Included(ButtonManager.ButtonData button) => button.Name != "Profile Slot" && button.Name != "Load Profile Mods" && (button.Category == "Settings" || AllowedMods.Contains(button.Name)) && button.Type != ButtonType.Press;
    [MenuButton("Profile Slot", ButtonType.Option, Category = "Settings", Order = 430, Options = new[] { "Default", "Movement", "Visuals", "Recording" }, Notify = false)]
    public static void SetSlot() => slot = ButtonManager.GetButton("Profile Slot")?.OptionIndex ?? 0;
    [MenuButton("Load Profile Mods", ButtonType.Toggle, Category = "Settings", Order = 431, runOption = RunOption.Once, Description = "Also restore the supported mod toggles when loading a profile")]
    public static void EnableMods() => loadMods = true;
    [MenuDisable("Load Profile Mods")]
    public static void DisableMods() => loadMods = false;
    [MenuButton("Save Profile", ButtonType.Press, Category = "Settings", Order = 432, Notify = false)]
    public static void Save()
    {
        try
        {
            Profile profile = new();
            foreach (var button in ButtonManager.Buttons) if (Included(button)) profile.Entries.Add(new Entry { Name = button.Name, Enabled = button.Enabled, Option = button.OptionIndex });
            Directory.CreateDirectory(Folder);
            string temporary = CurrentPath + ".tmp";
            File.WriteAllText(temporary, JsonConvert.SerializeObject(profile, Formatting.Indented), new System.Text.UTF8Encoding(false));
            if (File.Exists(CurrentPath)) File.Replace(temporary, CurrentPath, null); else File.Move(temporary, CurrentPath);
            CreativeTools.Notice($"{Slots[slot]} profile saved.");
        }
        catch (Exception ex) { UnityEngine.Debug.LogException(ex); CreativeTools.Notice("Could not save the profile."); }
    }
    private static Profile Read()
    {
        FileInfo info = new(CurrentPath);
        if (!info.Exists || info.Length > 256 * 1024) throw new InvalidDataException("Profile is missing or too large.");
        Profile? profile = JsonConvert.DeserializeObject<Profile>(File.ReadAllText(CurrentPath), JsonSettings);
        if (profile == null || profile.Version != 1 || profile.Entries == null || profile.Entries.Count > 2048) throw new InvalidDataException("Unsupported profile.");
        HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
        foreach (Entry entry in profile.Entries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Name) || !names.Add(entry.Name)) throw new InvalidDataException("Invalid profile entry.");
            var button = ButtonManager.GetButton(entry.Name);
            if (button == null || !Included(button)) continue;
            if (button.Type == ButtonType.Option && (entry.Option < 0 || entry.Option >= button.Attribute.Options.Length)) throw new InvalidDataException("Invalid profile setting.");
        }
        return profile;
    }
    [MenuButton("Preview Profile", ButtonType.Press, Category = "Settings", Order = 433, Notify = false)]
    public static void Preview()
    {
        try
        {
            Profile profile = Read();
            int settings = 0, mods = 0;
            foreach (Entry entry in profile.Entries)
            {
                var button = ButtonManager.GetButton(entry.Name);
                if (button == null || !Included(button)) continue;
                if (button.Category == "Settings") settings++; else if (entry.Enabled) mods++;
            }
            CreativeTools.Notice($"{Slots[slot]}: {settings} settings, {mods} enabled mods. Mod restoration is {(loadMods ? "ON" : "OFF")}.");
        }
        catch (Exception ex) { UnityEngine.Debug.LogWarning(ex.Message); CreativeTools.Notice("No valid profile exists in this slot."); }
    }
    [MenuButton("Load Profile", ButtonType.Press, Category = "Settings", Order = 434, Notify = false)]
    public static void Load()
    {
        try
        {
            Profile profile = Read();
            foreach (Entry entry in profile.Entries)
            {
                var button = ButtonManager.GetButton(entry.Name);
                if (button == null || !Included(button)) continue;
                if (button.Type == ButtonType.Option) ButtonManager.ChangeOption(button, entry.Option - button.OptionIndex);
                else if (button.Category == "Settings") ButtonManager.SetEnabled(button, entry.Enabled);
            }
            if (loadMods)
            {
                foreach (string name in AllowedMods) ButtonManager.SetEnabled(name, false);
                foreach (Entry entry in profile.Entries) if (entry.Enabled && AllowedMods.Contains(entry.Name)) ButtonManager.SetEnabled(entry.Name, true);
            }
            CreativeTools.Notice($"{Slots[slot]} profile loaded.");
        }
        catch (Exception ex) { UnityEngine.Debug.LogException(ex); CreativeTools.Notice("Could not load the profile."); }
    }
    [MenuButton("Reset Profile Settings", ButtonType.Press, Category = "Settings", Order = 435, Notify = false, Description = "Reset profile-supported settings to defaults; saved profile files are kept")]
    public static void Reset()
    {
        foreach (var button in ButtonManager.Buttons)
        {
            if (!Included(button)) continue;
            if (button.Type == ButtonType.Option) ButtonManager.ChangeOption(button, -button.OptionIndex);
            else ButtonManager.SetEnabled(button, button.Category == "Settings" && button.Attribute.DefaultEnabled);
        }
        CreativeTools.Notice("Profile-supported settings reset.");
    }
}
