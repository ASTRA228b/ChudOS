using System;
using System.IO;
using ChudOS.Stuff;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace ChudOS.Core.Managers.CustomBoards;

public sealed class Changetext : MonoBehaviour
{
    public string COCHeadingTXT = $"{Constantss.Name} - V:{Constantss.Version}";
    public string MOTDHeadingTXT = Constantss.Name;
    public string MOTDbodyTXT =
        $"Welcome to ChudOS V:{Constantss.Version}!\n\n" +
        "ChudOS is a Gorilla Tag PCVR mod menu developed to provide a customizable and easy-to-use experience.\n\n" +
        "DISCLAIMER\n" +
        "ChudOS is an independent, third-party project and is not affiliated with Gorilla Tag or Another Axiom.\n\n" +
        "Some features may cause unexpected behavior or stop working after game updates. Use ChudOS at your own risk and only in environments where modding is permitted.\n\n" +
        "Thank you for using ChudOS! - Astra";
    public string COCbodyTXT =
        "CHUDOS - INFORMATION\n\n" +
        "ChudOS is a Gorilla Tag PCVR mod menu developed by Astra.\n\n" +
        "CONTROLS\n" +
        "VR: Left controller secondary button. Hold your wrist facing you.\n" +
        "Desktop: Press H to open the menu.\n\n" +
        $"Version: {Constantss.Version}\nDeveloped by Astra.";

    private const string BoardRoot = "Environment Objects/LocalObjects_Prefab/TreeRoom/";
    private static Changetext? instance;
    private static bool boardsEnabled = true, motdEnabled = true, cocEnabled = true, uppercase;
    private static int colorIndex;
    private float nextScan, nextApply;
    private string[] displayText = Array.Empty<string>();
    private readonly BoardSlot[] slots =
    {
        new("motdHeadingText", true),
        new("motdBodyText", true),
        new("CodeOfConductHeadingText", false),
        new("COCBodyText_TitleData", false)
    };

    private static string TextPath => Path.Combine(FileManager.RootFolder, "boards.json");

    private sealed class BoardText
    {
        public string? MOTDHeadingTXT;
        public string? MOTDbodyTXT;
        public string? COCHeadingTXT;
        public string? COCbodyTXT;
    }

    private sealed class BoardSlot
    {
        public readonly string Path;
        public readonly bool IsMotd;
        public TMP_Text? Text;
        private PlayFabTitleDataTextDisplay? updater;
        private string originalText = "";
        private Color originalColor;
        private bool updaterWasEnabled, applied;

        public BoardSlot(string path, bool isMotd) { Path = path; IsMotd = isMotd; }

        public void Find()
        {
            if (Text != null) return;
            GameObject board = GameObject.Find(BoardRoot + Path);
            if (board == null) return;
            Text = board.GetComponent<TMP_Text>();
            updater = board.GetComponent<PlayFabTitleDataTextDisplay>();
            applied = false;
        }

        public void Apply(string value)
        {
            if (Text == null) return;
            if (!applied)
            {
                originalText = Text.text;
                originalColor = Text.color;
                updaterWasEnabled = updater != null && updater.enabled;
                applied = true;
            }
            if (updater != null && updater.enabled) updater.enabled = false;
            if (Text.text != value) Text.text = value;
            Color color = colorIndex switch
            {
                1 => new Color32(155, 18, 254, 255),
                2 => Color.cyan,
                3 => Color.green,
                4 => Color.white,
                _ => originalColor
            };
            if (Text.color != color) Text.color = color;
        }

        public void Restore()
        {
            if (!applied) return;
            if (Text != null) { Text.text = originalText; Text.color = originalColor; }
            if (updater != null) updater.enabled = updaterWasEnabled;
            applied = false;
        }
    }

