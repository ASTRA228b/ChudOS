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

using System.Collections;
using ChudOS.Core.AGUI;
using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Libraries.Notifications;
using ChudOS.Libraries.Notifications.CanvasNotify;
using ChudOS.Mods.SoundBoard;
using ChudOS.Stuff;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChudOS.Core;


public sealed partial class Main : MonoBehaviour
{
    public static bool IsMenuOpen;
    public static GameObject? MenuMain;
    public static GameObject? MenuBackground;
    public static TextMesh? FPS;


    private static Color BackgroundColor = new Color32(12, 11, 16, 255);
    private static Color MenuColor = new Color32(14, 13, 17, 255);
    private static Color ButtonColor = new Color32(30, 30, 33, 255);
    private static Color HeaderColor = new Color32(31, 31, 34, 255);
    private static Color PrimaryColor = new Color32(155, 18, 254, 255);
    private static Color BorderColor = new Color32(164, 30, 255, 255);
    private static Color DividerColor = new Color32(155, 18, 254, 255);
    private static Color SearchBorderColor = new Color32(69, 70, 75, 255);
    private static Color TextColor = new Color32(255, 255, 255, 255);
    private static Color SecondaryTextColor = new Color32(142, 145, 154, 255);
    public static Color PrimaryColorValue => PrimaryColor;
    public static Color ButtonColorValue => ButtonColor;
    public static Color TextColorValue => TextColor;
    public static bool TextOnlyMenu { get; private set; }

    private static readonly Vector3 VRMenuScale = new(0.129f, 0.221f, 0.297f);
    private static readonly Vector3 DesktopMenuScale = new(0.1f, 0.3f, 0.3825f);
    public static Vector3 VRMenuPosition = new(0f, 0.02f, 0f);

