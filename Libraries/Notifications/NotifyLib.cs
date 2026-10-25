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

using UnityEngine;
using ChudOS.Libraries.Notifications.CanvasNotify;

namespace ChudOS.Libraries.Notifications;

public static class NotifyLib
{
    public class ToastData
    {
        public string toast = "";
        public long toastUntil;
        internal readonly GUIContent Content = new();
        internal float LayoutWidth = -1f, LayoutHeight;
    }
    private static readonly List<ToastData> Toasts = new();
    public static bool HasToasts => Toasts.Count > 0;

    public static void Tick()
    {
        if (Toasts.Count == 0) return;
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        for (int i = Toasts.Count - 1; i >= 0; i--)
            if (now >= Toasts[i].toastUntil) Toasts.RemoveAt(i);
    }

    public static void InitToast()
    {
        if (Toasts.Count == 0 || Event.current.type != EventType.Repaint) return;
        GUIStylesEX.Init();
        float width = Mathf.Min(320f, Mathf.Max(44f, Screen.width - 44f));
        const float spacing = 8f;
        float x = Screen.width - width - 22f;
        float bottomY = Screen.height - 22f;

        for (int i = Toasts.Count - 1; i >= 0; i--)
        {
            ToastData toast = Toasts[i];
            float textWidth = Mathf.Max(1f, width - 30f);
            if (toast.LayoutWidth != textWidth || toast.Content.text != toast.toast)
            {
                toast.Content.text = toast.toast;
                toast.LayoutWidth = textWidth;
                toast.LayoutHeight = Mathf.Max(54f, Mathf.Ceil(GUIStylesEX.ToastLabel.CalcHeight(toast.Content, textWidth)) + 20f);
            }
            float height = Mathf.Min(toast.LayoutHeight, Mathf.Max(1f, Screen.height - 44f));
            float y = bottomY - height;
            if (y < 22f) break;
            Rect backgroundRect = new(x, y, width, height);
            Rect accentRect = new(x, y, 6f, height);
            Rect textRect = new(x + 20f, y + 10f, textWidth, Mathf.Max(1f, height - 20f));
            GUI.Box(backgroundRect, GUIContent.none, GUIStylesEX.ToastBackground);
            GUI.Box(accentRect, GUIContent.none, GUIStylesEX.AccentBar);
            GUI.Label(textRect, toast.Content, GUIStylesEX.ToastLabel);
            bottomY = y - spacing;
        }
    }

    public static void MessageToast(string message, int durationMilliseconds = 5000)
    {
        Toasts.Add(new ToastData
        {
            toast = message,
            toastUntil = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + durationMilliseconds
        });
    }

    public static void SendNotificationALL(string message, int Mills) // NotifyLib.SendNotificationALL("", 5500);
    {
        MessageToast(message, Mills);
        CanvasNotifyLib.SendCanvasNotify(message);
    }
}
