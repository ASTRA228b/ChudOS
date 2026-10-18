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
        else if (value.Length == 1 && Query.Length < 32 && char.IsLetterOrDigit(value[0])) Query += value.ToLowerInvariant();
        else return;
        Rebuild();
        CategoryManager.SearchChanged();
    }
    private static void Rebuild()
    {
        Matches.Clear();
        if (!Active || Query.Length == 0) return;
        Matches.AddRange(ButtonManager.Buttons.Where(b => b.Name.IndexOf(Query, StringComparison.OrdinalIgnoreCase) >= 0));
    }
}
