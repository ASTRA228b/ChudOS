// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.IO;
using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChudOS.Mods.Holdables;

internal sealed class HoldableBrowser : ExpansionWindowFeature
{
    private sealed class Manifest { public string Name = "", Bundle = "", Prefab = ""; }
    private sealed class Entry { internal string Id = "", Name = "", Bundle = "", Prefab = "", Builtin = ""; }
    private sealed class BundleRecord
    {
        internal AssetBundle Bundle = null!;
        internal readonly Dictionary<string, GameObject> Prefabs = new(StringComparer.Ordinal);
    }
    private sealed class Equipped
    {
        internal GameObject Object = null!;
        internal Vector3 Scale;
        internal Renderer[] Renderers = Array.Empty<Renderer>();
    }
    private readonly List<Entry> entries = new();
    private readonly BuiltinHoldables builtin = new();
    private readonly Dictionary<string, BundleRecord> bundles = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> favorites = new(StringComparer.OrdinalIgnoreCase);
    private readonly Equipped?[] equipped = new Equipped?[2];
    private Entry? selected, pendingEntry;
    private GameObject? preview;
    private AssetBundleCreateRequest? bundleRequest;
    private AssetBundleRequest? prefabRequest;
    private BundleRecord? pendingRecord;
    private bool loading, disposed, favoritesOnly;
    private int appearance = -1;
    private Color tint;
    private Vector2 scroll;
    private string status = "";
    private string Folder => Path.Combine(FileManager.RootFolder, "Holdables");
    private string FavoritesPath => Path.Combine(Folder, "favorites.json");

    [MenuButton("Holdable Browser", ButtonType.Toggle, Category = "Holdables", Order = 0, runOption = RunOption.Once, Description = "Equip twelve premade local holdables, including a wooden cross and pride flags. Custom bundles are optional.")]
    public static void EnableBrowser() => Enable<HoldableBrowser>("Holdable Browser");
    [MenuDisable("Holdable Browser")]
    public static void DisableBrowser() => Disable("Holdable Browser");

