// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.IO;
using ChudOS.Core.AGUI.GUIHelpers;
using ChudOS.Core.Managers;
using ChudOS.Libraries;
using GorillaNetworking;
using Newtonsoft.Json;
using Photon.Pun;
using UnityEngine;

namespace ChudOS.Mods.Room;

internal sealed class RoomNotebook : MonoBehaviour
{
    private sealed class Data { public List<string> Favorites = new(), History = new(); }
    private static RoomNotebook? instance;
    private Data data = new();
    private float nextPoll;
    private string last = "", error = "";
    private static bool visible;
    private static int tab;
    private static Rect window = new(100, 100, 370, 420);
    private static Vector2 scroll;
    private string PathName => Path.Combine(FileManager.RootFolder, "room-notebook.json");
    internal static void Initialize(GameObject host)
    {
        if (instance != null) return;
        instance = host.AddComponent<RoomNotebook>(); instance.Load();
        GUIManager.Register("Room Notebook", v => visible = v, () => visible, CreateGUI, () => window);
    }
    internal static void Open(bool history) { tab = history ? 1 : 0; visible = true; }
    internal static void CopyCode()
    {
        if (PhotonNetwork.CurrentRoom == null) { CreativeTools.Notice("Join a room first."); return; }
        GUIUtility.systemCopyBuffer = PhotonNetwork.CurrentRoom.Name; CreativeTools.Notice("Room code copied.");
    }
    internal static void JoinClipboard() => Join(GUIUtility.systemCopyBuffer.Trim().ToUpperInvariant());
    private static void Join(string code)
    {
        if (!ValidCode(code)) { CreativeTools.Notice("Use a room code containing 1–12 letters or numbers."); return; }
        if (PhotonNetworkController.Instance == null) { CreativeTools.Notice("The room controller is not ready."); return; }
        PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(code, JoinType.Solo);
    }
    private static bool ValidCode(string code) => code.Length is >= 1 and <= 12 && code.All(c => c is >= 'A' and <= 'Z' or >= '0' and <= '9');
    private void Update()
    {
        if (Time.unscaledTime < nextPoll) return;
        nextPoll = Time.unscaledTime + 1f;
        string current = PhotonNetwork.CurrentRoom?.Name ?? "";
        if (current == last) return;
        last = current;
        if (!ValidCode(current)) return;
        data.History.Remove(current); data.History.Insert(0, current);
        if (data.History.Count > 30) data.History.RemoveRange(30, data.History.Count - 30);
        Save();
    }
    private void Load()
    {
        try
        {
            if (File.Exists(PathName) && new FileInfo(PathName).Length < 65536)
            {
                data = JsonConvert.DeserializeObject<Data>(File.ReadAllText(PathName)) ?? new();
                data.Favorites = (data.Favorites ?? new()).Where(ValidCode).Distinct().Take(30).ToList();
                data.History = (data.History ?? new()).Where(ValidCode).Distinct().Take(30).ToList();
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { error = "Couldn't read saved rooms."; data = new(); }
    }
    private void Save()
    {
        try
        {
            Directory.CreateDirectory(FileManager.RootFolder);
            string temporary = PathName + ".tmp"; File.WriteAllText(temporary, JsonConvert.SerializeObject(data, Formatting.Indented));
            if (File.Exists(PathName)) File.Replace(temporary, PathName, null); else File.Move(temporary, PathName);
            error = "";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { error = "Couldn't save the room notebook."; }
    }
    private static void CreateGUI() { if (visible && instance != null) window = MenuHelper.MakeWindow(854100, window, instance.Contents, "ChudOS Rooms"); }
    private void Contents(int id)
    {
        tab = MenuHelper.Toolbar(tab, new[] { "Favorites", "History" });
        string current = PhotonNetwork.CurrentRoom?.Name ?? "";
        MenuHelper.Label("Current: " + (current.Length > 0 ? current : "Not in a room"));
        if (ValidCode(current) && data.Favorites.Count < 30 && MenuHelper.Button("Favorite Current Room")) { if (!data.Favorites.Contains(current)) data.Favorites.Add(current); Save(); }
        scroll = GUILayout.BeginScrollView(scroll);
        var list = tab == 0 ? data.Favorites : data.History;
        foreach (string code in list.ToArray())
        {
            GUILayout.BeginHorizontal(); if (MenuHelper.Button("Join: " + code)) Join(code);
            if (tab == 0 && MenuHelper.Button("Remove")) { data.Favorites.Remove(code); Save(); }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        if (error.Length > 0) MenuHelper.Label(error);
        if (MenuHelper.Button("Close")) visible = false;
        GUI.DragWindow();
    }
    private void OnDestroy() { if (instance == this) { GUIManager.Unregister("Room Notebook"); instance = null; visible = false; } }
}
