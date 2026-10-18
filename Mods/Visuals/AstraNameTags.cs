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

// Port of Astra's NameTags V2 by Astra, revision f14a3af67a0e10424d36b7928429dd51a8721969.
// https://github.com/ASTRA228b/Astras-NameTags-V2
// Uses ChudOS lifecycle/buttons; fixes stale rig cleanup and caches voice lookups.
using System.Reflection;
using ChudOS.Libraries;
using ChudOS.Stuff;
using Photon.Pun;
using Photon.Voice.PUN;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Visuals;

public static class AstraNameTags
{
    private static AstraNameTagRuntime? runtime;

    [MenuButton("Name Tags", ButtonType.Toggle, Category = "Visuals", Order = 100, runOption = RunOption.Once, Description = "Astra names, player colors, FPS and speaking indicators")]
    public static void Enable()
    {
        PortTools.Register(Cleanup);
        if (runtime == null)
        {
            ChudOS.Core.Main main = Object.FindFirstObjectByType<ChudOS.Core.Main>();
            if (main == null) throw new InvalidOperationException("ChudOS runtime is unavailable.");
            runtime = main.GetComponent<AstraNameTagRuntime>() ?? main.gameObject.AddComponent<AstraNameTagRuntime>();
        }
        runtime.enabled = true;
    }

    [MenuDisable("Name Tags")]
    public static void Disable() { if (runtime != null) runtime.enabled = false; }
    private static void Cleanup() { if (runtime != null) Object.Destroy(runtime); runtime = null; }
}