    protected override void Setup()
    {
        OpenWindow(870300, "ChudOS Holdables"); Reload();
        try
        {
            if (File.Exists(FavoritesPath) && new FileInfo(FavoritesPath).Length < 65536)
                foreach (string id in JsonConvert.DeserializeObject<string[]>(File.ReadAllText(FavoritesPath)) ?? Array.Empty<string>()) if (!string.IsNullOrWhiteSpace(id)) favorites.Add(id);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { status = "Saved holdable favorites could not be loaded."; }
    }
    private void Reload()
    {
        entries.Clear(); selected = null; Remove(preview); preview = null;
        foreach (var model in BuiltinHoldables.Models) entries.Add(new Entry { Id = "builtin:" + model.Id, Name = model.Name, Builtin = model.Id });
        try
        {
            Directory.CreateDirectory(Folder);
            foreach (string path in Directory.EnumerateFiles(Folder, "*.holdable.json").Take(64))
            {
                if (new FileInfo(path).Length > 65536) continue;
                Manifest? manifest;
                try { manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(path)); }
                catch (JsonException) { continue; }
                if (manifest == null || string.IsNullOrWhiteSpace(manifest.Name) || string.IsNullOrWhiteSpace(manifest.Prefab) || !SafeFilename(manifest.Bundle)) continue;
                string bundle = Path.Combine(Folder, manifest.Bundle);
                if (!File.Exists(bundle) || new FileInfo(bundle).Length > 64L * 1024L * 1024L) continue;
                entries.Add(new Entry { Id = Path.GetFileName(path), Name = manifest.Name.Length > 80 ? manifest.Name.Substring(0, 80) : manifest.Name, Bundle = bundle, Prefab = manifest.Prefab });
            }
            entries.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            status = BuiltinHoldables.Models.Length + " premade holdables | " + (entries.Count - BuiltinHoldables.Models.Length) + " custom models.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { status = "The holdables folder could not be read."; }
    }
    internal static bool SafeFilename(string name) => !string.IsNullOrWhiteSpace(name) && name != "." && name != ".." && Path.GetFileName(name) == name && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

    private void Select(Entry entry)
    {
        if (loading || disposed) return;
        selected = pendingEntry = entry;
        if (entry.Builtin.Length > 0) { ShowPreview(builtin.Get(entry.Builtin, transform)); status = entry.Name; return; }
        if (bundles.TryGetValue(entry.Bundle, out var cached)) { LoadPrefab(cached); return; }
        if (bundles.Count >= 4) { status = "Four bundles are cached. Reopen the browser to load another set."; return; }
        try
        {
            loading = true; status = "Loading " + entry.Name + "...";
            bundleRequest = AssetBundle.LoadFromFileAsync(entry.Bundle); bundleRequest.completed += BundleLoaded;
        }
        catch (Exception ex) { loading = false; status = "The bundle could not be opened: " + ex.GetType().Name; }
    }
    private void BundleLoaded(AsyncOperation operation)
    {
        var request = (AssetBundleCreateRequest)operation;
        request.completed -= BundleLoaded; bundleRequest = null;
        var bundle = request.assetBundle;
        if (disposed) { if (bundle != null) bundle.Unload(true); return; }
        if (bundle == null || pendingEntry == null) { loading = false; status = "This bundle is incompatible or invalid."; return; }
        var record = new BundleRecord { Bundle = bundle };
        bundles[pendingEntry.Bundle] = record;
        LoadPrefab(record);
    }
    private void LoadPrefab(BundleRecord record)
    {
        if (pendingEntry == null) { loading = false; return; }
        if (record.Prefabs.TryGetValue(pendingEntry.Prefab, out var cached)) { loading = false; ShowPreview(cached); return; }
        try
        {
            pendingRecord = record; loading = true;
            prefabRequest = record.Bundle.LoadAssetAsync<GameObject>(pendingEntry.Prefab); prefabRequest.completed += PrefabLoaded;
        }
        catch (Exception ex) { pendingRecord = null; loading = false; status = "The prefab could not be opened: " + ex.GetType().Name; }
    }
    private void PrefabLoaded(AsyncOperation operation)
    {
        var request = (AssetBundleRequest)operation;
        request.completed -= PrefabLoaded; prefabRequest = null;
        var record = pendingRecord; pendingRecord = null; loading = false;
        if (disposed) { if (record?.Bundle != null) record.Bundle.Unload(true); return; }
        if (request.asset is not GameObject prefab || record == null || pendingEntry == null) { status = "The named prefab was not found."; return; }
        // This format contains visual prefabs, not game/plugin behaviours.
        if (prefab.GetComponentsInChildren<MonoBehaviour>(true).Length != 0) { status = "Use a visual prefab without script components."; return; }
        record.Prefabs[pendingEntry.Prefab] = prefab;
        ShowPreview(prefab); status = pendingEntry.Name;
    }
    private GameObject Clone(GameObject prefab)
    {
        var staging = new GameObject("ChudOS Holdable Staging"); staging.SetActive(false);
        var obj = Object.Instantiate(prefab, staging.transform); obj.name = "ChudOS Local Holdable";
        foreach (var collider in obj.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        foreach (var body in obj.GetComponentsInChildren<Rigidbody>(true)) { body.isKinematic = true; body.useGravity = false; }
        foreach (var audio in obj.GetComponentsInChildren<AudioSource>(true)) audio.enabled = false;
        foreach (Transform child in obj.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 2;
        obj.transform.SetParent(transform, false); obj.SetActive(true); Object.Destroy(staging); Objects.Add(obj);
        return obj;
    }
    private void Remove(GameObject? obj)
    {
        if (obj == null) return;
        obj.SetActive(false); Objects.Remove(obj); Object.Destroy(obj);
    }
    private void ShowPreview(GameObject prefab)
    {
        Remove(preview); preview = Clone(prefab);
        Bounds bounds = new(preview.transform.position, Vector3.zero);
        foreach (var renderer in preview.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(renderer.bounds);
        float largest = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
        if (largest > 0.001f) preview.transform.localScale *= 0.2f / largest;
    }
    private void Equip()
    {
        if (selected == null) return;
        GameObject? prefab = selected.Builtin.Length > 0 ? builtin.Get(selected.Builtin, transform) : null;
        if (prefab == null && (!bundles.TryGetValue(selected.Bundle, out var record) || !record.Prefabs.TryGetValue(selected.Prefab, out prefab))) return;
        if (prefab == null) return;
        int hand = ExpansionPreferences.LeftHand ? 0 : 1;
        Remove(equipped[hand]?.Object);
        var obj = Clone(prefab);
        equipped[hand] = new Equipped { Object = obj, Scale = prefab.transform.localScale, Renderers = obj.GetComponentsInChildren<Renderer>(true) };
        Remove(preview); preview = null;
        appearance = -1; HoldableFeedback.Pulse(hand == 0);
    }
    protected override void Tick()
    {
        Color color = ExpansionPreferences.HoldableColor;
        bool repaint = appearance != ExpansionPreferences.Revision || tint != color;
        appearance = ExpansionPreferences.Revision; tint = color;
        for (int i = 0; i < equipped.Length; i++)
        {
            var slot = equipped[i];
            if (slot?.Object == null) continue;
            Transform hand = i == 0 ? CreativeTools.Left : CreativeTools.Right;
            slot.Object.transform.SetPositionAndRotation(hand.TransformPoint(ExpansionPreferences.HoldableOffset), hand.rotation * Quaternion.Euler(ExpansionPreferences.HoldableRotation));
            slot.Object.transform.localScale = slot.Scale * ExpansionPreferences.HoldableSize;
            if (repaint) foreach (var renderer in slot.Renderers) if (renderer != null) { if (ExpansionPreferences.OriginalHoldableColors) renderer.SetPropertyBlock(null); else CreativeTools.Paint(renderer, ExpansionPreferences.HoldableColor); }
        }
        if (preview != null) preview.transform.SetPositionAndRotation(CreativeTools.Head.position + CreativeTools.Head.forward * 0.65f - CreativeTools.Head.up * 0.2f, Quaternion.Euler(0f, Time.unscaledTime * 25f, 0f));
    }
    private void Favorite()
    {
        if (selected == null) return;
        if (!favorites.Add(selected.Id)) favorites.Remove(selected.Id);
        try
        {
            string temporary = FavoritesPath + ".tmp";
            File.WriteAllText(temporary, JsonConvert.SerializeObject(favorites.OrderBy(id => id).ToArray(), Formatting.Indented));
            if (File.Exists(FavoritesPath)) File.Replace(temporary, FavoritesPath, null); else File.Move(temporary, FavoritesPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { status = "Favorites could not be saved."; }
    }
    protected override void Window(int id)
    {
        MenuHelper.Label(status); favoritesOnly = MenuHelper.Toggle("Favorites Only", favoritesOnly);
        scroll = GUILayout.BeginScrollView(scroll);
        foreach (var entry in entries)
        {
            if (favoritesOnly && !favorites.Contains(entry.Id)) continue;
            if (MenuHelper.Button((favorites.Contains(entry.Id) ? "* " : "") + entry.Name) && !loading) Select(entry);
        }
        GUILayout.EndScrollView(); GUILayout.BeginHorizontal();
        if (MenuHelper.Button("Equip") && !loading) Equip();
        if (MenuHelper.Button("Favorite")) Favorite();
        if (MenuHelper.Button("Remove")) { int hand = ExpansionPreferences.LeftHand ? 0 : 1; Remove(equipped[hand]?.Object); equipped[hand] = null; }
        GUILayout.EndHorizontal();
        if (MenuHelper.Button("Reload List") && !loading) Reload();
        MenuHelper.Label("Premade models need no bundles. Select left/right placement in Settings."); GUI.DragWindow();
    }
    protected override void Cleanup()
    {
        disposed = true;
        foreach (var slot in equipped) Remove(slot?.Object);
        Remove(preview);
        // An in-flight prefab request retains its bundle until its completion callback releases it.
        foreach (var record in bundles.Values) if (record != pendingRecord && record.Bundle != null) record.Bundle.Unload(true);
        bundles.Clear(); base.Cleanup();
        builtin.Dispose();
    }
}