    private static Changetext EnsureInstance()
    {
        if (instance != null) return instance;
        throw new InvalidOperationException("Board manager has not been started by Plugin.cs.");
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(this); return; }
        instance = this;
        ReloadText();
    }

    private void LateUpdate()
    {
        if (!boardsEnabled) return;
        float now = Time.unscaledTime;
        if (now >= nextScan)
        {
            nextScan = now + 1f;
            foreach (BoardSlot slot in slots)
                if (slot.Text == null) slot.Find();
        }
        if (now < nextApply) return;
        nextApply = now + 0.25f;
        ApplyBoards();
    }

    private void ApplyBoards()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            BoardSlot slot = slots[i];
            if (boardsEnabled && (slot.IsMotd ? motdEnabled : cocEnabled)) slot.Apply(displayText[i]);
            else slot.Restore();
        }
    }

    private void RefreshText()
    {
        displayText = new[] { MOTDHeadingTXT, MOTDbodyTXT, COCHeadingTXT, COCbodyTXT };
        for (int i = 0; i < displayText.Length; i++)
            displayText[i] = uppercase ? (displayText[i] ?? "").ToUpperInvariant() : displayText[i] ?? "";
        ApplyBoards();
        nextApply = 0f;
    }

    private void ReloadText()
    {
        try
        {
            if (File.Exists(TextPath))
            {
                BoardText? data = JsonConvert.DeserializeObject<BoardText>(File.ReadAllText(TextPath));
                if (data == null) throw new InvalidDataException("Board text file is empty.");
                MOTDHeadingTXT = data.MOTDHeadingTXT ?? MOTDHeadingTXT;
                MOTDbodyTXT = data.MOTDbodyTXT ?? MOTDbodyTXT;
                COCHeadingTXT = data.COCHeadingTXT ?? COCHeadingTXT;
                COCbodyTXT = data.COCbodyTXT ?? COCbodyTXT;
            }
            else
            {
                Directory.CreateDirectory(FileManager.RootFolder);
                File.WriteAllText(TextPath, JsonConvert.SerializeObject(new BoardText
                {
                    MOTDHeadingTXT = MOTDHeadingTXT, MOTDbodyTXT = MOTDbodyTXT,
                    COCHeadingTXT = COCHeadingTXT, COCbodyTXT = COCbodyTXT
                }, Formatting.Indented));
            }
        }
        catch (Exception ex) { Debug.LogError($"[{Constantss.GUID}] Board text: {ex.Message}"); }
        RefreshText();
    }

    private void OnDisable()
    {
        foreach (BoardSlot slot in slots) slot.Restore();
    }

    private void OnEnable() { nextScan = 0f; nextApply = 0f; }

    private void OnDestroy()
    {
        foreach (BoardSlot slot in slots) slot.Restore();
        if (instance == this) instance = null;
    }

    [MenuButton("Custom Boards", ButtonType.Toggle, Category = "Settings", Order = 20,
        DefaultEnabled = true, runOption = RunOption.Once)]
    public static void EnableBoards()
    {
        boardsEnabled = true;
        Changetext manager = EnsureInstance();
        manager.enabled = true;
        manager.RefreshText();
    }

    [MenuDisable("Custom Boards")]
    public static void DisableBoards()
    {
        boardsEnabled = false;
        if (instance != null) { instance.ApplyBoards(); instance.enabled = false; }
    }

    [MenuButton("Custom MOTD Board", ButtonType.Toggle, Category = "Settings", Order = 21,
        DefaultEnabled = true, runOption = RunOption.Once)]
    public static void EnableMotd() { motdEnabled = true; EnsureInstance().ApplyBoards(); }

    [MenuDisable("Custom MOTD Board")]
    public static void DisableMotd() { motdEnabled = false; if (instance != null) instance.ApplyBoards(); }

    [MenuButton("Custom COC Board", ButtonType.Toggle, Category = "Settings", Order = 22,
        DefaultEnabled = true, runOption = RunOption.Once)]
    public static void EnableCoc() { cocEnabled = true; EnsureInstance().ApplyBoards(); }

    [MenuDisable("Custom COC Board")]
    public static void DisableCoc() { cocEnabled = false; if (instance != null) instance.ApplyBoards(); }

    [MenuButton("Board Uppercase", ButtonType.Toggle, Category = "Settings", Order = 23, runOption = RunOption.Once)]
    public static void EnableUppercase() { uppercase = true; EnsureInstance().RefreshText(); }

    [MenuDisable("Board Uppercase")]
    public static void DisableUppercase() { uppercase = false; if (instance != null) instance.RefreshText(); }

    [MenuButton("Board Color", ButtonType.Option, Category = "Settings", Order = 24,
        Options = new[] { "Original", "Purple", "Cyan", "Green", "White" })]
    public static void ChangeBoardColor()
    {
        colorIndex = ButtonManager.GetButton("Board Color")?.OptionIndex ?? 0;
        EnsureInstance().ApplyBoards();
    }

    [MenuButton("Open Board Text", ButtonType.Press, Category = "Settings", Order = 25)]
    public static void OpenBoardText()
    {
        EnsureInstance();
        if (!File.Exists(TextPath)) throw new FileNotFoundException("Board text file could not be created.", TextPath);
        Application.OpenURL(new Uri(TextPath).AbsoluteUri);
    }

    [MenuButton("Reload Board Text", ButtonType.Press, Category = "Settings", Order = 26)]
    public static void ReloadBoardText() => EnsureInstance().ReloadText();
}
