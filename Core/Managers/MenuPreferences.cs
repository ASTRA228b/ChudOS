// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.IO;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using Newtonsoft.Json;
using UnityEngine;

namespace ChudOS.Core.Managers;

public static class MenuPreferences
{
    private static SavedPreferences saved = new();
    private static readonly HashSet<string> favorites = new(StringComparer.OrdinalIgnoreCase);
    private static bool remember, help;
    private static string SavePath => Path.Combine(FileManager.RootFolder, "menu-preferences.json");
    public static IEnumerable<ButtonManager.ButtonData> Favorites => ButtonManager.Buttons.Where(b => favorites.Contains(b.Name));

    public sealed class SavedPreferences
    {
        public List<string> Favorites { get; set; } = new();
        public string Category { get; set; } = CategoryManager.Home;
        public int Page { get; set; }
    }

    public static void Initialize()
    {
        favorites.Clear();
        try
        {
            saved = File.Exists(SavePath) ? JsonConvert.DeserializeObject<SavedPreferences>(File.ReadAllText(SavePath)) ?? new() : new();
            foreach (string name in saved.Favorites ?? new())
                if (name != null && ButtonManager.GetButton(name) != null) favorites.Add(name);
        }
        catch (Exception ex) { saved = new(); Debug.LogWarning($"[ChudOS] Menu preferences: {ex.Message}"); }
    }

    public static bool IsFavorite(string name) => favorites.Contains(name);
    public static void ToggleFavorite(ButtonManager.ButtonData button)
    {
        bool added = favorites.Add(button.Name);
        if (!added) favorites.Remove(button.Name);
        Save();
        NotifyLib.SendNotificationALL(TranslationManager.T(added ? "Added to Favorites" : "Removed from Favorites") + ": " + TranslationManager.T(button.Name), 2500);
        CategoryManager.SearchChanged();
    }

    private static void Save()
    {
        try
        {
            saved.Favorites = favorites.OrderBy(name => name).ToList();
            Directory.CreateDirectory(FileManager.RootFolder);
            File.WriteAllText(SavePath, JsonConvert.SerializeObject(saved, Formatting.Indented));
        }
        catch (Exception ex) { Debug.LogWarning($"[ChudOS] Saving menu preferences: {ex.Message}"); }
    }

    public static void RememberLocation()
    {
        if (!remember || CategoryManager.IsSearching) return;
        if (saved.Category == CategoryManager.CurrentCategory && saved.Page == CategoryManager.PageNumber) return;
        saved.Category = CategoryManager.CurrentCategory;
        saved.Page = CategoryManager.PageNumber;
        Save();
    }

    public static void Reopen()
    {
        if (remember && saved.Category != CategoryManager.Home) CategoryManager.Open(saved.Category, saved.Page);
        else CategoryManager.GoHome();
    }

    public static void ShowHelp(ButtonManager.ButtonData button)
    {
        if (!help) return;
        string description = string.IsNullOrWhiteSpace(button.Attribute.Description) ? "No extra instructions for this button." : button.Attribute.Description;
        NotifyLib.SendNotificationALL(TranslationManager.T(button.Name) + ": " + TranslationManager.T(description), 5000);
    }

    [MenuButton("Remember Last Category", ButtonType.Toggle, Category = "Settings", Order = 150, runOption = RunOption.Once, Notify = false)]
    public static void EnableRemember() => remember = true;
    [MenuDisable("Remember Last Category")]
    public static void DisableRemember() => remember = false;

    [MenuButton("Button Help", ButtonType.Toggle, Category = "Settings", Order = 151, runOption = RunOption.Once, Notify = false, Description = "Show instructions when pressing a mod; grip + click or Shift + click favorites it")]
    public static void EnableHelp() => help = true;
    [MenuDisable("Button Help")]
    public static void DisableHelp() => help = false;
}
