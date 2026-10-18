// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra
//
// This file is part of ChudOS.
// ChudOS is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 3,
// as published by the Free Software Foundation.
//
// ChudOS is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with ChudOS. If not, see <https://www.gnu.org/licenses/>.

using UnityEngine;
using ChudOS.Stuff;

namespace ChudOS.Core.Managers;


public static class CategoryManager
{
    public const string Home = "Home";
    public const string Main = "Main";
    private static readonly List<string> Categories = new() { "Main", "Enabled Mods", "Movement", "Player", "Rig", "Visuals", "Weather", "Fun", "Room", "Soundboard", "Safety", "Settings" };
    public static string CurrentCategory { get; private set; } = Home;
    public static int PageNumber { get; private set; }
    public static int PageLimit = 3;
    public static bool IsHome => CurrentCategory == Home;
    public static bool IsSearching => SearchManager.Active && SearchManager.Query.Length > 0;
    public static event Action? OnPageChanged;

    public static void Initialize()
    {
        foreach (string category in ButtonManager.GetCategories())
            if (!Categories.Any(c => string.Equals(c, category, StringComparison.OrdinalIgnoreCase))) Categories.Add(category);
        GoHome();
    }


    private static bool IsEnabledMod(ButtonManager.ButtonData button) => button.Type == ButtonType.Toggle && button.Enabled && !string.Equals(button.Category, "Settings", StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<string> GetVisibleCategories() => Categories.Skip(PageNumber * PageLimit).Take(PageLimit).ToList();

    public static IReadOnlyList<ButtonManager.ButtonData> GetVisibleButtons()
    {
        IEnumerable<ButtonManager.ButtonData> buttons = IsSearching ? SearchManager.Results : CurrentCategory == "Enabled Mods" ? ButtonManager.Buttons.Where(IsEnabledMod) : ButtonManager.GetCategory(CurrentCategory);
        PageNumber = Mathf.Clamp(PageNumber, 0, TotalPages - 1);
        return buttons.Skip(PageNumber * PageLimit).Take(PageLimit).ToList();
    }


    public static int TotalPages
    {
        get
        {
            int count = IsSearching ? SearchManager.Results.Count : IsHome ? Categories.Count : CurrentCategory == "Enabled Mods" ? ButtonManager.Buttons.Count(IsEnabledMod) : ButtonManager.GetCategory(CurrentCategory).Count;
            return Mathf.Max(1, Mathf.CeilToInt(count / (float)PageLimit));
        }
    }

    public static void Open(string category)
    {
        string? match = Categories.FirstOrDefault(c => string.Equals(c, category, StringComparison.OrdinalIgnoreCase));
        if (match == null) return;
        SearchManager.Close();
        CurrentCategory = match;
        PageNumber = 0;
        OnPageChanged?.Invoke();
    }
    public static void GoHome()
    {
        SearchManager.Close();
        CurrentCategory = Home;
        PageNumber = 0;
        OnPageChanged?.Invoke();
    }
    public static void Back() => GoHome();
    public static void SearchChanged() { PageNumber = 0; OnPageChanged?.Invoke(); }
    public static void NextPage() { PageNumber = (PageNumber + 1) % TotalPages; OnPageChanged?.Invoke(); }
    public static void PreviousPage() { PageNumber = (PageNumber + TotalPages - 1) % TotalPages; OnPageChanged?.Invoke(); }
}
