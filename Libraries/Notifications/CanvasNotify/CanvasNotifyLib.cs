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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ChudOS.Stuff;

namespace ChudOS.Libraries.Notifications.CanvasNotify;

public static class CanvasNotifyLib
{
    // Other
    public static int NotifyThreshold = 30;
    public static string PreviousNotification = "";
    public static readonly int notificationDecayTime = 144;
    private static readonly List<string> Messages = new();
    private static float LastDecay;
    // Object
    private static GameObject? HUDObject;
    private static GameObject? HUDObject2;
    private static Camera? MainCam;
    private static Text? notificationText;
    public static bool Enabled = true;
    public static bool HasMessages => Messages.Count > 0;

    public static void InitCanvas() => Create();
    public static void Create()
    {
        if (HUDObject != null) return;
        MainCam = Camera.main;
        if (MainCam == null) return;
        HUDObject2 = new GameObject("ChudOS_HUDObject_Other");
        HUDObject2.transform.SetParent(MainCam.transform, false);
        HUDObject2.transform.localPosition = new Vector3(0f, 0f, 1.5f);
        HUDObject = new GameObject("ChudOS_HUDObject_Main");
        HUDObject.transform.SetParent(HUDObject2.transform, false);
        Canvas canvas = HUDObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = MainCam;
        RectTransform rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(800f, 350f);
        rect.localScale = Vector3.one * 0.0014f;
        GameObject obj = new GameObject("ChudOS_Notifications");
        obj.transform.SetParent(HUDObject.transform, false);
        notificationText = obj.AddComponent<Text>();
        notificationText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        notificationText.fontSize = 25;
        notificationText.color = Color.white;
        notificationText.alignment = TextAnchor.LowerLeft;
        notificationText.horizontalOverflow = HorizontalWrapMode.Wrap;
        notificationText.verticalOverflow = VerticalWrapMode.Overflow;
        notificationText.rectTransform.sizeDelta = new Vector2(690f, 270f);
        notificationText.rectTransform.anchoredPosition = new Vector2(-45f, -35f);
        Refresh();
        Debug.Log($"[{Constantss.GUID}] Canvas notifications initialized.");
    }
    public static void CanvasUpdate()
    {
        if (!Enabled) return;
        if (HUDObject == null) Create();
        if (Messages.Count > 0 && Time.unscaledTime - LastDecay > notificationDecayTime / 60f)
        {
            Messages.RemoveAt(0);
            LastDecay = Time.unscaledTime;
            Refresh();
        }
    }
    public static void SendCanvasNotify(string Message)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(Message)) return;
        if (Message == PreviousNotification && Time.unscaledTime - LastDecay < 0.25f) return;
        PreviousNotification = Message;
        Messages.Add(Message);
        while (Messages.Count > Mathf.Max(1, NotifyThreshold)) Messages.RemoveAt(0);
        LastDecay = Time.unscaledTime;
        Refresh();
    }
    private static void Refresh()
    {
        if (notificationText != null) notificationText.text = string.Join("\n", Messages);
        if (HUDObject != null && HUDObject.activeSelf != HasMessages) HUDObject.SetActive(HasMessages);
    }
    public static void Clear() { Messages.Clear(); PreviousNotification = ""; Refresh(); }
    public static void ClearOld(int amount) { Messages.RemoveRange(0, Mathf.Min(Mathf.Max(0, amount), Messages.Count)); Refresh(); }
    public static void Shutdown()
    {
        if (HUDObject2 != null) UnityEngine.Object.Destroy(HUDObject2);
        HUDObject = null; HUDObject2 = null; notificationText = null; MainCam = null;
        Messages.Clear();
    }
}
