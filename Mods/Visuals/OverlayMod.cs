// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Stuff;
using Photon.Pun;
using TMPro;
using UnityEngine;

using MenuRuntime = ChudOS.Core.Main;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Visuals;

public static class OverlayMod
{
    private static GameObject? root;
    private static Camera? camera;
    private static readonly TextMeshProUGUI?[] labels = new TextMeshProUGUI?[5];
    private static readonly RectTransform?[] panels = new RectTransform?[2];
    private static readonly string[] settings = { "Overlay FPS", "Overlay Velocity", "Overlay Ping", "Overlay Room", "Overlay Players" };
    private static readonly bool[] visible = { true, true, true, true, true };
    private static TMP_FontAsset? font;
    private static Font? sourceFont;
    private static bool ownsFont;
    private static float nextUpdate, nextCameraScan, smoothDelta, size = 1f;
    private static int position;

    [MenuButton("Overlay", ButtonType.Toggle, Category = "Visuals", Order = 100, runOption = RunOption.LateUpdate, Description = "Themed FPS, velocity and room information")]
    public static void Tick()
    {
        float now = Time.unscaledTime;
        if (camera == null || !camera.isActiveAndEnabled || now >= nextCameraScan)
        {
            nextCameraScan = now + 1f;
            Camera current = Camera.main;
            if (camera != current)
            {
                RemoveCanvas();
                camera = current;
            }
        }
        if (camera == null || !camera.isActiveAndEnabled) return;
        float delta = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        smoothDelta = smoothDelta <= 0f ? delta : Mathf.Lerp(smoothDelta, delta, 0.1f);
        if (root == null) Create();
        if (now < nextUpdate) return;
        nextUpdate = now + 0.1f;
        Refresh();
    }

    private static void Create()
    {
        if (camera == null) return;
        root = new GameObject("ChudOS Overlay", typeof(RectTransform), typeof(Canvas));
        root.transform.SetParent(camera.transform, false);
        root.transform.localPosition = Vector3.forward;
        root.transform.localScale = Vector3.one * 0.00075f * size;
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(1600f, 900f);
        sourceFont = Font.CreateDynamicFontFromOSFont("Georgia", 48) ?? Font.CreateDynamicFontFromOSFont("Arial", 48);
        if (sourceFont != null) { font = TMP_FontAsset.CreateFontAsset(sourceFont); ownsFont = font != null; }
        font ??= TMP_Settings.defaultFontAsset;
        for (int side = 0; side < 2; side++)
        {
            var panel = new GameObject("Overlay Group", typeof(RectTransform));
            panel.transform.SetParent(root.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = side == 0 ? new Vector2(0f, 0f) : new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(side == 0 ? 20f : -20f, 20f);
            rect.sizeDelta = new Vector2(650f, side == 0 ? 160f : 240f);
            panels[side] = rect;

        }
        for (int i = 0; i < labels.Length; i++)
        {
            int side = i < 2 ? 0 : 1;
            var obj = new GameObject("Overlay " + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            obj.transform.SetParent(panels[side]!.transform, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(0f, -(side == 0 ? i : i - 2) * 80f);
            rect.sizeDelta = new Vector2(650f, 80f);
            var text = obj.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = 43f;
            text.outlineWidth = 0.12f;
            text.outlineColor = new Color(0f, 0f, 0f, 0.9f);
            text.fontStyle = FontStyles.Bold;
            text.alignment = side == 0 ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.TopRight;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.richText = false;
            text.raycastTarget = false;
            labels[i] = text;
        }
        Refresh();
    }

    private static void SetText(int index, string text)
    {
        var label = labels[index];
        if (label != null && label.text != text) label.text = text;
    }

    private static void Refresh()
    {
        if (root == null) return;
        root.transform.localScale = Vector3.one * 0.00075f * size;
        var rb = GorillaTagger.Instance == null ? null : GorillaTagger.Instance.rigidbody;
        float speed = rb == null ? 0f : rb.linearVelocity.magnitude;
        var room = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom : null;
        SetText(0, TranslationManager.T("FPS") + ": " + Mathf.RoundToInt(1f / Mathf.Max(smoothDelta, 0.0001f)));
        SetText(1, TranslationManager.T("Velocity") + $": {speed:F1} m/s");
        SetText(2, TranslationManager.T("Ping") + ": " + (room == null ? "--" : PhotonNetwork.GetPing() + " ms"));
        SetText(3, TranslationManager.T("Room") + ": " + (room == null ? TranslationManager.T("Not In Room") : room.Name));
        SetText(4, TranslationManager.T("Players") + ": " + (room == null ? "0/0" : room.PlayerCount + "/" + room.MaxPlayers));
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] == null) continue;
            labels[i]!.gameObject.SetActive(visible[i]);
            labels[i]!.color = i == 1 ? VelocityColor(speed) : i == 0 || i == 2 ? MenuRuntime.PrimaryColorValue : Color.white;
        }
        for (int side = 0; side < panels.Length; side++)
        {
            var panel = panels[side];
            if (panel == null) continue;
            panel.gameObject.SetActive(side == 0 ? visible[0] || visible[1] : visible[2] || visible[3] || visible[4]);
            var rect = panel;
            bool top = position == 0;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(side, top ? 1f : 0f);
            rect.anchoredPosition = new Vector2(side == 0 ? 55f : -55f, top ? -55f : 55f);
        }
    }

