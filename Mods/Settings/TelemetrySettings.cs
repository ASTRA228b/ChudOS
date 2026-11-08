// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;

namespace ChudOS.Mods.Settings;

public static class TelemetrySettings
{
    [MenuButton("Usage Reporting Info", ButtonType.Press, Category = "Settings", Order = 860, Notify = false)]
    public static void Information()
    {
        NotifyLib.SendNotificationALL($"Usage reporting: {TelemetryManager.Status}. Optional reporting sends random IDs and the menu version. No names, account IDs, room codes, or voice.", 9000);
    }

    [MenuButton("Enable Usage Reporting", ButtonType.Press, Category = "Settings", Order = 861, Notify = false)]
    public static void Enable() => NotifyLib.SendNotificationALL(TelemetryManager.EnableReporting(), 7000);

    [MenuButton("Disable Usage Reporting", ButtonType.Press, Category = "Settings", Order = 862, Notify = false)]
    public static void Disable() => NotifyLib.SendNotificationALL(TelemetryManager.DisableReporting(), 6000);

    [MenuButton("Reset Reporting ID", ButtonType.Press, Category = "Settings", Order = 863, Notify = false)]
    public static void Reset() => NotifyLib.SendNotificationALL(TelemetryManager.ResetIdentity(), 6000);
}
