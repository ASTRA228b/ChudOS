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

using ChudOS.Stuff;
using Photon.Pun;
using UnityEngine;
using System.Collections;
using static ChudOS.Core.Main;

namespace ChudOS.Core.Managers;

// Reuses the pointer/collider interaction model from Astra's Trolling Menu.
public sealed class MenuButtonCollider : MonoBehaviour
{
    public static float ButtonCooldown;
    public ButtonManager.ButtonData? RelatedButton;
    public string RelatedCategory = "";
    public bool Home, NextPage, PreviousPage, Disconnect, SearchToggle;
    public bool Incremental, Positive;
    public string KeyboardKey = "";
    public TextMesh? Label;
    public Action? OnPress;
    public float TextWidth = 0.84f;
    public Color IdleColor = Main.ButtonColorValue;
    private Renderer? Render;
    private MaterialPropertyBlock? Properties;
    private float PressedUntil;

    private Coroutine? pressReset;
    private Color lastColor;
    private bool hasColor;

    private void Awake()
    {
        Render = GetComponent<Renderer>();
        Properties = new MaterialPropertyBlock();
        ButtonManager.OnButtonsChanged += RefreshAppearance;
    }
    private void Start() => RefreshAppearance();
    private void OnDestroy() => ButtonManager.OnButtonsChanged -= RefreshAppearance;

    public void RefreshAppearance()
    {
        if (Render == null || Properties == null) return;
        bool active = RelatedButton?.Type == ButtonType.Toggle && RelatedButton.Enabled;
        Color color = active || Time.unscaledTime < PressedUntil ? Main.PrimaryColorValue : IdleColor;
        if (!hasColor || color != lastColor)
        {
            hasColor = true;
            lastColor = color;
            Properties.SetColor("_Color", color);
            Render.SetPropertyBlock(Properties);
        }
        if (Label == null || RelatedButton == null) return;
        string value = Main.GetButtonLabel(RelatedButton);
        if (Label.text != value) { Label.text = value; Main.FitButtonText(Label, value, TextWidth); }
    }

    private IEnumerator ResetPress()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        pressReset = null;
        RefreshAppearance();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TryGetReferenceHand(other, out bool isRightHand)) Press(isRightHand);
    }

    public void Press(bool? rightHand = null)
    {
        if (MenuMain == null || Time.unscaledTime < ButtonCooldown) return;
        ButtonCooldown = Time.unscaledTime + 0.2f;
        PressedUntil = Time.unscaledTime + 0.25f;
        RefreshAppearance();
        if (pressReset != null) StopCoroutine(pressReset);
        pressReset = StartCoroutine(ResetPress());
        MenuFeedback.Play(rightHand);
        if (OnPress != null) { OnPress(); return; }
        if (Disconnect) { ScheduleAction(() => { if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect(); }); return; }
        if (SearchToggle) { SearchManager.Toggle(); RequestRefresh(); return; }
        if (KeyboardKey.Length > 0) { SearchManager.Key(KeyboardKey); RequestRefresh(); return; }
        if (SearchManager.KeyboardVisible) SearchManager.Blur();
        if (Home) { if (Main.SoundboardControlOpen) Main.CloseSoundboardControl(); else CategoryManager.GoHome(); return; }
        if (Main.SoundboardControlOpen && (NextPage || PreviousPage || SearchToggle)) return;
        if (NextPage) { CategoryManager.NextPage(); return; }
        if (PreviousPage) { CategoryManager.PreviousPage(); return; }
        if (!string.IsNullOrEmpty(RelatedCategory)) { CategoryManager.Open(RelatedCategory); return; }
        if (RelatedButton == null) return;
        bool favorite = rightHand.HasValue ? ControllerInputPoller.instance != null && ChudOS.Libraries.InputLib.RightGrab : UnityEngine.InputSystem.Keyboard.current?.leftShiftKey.isPressed == true || UnityEngine.InputSystem.Keyboard.current?.rightShiftKey.isPressed == true;
        if (favorite) { MenuPreferences.ToggleFavorite(RelatedButton); return; }
        MenuPreferences.ShowHelp(RelatedButton);
        if (Incremental) ButtonManager.ChangeOption(RelatedButton, Positive ? 1 : -1);
        else ButtonManager.Press(RelatedButton);
    }
}