    private static Color VelocityColor(float speed) => speed <= 20f ? Color.green : speed <= 50f ? Color.yellow : Color.red;

    [MenuDisable("Overlay")]
    public static void Shutdown()
    {
        RemoveCanvas();
        camera = null;
        nextUpdate = nextCameraScan = smoothDelta = 0f;
    }

    private static void RemoveCanvas()
    {
        if (root != null) { root.SetActive(false); Object.Destroy(root); }
        root = null;
        Array.Clear(labels, 0, labels.Length);
        Array.Clear(panels, 0, panels.Length);
        if (ownsFont && font != null)
        {
            if (font.material != null) Object.Destroy(font.material);
            foreach (var atlas in font.atlasTextures)
                if (atlas != null) Object.Destroy(atlas);
            Object.Destroy(font);
        }
        if (sourceFont != null) Object.Destroy(sourceFont);
        font = null;
        sourceFont = null;
        ownsFont = false;
    }

    [MenuButton("Overlay Size", ButtonType.Option, Category = "Settings", Order = 140, Options = new[] { "Medium", "Small", "Large" }, Notify = false)]
    public static void SetSize() { size = new[] { 1f, 0.8f, 1.2f }[ButtonManager.GetButton("Overlay Size")?.OptionIndex ?? 0]; Refresh(); }

    [MenuButton("Overlay Position", ButtonType.Option, Category = "Settings", Order = 141, Options = new[] { "Top", "Bottom" }, Notify = false)]
    public static void SetPosition() { position = ButtonManager.GetButton("Overlay Position")?.OptionIndex ?? 0; Refresh(); }

    private static void SetVisible(int index) { visible[index] = (ButtonManager.GetButton(settings[index])?.OptionIndex ?? 0) == 0; Refresh(); }

    [MenuButton("Overlay FPS", ButtonType.Option, Category = "Settings", Order = 142, Options = new[] { "ON", "OFF" }, Notify = false)]
    public static void SetFPS() => SetVisible(0);
    [MenuButton("Overlay Velocity", ButtonType.Option, Category = "Settings", Order = 143, Options = new[] { "ON", "OFF" }, Notify = false)]
    public static void SetVelocity() => SetVisible(1);
    [MenuButton("Overlay Ping", ButtonType.Option, Category = "Settings", Order = 144, Options = new[] { "ON", "OFF" }, Notify = false)]
    public static void SetPing() => SetVisible(2);
    [MenuButton("Overlay Room", ButtonType.Option, Category = "Settings", Order = 145, Options = new[] { "ON", "OFF" }, Notify = false)]
    public static void SetRoom() => SetVisible(3);
    [MenuButton("Overlay Players", ButtonType.Option, Category = "Settings", Order = 146, Options = new[] { "ON", "OFF" }, Notify = false)]
    public static void SetPlayers() => SetVisible(4);
}
