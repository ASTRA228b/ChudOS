// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

using ChudOS.Core.Managers;
using ChudOS.Libraries;
using ChudOS.Mods.Settings;
using ChudOS.Stuff;
using UnityEngine;
using UnityEngine.Rendering;

namespace ChudOS.Mods.Visuals;

public static class SmoothDesktopCamera
{
    private static Camera? source;
    private static Vector3 position, velocity, originalPosition;
    private static Quaternion rotation, originalRotation;
    private static bool subscribed, sampled, applied;
    private static int frame = -1;
    [MenuButton("Smooth Desktop Camera", ButtonType.Toggle, Category = "Visuals", Order = 403, runOption = RunOption.Once, Description = "Smooth the game's desktop camera without changing VR head tracking")]
    public static void Enable()
    {
        Stop();
        if (GorillaTagger.Instance != null && GorillaTagger.Instance.thirdPersonCamera != null) source = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>(true);
        if (source == null || source.stereoEnabled)
        {
            CreativeTools.Notice("A non-stereo desktop camera is not available yet.");
            ButtonManager.SetEnabled("Smooth Desktop Camera", false);
            return;
        }
        ButtonManager.SetEnabled("Free Camera", false);
        Camera.onPreCull += Before; Camera.onPostRender += After;
        RenderPipelineManager.beginCameraRendering += BeginRender; RenderPipelineManager.endCameraRendering += EndRender;
        subscribed = true;
        PortTools.Register(Stop, WatchCamera);
    }
    private static void WatchCamera()
    {
        if (source == null) { ButtonManager.SetEnabled("Smooth Desktop Camera", false); return; }
        if (applied) After(source);
    }
    private static void BeginRender(ScriptableRenderContext context, Camera camera) => Before(camera);
    private static void EndRender(ScriptableRenderContext context, Camera camera) => After(camera);
    private static void Before(Camera camera)
    {
        if (camera != source || camera.stereoEnabled || applied) return;
        originalPosition = camera.transform.position; originalRotation = camera.transform.rotation;
        if (frame != Time.frameCount)
        {
            float dt = Mathf.Clamp(Time.unscaledDeltaTime, 0.001f, 0.1f);
            if (!sampled || Vector3.Distance(position, originalPosition) > 3f) { position = originalPosition; rotation = originalRotation; velocity = Vector3.zero; sampled = true; }
            else
            {
                position = Vector3.SmoothDamp(position, originalPosition, ref velocity, CreativeSettings.CameraTime, Mathf.Infinity, dt);
                rotation = Quaternion.Slerp(rotation, originalRotation, 1f - Mathf.Exp(-dt / CreativeSettings.CameraTime));
            }
            frame = Time.frameCount;
        }
        camera.transform.SetPositionAndRotation(position, rotation);
        applied = true;
    }
    private static void After(Camera camera)
    {
        if (camera != source || !applied) return;
        camera.transform.SetPositionAndRotation(originalPosition, originalRotation);
        applied = false;
    }
    [MenuEnable("Free Camera")]
    public static void FreeCameraEnabled() => ButtonManager.SetEnabled("Smooth Desktop Camera", false);
    [MenuDisable("Smooth Desktop Camera")]
    public static void Stop()
    {
        if (applied && source != null) source.transform.SetPositionAndRotation(originalPosition, originalRotation);
        if (subscribed)
        {
            Camera.onPreCull -= Before; Camera.onPostRender -= After;
            RenderPipelineManager.beginCameraRendering -= BeginRender; RenderPipelineManager.endCameraRendering -= EndRender;
        }
        PortTools.RemoveTick(WatchCamera);
        source = null; subscribed = sampled = applied = false; velocity = Vector3.zero; frame = -1;
    }
}
