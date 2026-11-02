// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.AGUI;
using ChudOS.Core.Managers;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;

namespace ChudOS.Mods.Settings;

public static class CustomThemeSettings
{
    [MenuButton("Custom Theme Editor", ButtonType.Press, Category = "Settings", Order = 840, Notify = false)]
    public static void Open() => CustomThemeGUI.SetVisible(!CustomThemeGUI.Visible);
    [MenuButton("Next Custom Theme", ButtonType.Press, Category = "Settings", Order = 841, Notify = false)]
    public static void Next() => NotifyLib.SendNotificationALL(CustomThemeManager.Next(), 3500);
    [MenuButton("Reload Custom Themes", ButtonType.Press, Category = "Settings", Order = 842, Notify = false)]
    public static void Reload() => NotifyLib.SendNotificationALL(CustomThemeManager.Reload(), 3500);
}