internal sealed class AstraNameTagRuntime : MonoBehaviour
{
    private const float TagHeight = 0.80f, TagScale = 0.5f;
    private readonly Dictionary<VRRig, PlayerTag> tags = new();
    private readonly List<VRRig> removed = new(16);
    private readonly HashSet<VRRig> active = new();
    private static readonly FieldInfo? FpsField = typeof(VRRig).GetField("fps", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private Camera? view;
    private Texture2D? speakerTexture;
    private PhotonVoiceView[] voiceViews = Array.Empty<PhotonVoiceView>();
    private float nextScan, nextTextUpdate, nextVoiceScan;
    private bool wasInRoom;

    private sealed class PlayerTag
    {
        internal GameObject Root = null!;
        internal TextMeshPro Text = null!;
        internal RawImage Icon = null!;
        internal Material? TextMaterial;
        internal PhotonVoiceView? Voice;
        internal NetPlayer? Owner;
        internal float NextVoiceRetry;
        internal string LastText = "";
    }

    private void OnEnable() { nextScan = nextTextUpdate = nextVoiceScan = 0f; }

    private void LateUpdate()
    {
        if (NetworkSystem.Instance == null || !NetworkSystem.Instance.InRoom)
        {
            if (wasInRoom || tags.Count > 0) Clear();
            wasInRoom = false;
            return;
        }
        wasInRoom = true;
        float now = Time.unscaledTime;
        if (view == null) view = Camera.main;
        if (now >= nextScan)
        {
            nextScan = now + 1f;
            RefreshPlayers();
        }
        bool textDue = now >= nextTextUpdate;
        if (textDue) nextTextUpdate = now + 0.25f;
        removed.Clear();
        foreach (var pair in tags)
        {
            VRRig rig = pair.Key;
            PlayerTag tag = pair.Value;
            if (rig == null || tag.Root == null) { removed.Add(rig!); continue; }
            tag.Root.transform.localPosition = Vector3.up * TagHeight;
            if (view != null)
            {
                Vector3 direction = tag.Root.transform.position - view.transform.position;
                if (direction.sqrMagnitude > 0.001f) tag.Root.transform.rotation = Quaternion.LookRotation(direction);
            }
            if (tag.Owner != rig.Creator)
            {
                tag.Owner = rig.Creator;
                tag.Voice = null;
                tag.NextVoiceRetry = 0f;
            }
            if (tag.Voice == null && now >= tag.NextVoiceRetry)
            {
                tag.NextVoiceRetry = now + 1f;
                tag.Voice = FindVoice(rig, now);
            }
            bool speaking = tag.Voice != null && tag.Voice.IsSpeaking;
            tag.Icon.enabled = speaking && speakerTexture != null;
            if (textDue) UpdateText(rig, tag);
        }
        foreach (VRRig rig in removed) Remove(rig);
    }

    private void RefreshPlayers()
    {
        active.Clear();
        if (VRRigCache.ActiveRigs != null)
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig == VRRig.LocalRig || rig.isLocal || rig.isOfflineVRRig) continue;
                active.Add(rig);
                if (!tags.ContainsKey(rig)) tags.Add(rig, CreateTag(rig));
            }
        removed.Clear();
        foreach (var pair in tags)
            if (pair.Key == null || !active.Contains(pair.Key)) removed.Add(pair.Key!);
        foreach (VRRig rig in removed) Remove(rig);
    }

    private PlayerTag CreateTag(VRRig rig)
    {
        LoadIcon();
        GameObject root = new("ChudOS Astra Name Tag");
        root.transform.SetParent(rig.transform, false);
        root.transform.localPosition = Vector3.up * TagHeight;
        root.transform.localScale = Vector3.one * TagScale;
        GameObject label = new("Text");
        label.transform.SetParent(root.transform, false);
        TextMeshPro text = label.AddComponent<TextMeshPro>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 3f;
        text.fontStyle = FontStyles.Bold;
        text.richText = true;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        text.color = Color.white;
        Material? ownedMaterial = text.fontSharedMaterial != null ? new Material(text.fontSharedMaterial) : null;
        if (ownedMaterial != null)
        {
            ownedMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.15f);
            ownedMaterial.EnableKeyword("OUTLINE_ON");
            text.fontSharedMaterial = ownedMaterial;
        }
        text.rectTransform.sizeDelta = new Vector2(8f, 3f);
        GameObject canvasObject = new("Speaker Canvas", typeof(RectTransform));
        canvasObject.transform.SetParent(root.transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
        RectTransform rect = (RectTransform)canvasObject.transform;
        rect.sizeDelta = new Vector2(140f, 80f);
        rect.localScale = Vector3.one * 0.015f;
        rect.localPosition = new Vector3(0f, 0.36f, -0.02f);
        GameObject iconObject = new("Speaker Icon", typeof(RectTransform));
        iconObject.transform.SetParent(canvasObject.transform, false);
        RawImage icon = iconObject.AddComponent<RawImage>();
        icon.texture = speakerTexture;
        icon.raycastTarget = false;
        icon.enabled = false;
        icon.rectTransform.sizeDelta = new Vector2(35f, 35f);
        icon.rectTransform.anchoredPosition = new Vector2(0f, 19f);
        PlayerTag tag = new() { Root = root, Text = text, Icon = icon, TextMaterial = ownedMaterial, Owner = rig.Creator };
        UpdateText(rig, tag);
        return tag;
    }

    private void LoadIcon()
    {
        if (speakerTexture != null) return;
        using Stream? stream = typeof(AstraNameTagRuntime).Assembly.GetManifestResourceStream("ChudOS.Icons.nametags.png");
        if (stream == null) throw new FileNotFoundException("Embedded Astra speaker icon is missing.");
        using MemoryStream memory = new();
        stream.CopyTo(memory);
        Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(memory.ToArray()))
        {
            Object.Destroy(texture);
            throw new InvalidDataException("Cannot decode Astra speaker icon.");
        }
        texture.name = "ChudOS Astra Speaker";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        speakerTexture = texture;
    }

    private PhotonVoiceView? FindVoice(VRRig rig, float now)
    {
        PhotonVoiceView direct = rig.GetComponent<PhotonVoiceView>() ?? rig.GetComponentInChildren<PhotonVoiceView>(true);
        if (direct != null) return direct;
        if (now >= nextVoiceScan)
        {
            nextVoiceScan = now + 1f;
            voiceViews = Object.FindObjectsByType<PhotonVoiceView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }
        foreach (PhotonVoiceView candidate in voiceViews)
        {
            if (candidate == null) continue;
            PhotonView photon = candidate.GetComponent<PhotonView>() ?? candidate.GetComponentInParent<PhotonView>();
            if ((photon != null && rig.Creator != null && photon.OwnerActorNr == rig.Creator.ActorNumber) || candidate.transform.IsChildOf(rig.transform)) return candidate;
        }
        return null;
    }

    private static void UpdateText(VRRig rig, PlayerTag tag)
    {
        string name = rig.playerText1 != null ? rig.playerText1.text : "";
        if (string.IsNullOrWhiteSpace(name)) name = rig.Creator?.NickName ?? "PLAYER";
        name = name.Replace("<", "＜").Replace(">", "＞");
        int fps = -1;
        if (FpsField != null)
        {
            try { object? raw = FpsField.GetValue(rig); if (raw != null) fps = Convert.ToInt32(raw); }
            catch (Exception) { fps = -1; }
        }
        string fpsColor = fps >= 72 ? "55FF55" : fps >= 55 ? "FFD84A" : fps >= 0 ? "FF5555" : "BFBFBF";
        string content = $"<color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>{name}</color>\n" +
            $"<color=#FFFFFF>FPS: </color><color=#{fpsColor}>{(fps >= 0 ? fps.ToString() : "N/A")}</color>";
        if (content == tag.LastText) return;
        tag.LastText = content;
        tag.Text.text = content;
    }

    private void Remove(VRRig rig)
    {
        if (!tags.TryGetValue(rig, out PlayerTag? tag)) return;
        if (tag.Root != null) Object.Destroy(tag.Root);
        if (tag.TextMaterial != null) Object.Destroy(tag.TextMaterial);
        tags.Remove(rig);
    }

    private void Clear()
    {
        foreach (PlayerTag tag in tags.Values)
        {
            if (tag.Root != null) Object.Destroy(tag.Root);
            if (tag.TextMaterial != null) Object.Destroy(tag.TextMaterial);
        }
        tags.Clear(); active.Clear(); removed.Clear();
        voiceViews = Array.Empty<PhotonVoiceView>();
        view = null;
        nextScan = nextVoiceScan = 0f;
    }

    private void OnDisable() => Clear();
    private void OnDestroy()
    {
        Clear();
        if (speakerTexture != null) Object.Destroy(speakerTexture);
        speakerTexture = null;
    }
}
