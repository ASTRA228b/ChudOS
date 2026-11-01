// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System.Text;
using UnityEngine;

namespace ChudOS.Mods.Player;

internal static class PlayerInspectionDetails
{
    internal static string Describe(VRRig rig)
    {
        var text = new StringBuilder(512);
        var player = rig.Creator;
        text.Append("Player: ").Append(player != null ? player.NickName : rig.gameObject.name).AppendLine();
        if (player != null) text.Append("Actor: ").Append(player.ActorNumber).AppendLine();
        Color color = rig.playerColor;
        text.Append("Color: ").Append(color.r.ToString("F2")).Append(", ").Append(color.g.ToString("F2")).Append(", ").Append(color.b.ToString("F2")).AppendLine();
        text.AppendLine("Equipped cosmetics:");
        int count = 0;
        var set = rig.cosmeticSet;
        if (set != null && set.items != null) foreach (var item in set.items)
        {
            if (string.IsNullOrWhiteSpace(item.displayName)) continue;
            text.Append("- ").AppendLine(item.displayName);
            if (++count >= 24) break;
        }
        if (count == 0) text.Append("No equipped cosmetic data is available.");
        return text.ToString();
    }
    internal static bool HasCosmetics(VRRig rig)
    {
        var set = rig.cosmeticSet;
        if (set == null || set.items == null) return false;
        foreach (var item in set.items) if (!string.IsNullOrWhiteSpace(item.displayName)) return true;
        return false;
    }
    internal static bool IsInfected(VRRig rig)
    {
        var manager = GorillaGameManager.instance as GorillaTagManager;
        return manager != null && rig.Creator != null && manager.currentInfected.Contains(rig.Creator);
    }
}
