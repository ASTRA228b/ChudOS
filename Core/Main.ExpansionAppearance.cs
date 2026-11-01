// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using UnityEngine;

namespace ChudOS.Core;

public sealed partial class Main
{
    public static void RefreshExpansionAppearance()
    {
        ExpansionAppearance.Restore();
        if (BackgroundMat != null) BackgroundMat.color = BackgroundColor;
        if (MenuMat != null) MenuMat.color = MenuColor;
        if (ButtonMat != null) ButtonMat.color = ButtonColor;
        if (HeaderMat != null) HeaderMat.color = HeaderColor;
        if (PressedMat != null) PressedMat.color = PrimaryColor;
        if (BorderMat != null) BorderMat.color = BorderColor;
        if (DividerMat != null) DividerMat.color = DividerColor;
        ExpansionAppearance.Apply(new Material?[] { BackgroundMat, MenuMat, ButtonMat, HeaderMat, PressedMat, BorderMat, DividerMat });
        if (MenuMain != null)
        {
            foreach (var text in MenuMain.GetComponentsInChildren<TextMesh>(true)) ExpansionFonts.Apply(text);
            foreach (var marquee in MenuMain.GetComponentsInChildren<SoundboardMarquee>(true)) marquee.ApplyFont();
        }
        ExpansionFonts.RefreshBoards();
    }
}
