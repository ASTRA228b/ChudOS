// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using UnityEngine;

namespace ChudOS.Mods.Room;

public sealed class Reauthenticate : MonoBehaviour
{
    private static Reauthenticate? instance;
    private MothershipAuthenticator? authenticator;
    private volatile int result;
    private bool pending, timeoutShown;
    private float deadline;
    private void Awake() => enabled = false;

    [MenuButton("Reauthenticate", ButtonType.Press, Category = "Room", Order = 20, Notify = false, Description = "Retry the game's native Mothership login")]
    public static void Press()
    {
        if (instance == null)
        {
            var main = UnityEngine.Object.FindFirstObjectByType<ChudOS.Core.Main>();
            if (main == null) return;
            instance = main.gameObject.AddComponent<Reauthenticate>();
        }
        instance.Begin();
    }

    private void Begin()
    {
        if (pending) { Notify("Authentication is already running."); return; }
        authenticator = MothershipAuthenticator.Instance;
        if (authenticator == null) { Notify("Authentication is not ready yet."); return; }
        result = 0;
        pending = true;
        timeoutShown = false;
        deadline = Time.unscaledTime + 30f;
        authenticator.OnLoginSuccess += Success;
        authenticator.OnLoginFailure += Failure;
        enabled = true;
        try { Notify("Reauthenticating..."); authenticator.BeginLoginFlow(); }
        catch { Release(); Notify("Could not start authentication."); }
    }
    private void Success() => result = 1;
    private void Failure(string first, string second, string third) => result = 2;
    private void Update()
    {
        if (authenticator == null) { Release(); Notify("Authentication was reset; try again when the game is ready."); return; }
        if (result != 0)
        {
            bool success = result == 1;
            Release();
            Notify(success ? "Authentication completed." : "Authentication failed.");
        }
        else if (!timeoutShown && Time.unscaledTime >= deadline)
        {
            timeoutShown = true;
            Notify("Authentication is still waiting for a response.");
        }
    }
    private void Release()
    {
        if (authenticator != null)
        {
            authenticator.OnLoginSuccess -= Success;
            authenticator.OnLoginFailure -= Failure;
        }
        authenticator = null;
        pending = false;
        enabled = false;
    }
    private void OnDestroy() { Release(); if (instance == this) instance = null; }
    private static void Notify(string text) => NotifyLib.SendNotificationALL(text, 3500);
}
