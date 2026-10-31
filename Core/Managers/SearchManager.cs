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

namespace ChudOS.Core.Managers;


public static class SearchManager
{
    private static readonly List<ButtonManager.ButtonData> Matches = new();
    public static string Query { get; private set; } = "";
    public static bool Active { get; private set; }
    public static bool KeyboardVisible { get; private set; }
    public static IReadOnlyList<ButtonManager.ButtonData> Results => Matches;

    public static void Toggle()
    {
        if (Active) KeyboardVisible = !KeyboardVisible;
        else { Active = true; KeyboardVisible = true; Query = ""; Rebuild(); }
        CategoryManager.SearchChanged();
    }
   
    public static void Blur()
    {
        if (!Active) return;
        KeyboardVisible = false;
        if (Query.Length == 0) Close();
        CategoryManager.SearchChanged();
    }
    public static void Close()
    {
        Active = false;
        KeyboardVisible = false;
        Query = "";
        Matches.Clear();
    }
    public static void Key(string value)
    {
        if (!Active) return;
        if (value == "Done") { Blur(); return; }
        if (value == "Del") { if (Query.Length > 0) Query = Query.Substring(0, Query.Length - 1); }
        else if (value == "Clear") Query = "";
        else if (value == "Space") { if (Query.Length < 32) Query += " "; }
        else if (value.Length == 1 && Query.Length < 32 && !char.IsControl(value[0]) && !char.IsSurrogate(value[0])) Query += value.ToLowerInvariant();
        else return;
        Rebuild();
        CategoryManager.SearchChanged();
    }
    private static void Rebuild()
    {
        Matches.Clear();
        if (!Active || Query.Length == 0) return;
        Matches.AddRange(ButtonManager.Buttons.Where(b => b.Name.IndexOf(Query, StringComparison.OrdinalIgnoreCase) >= 0 || System.Globalization.CultureInfo.InvariantCulture.CompareInfo.IndexOf(TranslationManager.T(b.Name), Query, System.Globalization.CompareOptions.IgnoreCase | System.Globalization.CompareOptions.IgnoreNonSpace) >= 0));
    }
    public static void RefreshLanguage() => Rebuild();
}
