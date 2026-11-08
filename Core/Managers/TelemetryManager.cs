// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using ChudOS.Libraries.Notifications;
using ChudOS.Stuff;
using Newtonsoft.Json;
using UnityEngine;

namespace ChudOS.Core.Managers;

public sealed class TelemetryManager : MonoBehaviour
{
    private const string Endpoint = "https://chudgpt-landing.vercel.app/api/chudos/v1/heartbeat";
#if CHUDOS_PUBLIC_RELEASE
    private const bool EnabledByDefault = true;
#else
    private const bool EnabledByDefault = false;
#endif
    private static readonly Lazy<HttpClient> Client = new(() => new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false })
    {
        Timeout = TimeSpan.FromSeconds(8),
        MaxResponseContentBufferSize = 1024
    });

    private static TelemetryManager? Instance;

    private ConfigFile Settings = null!;
    private ConfigEntry<bool> Consent = null!;
    private ConfigEntry<string> InstallationId = null!;

    private readonly object State = new();
    private CancellationTokenSource? Operation;

    private volatile bool Reporting;
    private volatile string CurrentStatus = "Disabled";

    public static bool Enabled => Instance != null && Instance.Reporting;
    public static string Status => Instance != null ? Instance.CurrentStatus : "Unavailable";

    public static void Initialize(GameObject host)
    {
        if (Instance != null) return;

        try
        {
            Instance = host.AddComponent<TelemetryManager>();
            Instance.Configure();
        }
        catch (Exception)
        {
            if (Instance != null) Destroy(Instance);
            Instance = null;
            Debug.LogWarning("[ChudOS] Usage reporting could not initialize.");
        }
    }

    private void Configure()
    {
        string path = Path.Combine(Paths.ConfigPath, "ChudOS.Telemetry.cfg");

        Settings = new ConfigFile(path, false) { SaveOnConfigSet = false };
        Consent = Settings.Bind("Telemetry", "Enabled", EnabledByDefault, "Optional usage reporting.");
        InstallationId = Settings.Bind("Telemetry", "InstallationId", "", "Random identifier used only for usage reporting.");

        if (!Consent.Value) return;

        string id = ValidId(InstallationId.Value) ? InstallationId.Value : Guid.NewGuid().ToString("D");
        if (!Save(true, id))
        {
            CurrentStatus = "Disabled; settings could not be saved";
            return;
        }

#if CHUDOS_PUBLIC_RELEASE
        NotifyLib.SendNotificationALL("Usage reporting is on. It sends random reporting IDs and your menu version. Turn it off in Settings > Disable Usage Reporting.", 10000);
#endif
        StartReporting();
    }

    private static bool ValidId(string value)
    {
        return Guid.TryParseExact(value, "D", out _) && value[14] == '4' && "89abAB".IndexOf(value[19]) >= 0;
    }

    private bool Save(bool consent, string id)
    {
        bool previousConsent = Consent.Value;
        string previousId = InstallationId.Value;
        if (previousConsent == consent && previousId == id && File.Exists(Settings.ConfigFilePath)) return true;

        try
        {
            Consent.Value = consent;
            InstallationId.Value = id;
            Settings.Save();
            return true;
        }
        catch (Exception)
        {
            Consent.Value = previousConsent;
            InstallationId.Value = previousId;
            return false;
        }
    }

    private void StartReporting()
    {
        if (Reporting) return;

        byte[] payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
        {
            schemaVersion = 1,
            installationId = InstallationId.Value,
            sessionId = Guid.NewGuid().ToString("D"),
            version = Constantss.Version
        }));

        var operation = new CancellationTokenSource();
        lock (State)
        {
            Operation = operation;
            Reporting = true;
            CurrentStatus = "Waiting to report";
        }

        _ = Task.Run(() => Heartbeat(payload, operation));
    }

    private void StopReporting()
    {
        lock (State)
        {
            Reporting = false;
            var operation = Operation;
            Operation = null;
            operation?.Cancel();
        }
    }

    private static async Task<int> SendHeartbeat(HttpClient client, byte[] payload, CancellationToken token)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            request.Content = new ByteArrayContent(payload);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using var response = await client.SendAsync(request, token).ConfigureAwait(false);
            return (int)response.StatusCode;
        }
        catch (HttpRequestException) { return 0; }
        catch (OperationCanceledException) when (!token.IsCancellationRequested) { return 0; }
    }

    private void SetStatus(CancellationTokenSource operation, string status)
    {
        lock (State)
            if (ReferenceEquals(Operation, operation)) CurrentStatus = status;
    }

    private async Task Heartbeat(byte[] payload, CancellationTokenSource operation)
    {
        CancellationToken token = operation.Token;
        var random = new System.Random();
        int failures = 0;

        try
        {
            await Task.Delay(random.Next(2000, 6000), token).ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                int code = await SendHeartbeat(Client.Value, payload, token).ConfigureAwait(false);
                bool success = code == 200;
                bool rejected = code == 400 || code == 401 || code == 403 || code == 404 || code == 409 || code == 415;

                failures = success ? 0 : Math.Min(failures + 1, 4);
                int delay = rejected ? 900 : success ? 60 : Math.Min(900, 60 * (1 << failures));

                SetStatus(operation, success ? "Reporting normally" : rejected ? "Request rejected; retrying later" : "Service unavailable; retrying later");
                await Task.Delay(delay * 1000 + random.Next(0, 5000), token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception) { SetStatus(operation, "Reporting stopped; enable again to retry"); }
        finally
        {
            lock (State)
            {
                if (ReferenceEquals(Operation, operation))
                {
                    Operation = null;
                    Reporting = false;
                }

                operation.Dispose();
            }
        }
    }

    public static string EnableReporting()
    {
        if (Instance == null) return "Usage reporting is unavailable.";
        if (Instance.Reporting) return "Usage reporting is already enabled.";

        string id = ValidId(Instance.InstallationId.Value) ? Instance.InstallationId.Value : Guid.NewGuid().ToString("D");
        if (!Instance.Save(true, id)) return "Could not save consent. Usage reporting remains off.";

        Instance.StartReporting();
        return "Optional usage reporting enabled. Sends random reporting IDs and the menu version.";
    }

    public static string DisableReporting()
    {
        if (Instance == null) return "Usage reporting is unavailable.";

        Instance.StopReporting();
        Instance.CurrentStatus = "Disabled";
        if (!Instance.Save(false, Instance.InstallationId.Value)) return "Reporting stopped, but saving failed. Check this setting after restarting.";
        return "Usage reporting disabled. Online status expires within 3 minutes.";
    }

    public static string ResetIdentity()
    {
        if (Instance == null) return "Usage reporting is unavailable.";

        bool restart = Instance.Reporting;
        Instance.StopReporting();
        Instance.CurrentStatus = "Disabled";

        string id = restart ? Guid.NewGuid().ToString("D") : "";
        if (!Instance.Save(restart, id)) return "Reporting stopped, but the reset could not be saved. Check settings after restarting.";
        if (restart) Instance.StartReporting();
        return "Reporting ID reset. Previously collected statistics are unchanged.";
    }

    private void OnDestroy()
    {
        StopReporting();
        if (Instance == this) Instance = null;
    }
}
