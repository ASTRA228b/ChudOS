// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Mods.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace ChudOS.Core.Managers;

internal sealed class SoundboardMarquee : MonoBehaviour
{
    private const float TextScale = 0.0021f;
    private const float StartDelay = 1.5f, EndDelay = 1f;
    private Text label = null!;
    private RectTransform content = null!;
    private float viewportWidth, viewportHeight, overflow, started, lastOffset = -1f;
    private string songKey = "", title = "";
    private bool measurePending, initialized;

    internal static SoundboardMarquee Create(Transform parent, Vector3 position, Color color, float width = 0.9f, float height = 0.12f, float textScale = TextScale)
    {
        var root = new GameObject("Soundboard Title", typeof(RectTransform), typeof(Canvas), typeof(RectMask2D));
        root.layer = 2;
        var rect = root.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.localPosition = position;
        rect.localRotation = Quaternion.Euler(0f, -90f, 0f);
        rect.localScale = Vector3.one * textScale;
        rect.sizeDelta = new Vector2(width / textScale, height / textScale);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = false;

        var obj = new GameObject("Title Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        obj.layer = 2;
        var content = obj.GetComponent<RectTransform>();
        content.SetParent(rect, false);
        var text = obj.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 48;
        text.fontStyle = FontStyle.Normal;
        text.color = color;
        text.supportRichText = false;
        text.resizeTextForBestFit = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;

        var marquee = root.AddComponent<SoundboardMarquee>();
        marquee.label = text;
        marquee.content = content;
        marquee.viewportWidth = rect.sizeDelta.x;
        marquee.viewportHeight = rect.sizeDelta.y;
        marquee.enabled = false;
        return marquee;
    }

    private void Awake()
    {
        Font.textureRebuilt += FontRebuilt;
        SoundboardTextSettings.Changed += SettingsChanged;
    }
    private void OnDestroy()
    {
        Font.textureRebuilt -= FontRebuilt;
        SoundboardTextSettings.Changed -= SettingsChanged;
    }
    private void FontRebuilt(Font font)
    {
        if (label == null || label.font != font) return;
        measurePending = true;
        enabled = true;
    }
    private void SettingsChanged()
    {
        if (label == null) return;
        started = Time.unscaledTime;
        lastOffset = -1f;
        Measure();
    }
    internal void SetSong(string key, string value)
    {
        value ??= "";
        key ??= "";
        if (initialized && songKey == key && title == value) return;
        initialized = true;
        songKey = key;
        title = value;
        label.text = value;
        started = Time.unscaledTime;
        lastOffset = -1f;
        Measure();
    }
    private void Measure()
    {
        measurePending = false;
        float width = Mathf.Ceil(label.preferredWidth);
        overflow = Mathf.Max(0f, width - viewportWidth);
        content.localScale = Vector3.one;
        if (overflow <= 1f || !SoundboardTextSettings.Scrolling)
        {
            overflow = 0f;
            content.anchorMin = content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot = new Vector2(0.5f, 0.5f);
            content.sizeDelta = new Vector2(Mathf.Max(viewportWidth, width), viewportHeight);
            content.localScale = Vector3.one * Mathf.Min(1f, viewportWidth / Mathf.Max(1f, width));
            content.anchoredPosition = Vector2.zero;
            label.alignment = TextAnchor.MiddleCenter;
            enabled = measurePending;
            return;
        }
        content.anchorMin = content.anchorMax = new Vector2(0f, 0.5f);
        content.pivot = new Vector2(0f, 0.5f);
        content.sizeDelta = new Vector2(width, viewportHeight);
        label.alignment = TextAnchor.MiddleLeft;
        lastOffset = -1f;
        enabled = true;
        Move();
    }
    private void LateUpdate()
    {
        if (measurePending) Measure();
        if (overflow > 0f) Move();
    }
    internal static float Offset(float elapsed, float width, float speed)
    {
        if (width <= 0f || speed <= 0f) return 0f;
        float duration = width / speed;
        float phase = Mathf.Repeat(Mathf.Max(0f, elapsed), StartDelay + duration + EndDelay);
        return Mathf.Clamp((phase - StartDelay) * speed, 0f, width);
    }
    private void Move()
    {
        float offset = Offset(Time.unscaledTime - started, overflow, SoundboardTextSettings.Speed);
        if (Mathf.Abs(offset - lastOffset) < 0.01f) return;
        content.anchoredPosition = new Vector2(-offset, 0f);
        lastOffset = offset;
    }
}