    private GUIRuntime? gui;
    private static Main? Instance;
    private static GameObject? RightReference;
    private static SphereCollider? RightButtonCollider;
    private static Camera? PCamera;
    private static bool OnPC, LastController, NeedsRefresh, Pending;
    private static Material? BackgroundMat, MenuMat, ButtonMat, HeaderMat, BorderMat, DividerMat, SearchBorderMat;
    private static TextMesh? SearchText;
    private static readonly List<GameObject> PageObjects = new();
    private static readonly Dictionary<TextMesh, string> MenuTextOriginal = new();
    private static readonly List<GameObject> KeyboardObjects = new();
    private static bool KeyboardBuilt;
    private static float LastFpsTime;
    private static int LastFps;
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        FileManager.Initialize(this);
        TranslationManager.Initialize(this);
        TranslationManager.Changed += RefreshLanguage;
        ButtonManager.Initialize();
        MenuPreferences.Initialize();
        ChudOS.Mods.Settings.MenuThemes.Apply();
        CategoryManager.PageLimit = 3;
        CategoryManager.Initialize();
        SoundboardManager.Initialize(this);
        SoundboardManager.Changed += SoundboardChanged;
        GUIManager.Register("Soundboard", state => SoundboardGUI.Visible = state, () => SoundboardGUI.Visible, SoundboardGUI.CreateGUI, () => SoundboardGUI.Window);
        MacroManager.Reload();
        MultiMP3Manager.Initialize();
        GUIManager.Register("Macros", ChudOS.Mods.Movement.MacroMods.SetVisible, () => ChudOS.Mods.Movement.MacroMods.Visible, ChudOS.Mods.Movement.MacroMods.CreateGUI, () => ChudOS.Mods.Movement.MacroMods.Window);
        ArrayListGUI.Initialize();
        gui = gameObject.AddComponent<GUIRuntime>();
        gui.enabled = false;
        ButtonManager.OnNotification -= Notify;
        ButtonManager.OnNotification += Notify;
        ButtonManager.OnButtonsChanged += RequestRefresh;
        CategoryManager.OnPageChanged += RequestRefresh;
    }


    private void Start()
    {
        CanvasNotifyLib.InitCanvas();
        StartCoroutine(RestoreMenuState());
    }

    private IEnumerator RestoreMenuState()
    {
        while (GorillaTagger.Instance == null || VRRig.LocalRig == null)
            yield return null;

        yield return null;

        FileManager.LoadMenuState();
    }


    private void OnGUI()
    {
        if (!NotifyLib.HasToasts && !ArrayListGUI.HasEntries) return;
        NotifyLib.InitToast();
        ArrayListGUI.CreateArray();
    }

    private void Update()
    {
        ButtonManager.Update();
        SoundboardManager.Tick();
        GUIManager.Tick();
        CanvasNotifyLib.CanvasUpdate();
        NotifyLib.Tick();
        bool showGUI = GUIManager.HasVisibleWindows;
        if (gui != null && gui.enabled != showGUI) gui.enabled = showGUI;

        Keyboard? keyboard = Keyboard.current;
        bool vrInput = ControllerInputPoller.instance != null && ControllerInputPoller.instance.leftControllerSecondaryButton;
        if (vrInput && !LastController) SwitchMenu(false);
        LastController = vrInput;
        if (keyboard != null)
        {
            if (SearchManager.KeyboardVisible && IsMenuOpen) HandleSearchTyping(keyboard);
            else if (keyboard.hKey.wasPressedThisFrame) SwitchMenu(true);
        }
        if (!IsMenuOpen || MenuMain == null) return;
        if (OnPC && PCamera != null)
        {

            if (Mouse.current?.leftButton.wasPressedThisFrame == true && !GUIManager.ContainsPointer(new Vector2(Mouse.current.position.ReadValue().x, Screen.height - Mouse.current.position.ReadValue().y)))
            {
                Ray ray = PCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, 3f, 1 << 2, QueryTriggerInteraction.Collide) && hit.collider.TryGetComponent(out MenuButtonCollider button))
                    button.Press();
                else if (SearchManager.KeyboardVisible) { SearchManager.Blur(); RequestRefresh(); }
            }
        }
        if (NeedsRefresh) { NeedsRefresh = false; RefreshPage(); }
        UpdateSoundboardControl();
        if (Time.unscaledTime - LastFpsTime >= 0.25f)
        {
            LastFpsTime = Time.unscaledTime;
            LastFps = Mathf.RoundToInt(1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f));
            if (FPS != null) FPS.text = "FPS: " + LastFps;
        }
    }
    private static void HandleSearchTyping(Keyboard keyboard)
    {
        if (keyboard.escapeKey.wasPressedThisFrame) { SearchManager.Blur(); RequestRefresh(); return; }
        if (keyboard.enterKey.wasPressedThisFrame) { SearchManager.Key("Done"); return; }
        if (keyboard.backspaceKey.wasPressedThisFrame) { SearchManager.Key("Del"); return; }
        if (keyboard.spaceKey.wasPressedThisFrame) { SearchManager.Key("Space"); return; }
        for (int i = 0; i < 26; i++) if (keyboard[(Key)((int)Key.A + i)].wasPressedThisFrame) SearchManager.Key(((char)('A' + i)).ToString());
        for (int i = 0; i < 10; i++) if (keyboard[(Key)((int)Key.Digit0 + i)].wasPressedThisFrame) SearchManager.Key(i.ToString());
    }
    private void FixedUpdate() => ButtonManager.FixedUpdate();
    private void LateUpdate() => ButtonManager.LateUpdate();
    private void OnDestroy()
    {
        if (Instance != this) return;
        ChudOS.Mods.Movement.ExtraMovementMods.Clear();
        ChudOS.Mods.Movement.PunchMods.Stop();
        ChudOS.Mods.Safety.NoFingerPose.Disable();
        ChudOS.Mods.Movement.NoClipMods.Clear();
        ChudOS.Mods.Movement.Predictions.Stop();
        ChudOS.Mods.Movement.PitGeo.Stop();
        ChudOS.Core.Managers.Particles.ParticleManager.Shutdown();
        ChudOS.Mods.Movement.LongArmMods.Reset();
        MacroManager.Shutdown();
        GUIManager.Unregister("Macros");
        MultiMP3Manager.Shutdown();
        ButtonManager.OnNotification -= Notify;
        ButtonManager.OnButtonsChanged -= RequestRefresh;
        CategoryManager.OnPageChanged -= RequestRefresh;
        TranslationManager.Changed -= RefreshLanguage;
        TranslationManager.Shutdown();
        if (gui != null) Destroy(gui);
        GUIManager.Unregister("Soundboard");
        ArrayListGUI.Shutdown();
        ChudOS.Mods.Visuals.OverlayMod.Shutdown();
        ChudOS.Mods.Movement.PullMod.Stop();
        SoundboardManager.Changed -= SoundboardChanged;
        MenuFeedback.Shutdown();
        RemoveMenu();
        SoundboardManager.Shutdown();
        CanvasNotifyLib.Shutdown();
        GunLib.Cleanup();
        GlobalStyles.Cleanup();
        IconLoader.Cleanup();
        DestroyMaterial(BackgroundMat); DestroyMaterial(MenuMat); DestroyMaterial(ButtonMat); DestroyMaterial(HeaderMat);
        DestroyMaterial(BorderMat); DestroyMaterial(DividerMat); DestroyMaterial(SearchBorderMat);
        Instance = null;
    }
    private static void DestroyMaterial(Material? m) { if (m != null) Destroy(m); }
    private static void Notify(string text) => NotifyLib.SendNotificationALL(text, 3500);
    public static void RequestRefresh() { NeedsRefresh = true; }
    private static void RefreshLanguage()
    {
        foreach (var item in MenuTextOriginal.ToArray())
        {
            if (item.Key == null) { MenuTextOriginal.Remove(item.Key!); continue; }
            item.Key.text = TranslationManager.UI(item.Value);
        }
        SearchManager.RefreshLanguage();
        RequestRefresh();
    }
    public static void ApplyMenuTheme(ChudOS.Mods.Settings.MenuThemes.Theme theme)
    {
        Color oldSecondary = SecondaryTextColor;
        BackgroundColor = theme.Background;
        MenuColor = theme.Face;
        ButtonColor = theme.Button;
        HeaderColor = theme.Header;
        PrimaryColor = theme.Accent;
        BorderColor = theme.Border;
        DividerColor = theme.Divider;
        SearchBorderColor = theme.SearchBorder;
        TextColor = theme.Text;
        SecondaryTextColor = theme.SecondaryText;
        TextOnlyMenu = theme.TextOnly;

        if (BackgroundMat != null) BackgroundMat.color = BackgroundColor;
        if (MenuMat != null) MenuMat.color = MenuColor;
        if (ButtonMat != null) ButtonMat.color = ButtonColor;
        if (HeaderMat != null) HeaderMat.color = HeaderColor;
        if (BorderMat != null) BorderMat.color = BorderColor;
        if (DividerMat != null) DividerMat.color = DividerColor;
        if (SearchBorderMat != null) SearchBorderMat.color = SearchBorderColor;

        if (MenuMain == null) return;
        foreach (TextMesh label in MenuMain.GetComponentsInChildren<TextMesh>(true))
            label.color = label.color == oldSecondary ? SecondaryTextColor : TextColor;

        RefreshThemeObjects();
        RequestRefresh();
    }
    private static void RefreshThemeObjects()
    {
        if (MenuMain == null) return;
        foreach (Renderer renderer in MenuMain.GetComponentsInChildren<Renderer>(true))
            renderer.enabled = !TextOnlyMenu || renderer.GetComponent<TextMesh>() != null;

        foreach (MenuButtonCollider button in MenuMain.GetComponentsInChildren<MenuButtonCollider>(true))
        {
            Renderer renderer = button.GetComponent<Renderer>();
            button.IdleColor = renderer != null && renderer.sharedMaterial == HeaderMat ? HeaderColor : ButtonColor;
            button.RefreshAppearance();
            if ((button.PreviousPage || button.NextPage) && button.Label != null)
            {
                Renderer labelRenderer = button.Label.GetComponent<Renderer>();
                if (labelRenderer != null) labelRenderer.enabled = TextOnlyMenu;
            }
        }
    }
    public static void OpenPC() => SwitchMenu(true);
    private static void SwitchMenu(bool desktop)
    {
        if (IsMenuOpen && OnPC == desktop) { RemoveMenu(); return; }
        if (IsMenuOpen) RemoveMenu();
        OnPC = desktop;
        ManageEnableMenu();
    }
    public static void ManageEnableMenu()
    {
        if (IsMenuOpen) { RemoveMenu(); return; }
        MenuPreferences.Reopen();
        IsMenuOpen = true;
        CreateMenu();
    }
    private static void CreateMaterials()
    {
        if (MenuMat != null) return;
        Shader shader = Shader.Find("Unlit/Color") ?? Shader.Find("GorillaTag/UberShader") ?? Shader.Find("Standard");
        BackgroundMat = new Material(shader) { color = BackgroundColor };
        MenuMat = new Material(shader) { color = MenuColor };
        ButtonMat = new Material(shader) { color = ButtonColor };
        HeaderMat = new Material(shader) { color = HeaderColor };
        BorderMat = new Material(shader) { color = BorderColor };
        DividerMat = new Material(shader) { color = DividerColor };
        SearchBorderMat = new Material(shader) { color = SearchBorderColor };
    }
    public static void CreateMenu()
    {
        if (MenuMain != null) return;
        if (GorillaTagger.Instance == null && !OnPC) { IsMenuOpen = false; return; }
        if (OnPC) SetupDesktopCamera();
        if (OnPC && PCamera == null) { IsMenuOpen = false; return; }
        CreateMaterials();
        MenuMain = new GameObject("ChudOS Tablet");
        Transform parent = OnPC ? PCamera!.transform : GorillaTagger.Instance!.leftHandTransform;
        MenuMain.transform.SetParent(parent, false);
        MenuMain.transform.localPosition = OnPC ? new Vector3(-0.12f, -0.02f, 0.8f) : VRMenuPosition;
        MenuMain.transform.localRotation = OnPC ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
        MenuMain.transform.localScale = OnPC ? DesktopMenuScale : VRMenuScale;
        if (!OnPC) AlignToPalm();


        MenuBackground = Cube("Tablet Backing", new Vector3(0.475f, 0f, 0f), new Vector3(0.16f, 2.28f, 1.05f), BackgroundMat);
        Cube("Tablet Face", new Vector3(0.558f, 0f, 0f), new Vector3(0.015f, 2.24f, 1.015f), MenuMat);
        Cube("Outline Top", new Vector3(0.58f, 1.14f, 0f), new Vector3(0.025f, 0.017f, 1.055f), BorderMat);
        Cube("Outline Bottom", new Vector3(0.58f, -1.14f, 0f), new Vector3(0.025f, 0.017f, 1.055f), BorderMat);
        Cube("Outline Left", new Vector3(0.58f, 0f, 0.526f), new Vector3(0.025f, 2.28f, 0.017f), BorderMat);
        Cube("Outline Right", new Vector3(0.58f, 0f, -0.526f), new Vector3(0.025f, 2.28f, 0.017f), BorderMat);

        MakeButton("Disconnect", 0.985f, 0f, 0.91f, 0.175f, disconnect: true, material: HeaderMat);
        Text("ChudOS", 0.738f, 0f, 0.030f, color: TextColor);
        FPS = Text("FPS: --", 0.568f, 0f, 0.015f, color: SecondaryTextColor);
        int searchStart = MenuMain.transform.childCount;
        Cube("Search Border", new Vector3(0.615f, 0.348f, 0f), new Vector3(0.035f, 0.171f, 0.915f), SearchBorderMat);
        MakeButton("", 0.348f, 0f, 0.902f, 0.157f, search: true);
        SearchText = Text("Search...", 0.348f, -0.355f, 0.017f, color: SecondaryTextColor, anchor: TextAnchor.MiddleLeft);
        IconLoader.Create("Search", MenuMain.transform, new Vector3(0.703f, 0.348f, 0.361f), new Vector2(0.076f, 0.070f));
        SearchObjects.Clear();
        for (int i = searchStart; i < MenuMain.transform.childCount; i++) SearchObjects.Add(MenuMain.transform.GetChild(i).gameObject);
        Cube("Footer Divider", new Vector3(0.617f, -0.793f, 0f), new Vector3(0.022f, 0.010f, 0.93f), DividerMat);
        MakeButton("", -0.99f, -0.375f, 0.2f, 0.18f, previous: true);
        MakeButton("Home", -0.99f, 0f, 0.405f, 0.18f, home: true);
        MakeButton("", -0.99f, 0.375f, 0.2f, 0.18f, next: true);
        IconLoader.Create("Arrow_Left", MenuMain.transform, new Vector3(0.704f, -0.99f, -0.375f), new Vector2(0.085f, 0.075f));
        IconLoader.Create("Home", MenuMain.transform, new Vector3(0.704f, -0.99f, -0.092f), new Vector2(0.065f, 0.065f));
        IconLoader.Create("Arrow_Right", MenuMain.transform, new Vector3(0.704f, -0.99f, 0.375f), new Vector2(0.085f, 0.075f));

        if (!OnPC) Text("v" + Constantss.Version, -1.055f, 0.36f, 0.011f, true, SecondaryTextColor);
        if (!OnPC) CreateReferences();
        RefreshPage();
    }
    private static void AlignToPalm()
    {
        if (MenuMain == null || GorillaTagger.Instance == null) return;
        Transform hand = GorillaTagger.Instance.leftHandTransform;
        Transform head = GorillaTagger.Instance.headCollider.transform;
        // Match the proven palm-aligned version: orient ONCE when opened, then rotate with wrist.
        Vector3 towardHead = head.position - MenuMain.transform.position;
        towardHead.y = 0f;
        if (towardHead.sqrMagnitude < 0.0001f) return;
        Quaternion initial = Quaternion.LookRotation(Vector3.Cross(towardHead.normalized, Vector3.up), Vector3.up);
        MenuMain.transform.localRotation = Quaternion.Inverse(hand.rotation) * initial;
    }
    private static void SetupDesktopCamera()
    {
        GameObject found = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
        PCamera = found != null ? found.GetComponent<Camera>() : null;
        if (PCamera == null) PCamera = Camera.main;
    }
    private static GameObject Cube(string name, Vector3 position, Vector3 size, Material? material, bool touchable = false)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.layer = 2;
        go.transform.SetParent(MenuMain!.transform, false);
        go.transform.localPosition = position;
        go.transform.localScale = size;
        if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;
        BoxCollider collider = go.GetComponent<BoxCollider>();
        if (touchable) collider.isTrigger = true;
        else Destroy(collider);
        return go;
    }
    private static TextMesh Text(string value, float y, float z, float size, bool back = false, Color? color = null, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        GameObject go = new("Text " + value);
        go.layer = 2;
        go.transform.SetParent(MenuMain!.transform, false);
        go.transform.localPosition = new Vector3(back ? 0.375f : 0.696f, y, z);
        go.transform.localRotation = Quaternion.Euler(0f, back ? 90f : -90f, 0f);
        TextMesh text = go.AddComponent<TextMesh>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 48;
        text.characterSize = size;
        text.text = TranslationManager.UI(value);
        MenuTextOriginal[text] = value;
        text.anchor = anchor;
        text.alignment = anchor == TextAnchor.MiddleLeft ? TextAlignment.Left : TextAlignment.Center;
        text.color = color ?? TextColor;
        return text;
    }
    private static GameObject MakeButton(string label, float y, float z, float width, float height, ButtonManager.ButtonData? button = null, string category = "", bool home = false, bool previous = false, bool next = false, bool disconnect = false, bool search = false, string key = "", Material? material = null, List<GameObject>? track = null)
    {
        if (previous || next) label = previous ? "<" : ">";
        GameObject obj = Cube(label.Length == 0 ? "ChudOS Button" : label, new Vector3(0.63f, y, z), new Vector3(0.06f, height, width), material ?? ButtonMat, true);
        MenuButtonCollider trigger = obj.AddComponent<MenuButtonCollider>();
        trigger.IdleColor = material == HeaderMat ? HeaderColor : ButtonColor;
        trigger.RelatedButton = button;
        trigger.RelatedCategory = category;
        trigger.Home = home;
        trigger.PreviousPage = previous;
        trigger.NextPage = next;
        trigger.Disconnect = disconnect;
        trigger.SearchToggle = search;
        trigger.KeyboardKey = key;
        if (label.Length > 0)
        {
            float textZ = home ? z + 0.085f : z;
            float baseSize = home ? 0.012f : height > 0.19f ? 0.021f : 0.016f;
            TextMesh mesh = Text(label, y, textZ, baseSize);
            FitButtonText(mesh, label, home ? 0.23f : width - 0.06f);
            trigger.Label = mesh;
            trigger.TextWidth = home ? 0.23f : width - 0.06f;
            track?.Add(mesh.gameObject);
        }
        track?.Add(obj);
        return obj;
    }
    public static string GetButtonLabel(ButtonManager.ButtonData button) => (MenuPreferences.IsFavorite(button.Name) ? "[F] " : "") + (button.Category == "Main" && button.Name.StartsWith("Placeholder ", StringComparison.Ordinal) ? "Placeholder" : button.DisplayName);
    public static void FitButtonText(TextMesh mesh, string value, float availableWidth = 0.84f)
    {
        if (mesh == null) return;
        // TextMesh glyph advance at the built-in font's size is roughly 3 local units per characterSize.
        // Shrink the whole label instead of allowing long option/toggle names to overflow the button.
        float baseSize = mesh.name.StartsWith("Text Home", StringComparison.Ordinal) ? 0.012f : 0.021f;
        float fit = availableWidth * 0.88f / Mathf.Max(3f, value.Length * 3f);
        mesh.characterSize = Mathf.Min(baseSize, Mathf.Max(0.006f, fit));
    }
    public static void RefreshPage()
    {
        if (MenuMain == null) return;
        foreach (GameObject go in PageObjects) if (go != null) { if (go.TryGetComponent(out TextMesh label)) MenuTextOriginal.Remove(label); Destroy(go); }
        PageObjects.Clear();
        foreach (GameObject go in SearchObjects) if (go != null) go.SetActive(!SoundboardControlOpen);
        if (SoundboardControlOpen)
        {
            ClearKeyboard();
            BuildSoundboardControl();
            RefreshThemeObjects();
            return;
        }
        if (SearchText != null)
        {
            SearchText.text = SearchManager.Query.Length == 0 ? TranslationManager.T("Search...") : SearchManager.Query;
            SearchText.color = SearchManager.Query.Length == 0 ? SecondaryTextColor : TextColor;
            SearchText.characterSize = Mathf.Min(0.017f, 0.72f / Mathf.Max(3f, SearchText.text.Length * 3f));
        }
        if (CategoryManager.IsSearching || !CategoryManager.IsHome)
        {
            var buttons = CategoryManager.GetVisibleButtons();
            for (int i = 0; i < buttons.Count; i++)
            {
                ButtonManager.ButtonData b = buttons[i];
                MakeButton(GetButtonLabel(b), 0.057f - i * 0.29f, 0f, 0.90f, 0.23f, button: b, track: PageObjects);
            }
            if (buttons.Count == 0)
            {
                string empty = CategoryManager.IsSearching ? "No results" : "No mods yet";
                PageObjects.Add(Text(empty, -0.22f, 0f, 0.019f, color: SecondaryTextColor).gameObject);
            }
        }
        else
        {
            var categories = CategoryManager.GetVisibleCategories();
            for (int i = 0; i < categories.Count; i++) MakeButton(categories[i], 0.057f - i * 0.29f, 0f, 0.90f, 0.23f, category: categories[i], track: PageObjects);
        }
        if (SearchManager.Active && SearchManager.KeyboardVisible) CreateKeyboard();
        else ClearKeyboard();
        RefreshThemeObjects();
    }
    private static void CreateKeyboard()
    {
        if (KeyboardBuilt || MenuMain == null) return;
        KeyboardBuilt = true;
        // Sidecar keyboard (VR pointer and PC mouse use the same menu button colliders).
        // Anchoring to MenuMain also makes it follow the hand, never the HMD camera.
        CubeKeyboard("Keyboard Back", 0.55f, 0f, -1.215f, 1.06f, 1.11f, MenuMat);
        string[] rows = { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };
        for (int r = 0; r < rows.Length; r++)
        {
            string letters = rows[r];
            float step = 0.098f;
            float firstZ = -1.215f - (letters.Length - 1) * step / 2f;
            for (int i = 0; i < letters.Length; i++)
                KeyboardButton(letters[i].ToString(), 0.35f - r * 0.215f, firstZ + i * step, 0.086f);
        }
        KeyboardButton("Space", -0.34f, -1.495f, 0.22f);
        KeyboardButton("Del", -0.34f, -1.287f, 0.16f);
        KeyboardButton("Clear", -0.34f, -1.088f, 0.20f);
        KeyboardButton("Done", -0.34f, -0.864f, 0.20f);
    }
    private static void CubeKeyboard(string name, float depth, float y, float z, float height, float width, Material? mat)
    {
        GameObject go = Cube(name, new Vector3(depth, y, z), new Vector3(0.065f, height, width), mat);
        KeyboardObjects.Add(go);
    }
    private static void KeyboardButton(string key, float y, float z, float width)
    {
        GameObject button = MakeButton(key, y, z, width, 0.17f, key: key, track: KeyboardObjects);
        // The keyboard labels need smaller type than the main three-button layout.
        MenuButtonCollider collider = button.GetComponent<MenuButtonCollider>();
        TextMesh? label = collider != null ? collider.Label : null;
        if (label != null) FitButtonText(label, key, width - 0.02f);
    }
    private static void ClearKeyboard()
    {
        if (!KeyboardBuilt) return;
        foreach (GameObject go in KeyboardObjects) if (go != null) { if (go.TryGetComponent(out TextMesh label)) MenuTextOriginal.Remove(label); Destroy(go); }
        KeyboardObjects.Clear();
        KeyboardBuilt = false;
    }
    public static bool TryGetReferenceHand(Collider other, out bool rightHand)
    {
        rightHand = RightButtonCollider != null && other == RightButtonCollider;
        return rightHand;
    }
    private static void CreateReferences()
    {
        if (RightReference != null || GorillaTagger.Instance == null || OnPC) return;
        RightReference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        RightReference.name = "ChudOS Pointer";
        RightReference.layer = 2;
        RightReference.transform.SetParent(GorillaTagger.Instance.rightHandTransform, false);
        RightReference.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        RightReference.transform.localScale = Vector3.one * 0.012f;
        RightButtonCollider = RightReference.GetComponent<SphereCollider>();
        RightButtonCollider.isTrigger = true;
        Rigidbody rb = RightReference.AddComponent<Rigidbody>();
        rb.isKinematic = true; rb.useGravity = false;
        Renderer renderer = RightReference.GetComponent<Renderer>();
        if (renderer != null && BorderMat != null) renderer.sharedMaterial = BorderMat;
    }
    public static void ScheduleAction(Action action)
    {
        if (Instance != null && !Pending) Instance.StartCoroutine(Delay(action));
    }
    private static IEnumerator Delay(Action action)
    {
        Pending = true;
        yield return new WaitForSecondsRealtime(0.25f);
        if (IsMenuOpen) action();
        Pending = false;
    }
    public static void RemoveMenu()
    {
        if (IsMenuOpen) MenuPreferences.RememberLocation();
        IsMenuOpen = false; NeedsRefresh = false; Pending = false;
        SoundboardControlOpen = false;
        SongTitle = SongArtist = null;
        SongElapsed = SongRemaining = SongStatus = SongPlaybackFallback = null;
        SongPlayIcon = SongPauseIcon = null;
        SongProgress = null;
        SearchObjects.Clear();
        MenuTextOriginal.Clear();
        ClearKeyboard();
        PageObjects.Clear();
        if (MenuMain != null) { MenuMain.SetActive(false); Destroy(MenuMain); }
        SearchManager.Close();
        CategoryManager.GoHome();
        NeedsRefresh = false;
        if (RightReference != null) Destroy(RightReference);
        MenuMain = null; MenuBackground = null; FPS = null; SearchText = null;
        RightReference = null; RightButtonCollider = null;
        PCamera = null;
    }
}
