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

using ChudOS.Libraries;
using ChudOS.Stuff;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChudOS.Mods.Movement;


public static class MovementMods
{

    private static bool DashWasHeld;
    private static float flyspeed = 15f;
    private static float LastSpawn;
    private static GameObject? LeftPlatform;
    private static GameObject? RightPlatform;
    private static bool Leftspawn;
    private static bool Rightspawn;
    public static float FlySpeed = 10f;
    public static float ShiftMultiplier = 2.5f;
    public static float MouseSensitivity = 0.15f;

    private static float Pitch;
    private static bool Looking;
    private static CursorLockMode PreviousLock;
    private static bool PreviousVisibility;

    [MenuButton("Speed Boost", ButtonType.Toggle, Category = "Movement", Order = 0, runOption = RunOption.FixedUpdate, Description = "Increase jump speed while enabled")]
    public static void SpeedBoost()
    {
        GTPlayer player = GTPlayer.Instance;
        if (player == null) return;
        player.maxJumpSpeed = 8.5f;
        player.jumpMultiplier = 1.5f;
    }


    [MenuButton("Low Gravity", ButtonType.Toggle, Category = "Movement", Order = 1, runOption = RunOption.FixedUpdate, Description = "Reduce gravity locally")]
    public static void LowGravity()
    {
        GorillaTagger.Instance.rigidbody.AddForce(Vector3.up * 6.66f, ForceMode.Acceleration);
    }

    [MenuButton("High Gravity", ButtonType.Toggle, Category = "Movement", Order = 2, runOption = RunOption.FixedUpdate, Description = "Increase gravity locally")]
    public static void HighGravity()
    {
        GorillaTagger.Instance.rigidbody.AddForce(Vector3.down * 7.77f, ForceMode.Acceleration);
    }

    [MenuButton("Zero Gravity", ButtonType.Toggle, Category = "Movement", Order = 3, runOption = RunOption.FixedUpdate, Description = "Counteract default gravity")]
    public static void ZeroGravity()
    {
        GorillaTagger.Instance.rigidbody.AddForce(-Physics.gravity, ForceMode.Acceleration);
    }

    [MenuButton("Dash (X)", ButtonType.Toggle, Category = "Movement", Order = 4, runOption = RunOption.FixedUpdate, Description = "Press X to dash in your head's forward direction")]
    public static void Dash()
    {
        if (ControllerInputPoller.instance == null) { DashWasHeld = false; return; }
        bool pressed = InputLib.LeftControllerXButton;
        if (pressed && !DashWasHeld && GTPlayer.Instance != null && GorillaTagger.Instance != null && GorillaTagger.Instance.rigidbody != null)
            GorillaTagger.Instance.rigidbody.linearVelocity += GTPlayer.Instance.headCollider.transform.forward.normalized * 12f;
        DashWasHeld = pressed;
    }

    [MenuDisable("Dash (X)")]
    public static void DisableDash() => DashWasHeld = false;

    [MenuButton("Fly (X)", ButtonType.Toggle, Category = "Movement", Order = 5, runOption = RunOption.FixedUpdate, Description = "Press X to go weee.")]
    public static void Fly()
    {
        if (InputLib.LeftControllerXButton)
        {
            GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * flyspeed;
            GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
        }
    }

    [MenuButton("Slingshot Fly (A)", ButtonType.Toggle, Category = "Movement", Order = 6, runOption = RunOption.FixedUpdate, Description = "fly like slingshot.")]
    public static void UpdateSlingshotFly()
    {
        if (InputLib.RightControllerAButton && GTPlayer.Instance != null && GorillaTagger.Instance != null)
            GorillaTagger.Instance.rigidbody.linearVelocity += GTPlayer.Instance.headCollider.transform.forward * (50f * Time.fixedDeltaTime);
    }

    [MenuButton("Up And Down", ButtonType.Toggle, Category = "Movement", Order = 7, runOption = RunOption.FixedUpdate, Description = "monke go up and down.")]
    public static void UpdateUpAndDown()
    {
        if (GorillaTagger.Instance == null) return;
        Rigidbody rb = GorillaTagger.Instance.rigidbody;
        if (InputLib.LeftTrigger) rb.AddForce(Vector3.up * 50f, ForceMode.Acceleration);
        if (InputLib.RightTrigger) rb.AddForce(Vector3.down * 50f, ForceMode.Acceleration);
    }

    [MenuButton("Iron Man", ButtonType.Toggle, Category = "Movement", Order = 8, runOption = RunOption.FixedUpdate, Description = "become IRON MAN.")]
    public static void UpdateIronMan()
    {
        if (GorillaTagger.Instance == null) return;
        Rigidbody rb = GorillaTagger.Instance.rigidbody;
        const float speed = 37.5f;

        if (InputLib.LeftGrab)
            rb.AddForce(-GorillaTagger.Instance.leftHandTransform.right * speed * Time.fixedDeltaTime, ForceMode.VelocityChange);
        if (InputLib.RightGrab)
            rb.AddForce(GorillaTagger.Instance.rightHandTransform.right * speed * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    [MenuButton("Trigger Fly", ButtonType.Toggle, Category = "Movement", Order = 9)]
    public static void TriggerFly()
    {
        if (InputLib.LeftTrigger)
        {
            GorillaTagger.Instance.rigidbody.linearVelocity = GorillaTagger.Instance.headCollider.transform.forward * flyspeed;
        }
    }

    [MenuButton("Joystick Fly (LJ)", ButtonType.Toggle, Category = "Movement", Order = 10)]
    public static void JoyFly()
    {
        Vector2 joy = InputLib.LeftJoyStickAxis;
        if (Mathf.Abs(joy.x) > 0.3 || Mathf.Abs(joy.y) > 0.3)
        {
            Vector3 M = GorillaTagger.Instance.headCollider.transform.forward * joy.y + GorillaTagger.Instance.headCollider.transform.right * joy.x;
            GorillaTagger.Instance.rigidbody.linearVelocity = M.normalized * flyspeed;
        }
    }

    [MenuButton("Zero Grav Fly (A)", ButtonType.Toggle, Category = "Movement", Order = 11)]
    public static void ZGFly()
    {
        if (InputLib.RightControllerAButton)
        {
            GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * flyspeed;
            GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            ZeroGravity();
        }
    }

    [MenuButton("BarkFly (JR)(JL)", ButtonType.Toggle, Category = "Movement", Order = 12)]
    public static void BarkFly()
    {
        Vector3 JoyL = InputLib.LeftJoyStickAxis;
        Vector3 JoyR = InputLib.RightJoyStickAxis;

        Vector3 InputDirection = new(JoyL.x, JoyR.y, JoyL.y);
        Vector3 PlayerF = GTPlayer.Instance.bodyCollider.transform.forward.X_Z();
        Vector3 PlayerR = GTPlayer.Instance.bodyCollider.transform.right.X_Z();
        Vector3 Vel = InputDirection.x * PlayerR + InputDirection.y * Vector3.up + InputDirection.z * PlayerF;
        Vel *= flyspeed;
        GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.Lerp(GorillaTagger.Instance.rigidbody.linearVelocity, Vel, 0.12885f);
        ZeroGravity();
    }

    [MenuButton("Velocity BarkFly (JR)(JL)", ButtonType.Toggle, Category = "Movement", Order = 13)]
    public static void VelBark()
    {
        Vector3 JoyL = InputLib.LeftJoyStickAxis;
        Vector3 JoyR = InputLib.RightJoyStickAxis;
        if (Mathf.Abs(JoyL.x) > 0.3 || Mathf.Abs(JoyL.y) > 0.3 || Mathf.Abs(JoyR.x) > 0.3 || Mathf.Abs(JoyR.y) > 0.3)
            BarkFly();
    }

    [MenuButton("Hand Fly (B)", ButtonType.Toggle, Category = "Movement", Order = 14)]
    public static void HandFly()
    {
        if (InputLib.RightControllerBButton)
        {
            GorillaTagger.Instance.rigidbody.linearVelocity = GTPlayer.Instance.RightHand.controllerTransform.forward * flyspeed;
        }
    }

    [MenuButton("NoGrav SlingshotFly (B)", ButtonType.Toggle, Category = "Movement", Order = 15)]
    public static void GravSling()
    {
        if (InputLib.RightControllerBButton)
        {
            ZeroGravity();
            GorillaTagger.Instance.rigidbody.linearVelocity += GTPlayer.Instance.headCollider.transform.forward * (50f * Time.fixedDeltaTime);
        }
    }

    [MenuButton("WASD Fly", ButtonType.Toggle, Category = "Movement", Order = 16, runOption = RunOption.Update)]
    public static void WASD()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null || GTPlayer.Instance == null || GorillaTagger.Instance == null)
        {
            StopLooking();
            return;
        }

        Rigidbody rb = GorillaTagger.Instance.rigidbody;
        Transform head = GTPlayer.Instance.headCollider.transform;
        if (mouse != null && mouse.rightButton.isPressed)
        {
            if (!Looking)
            {
                Looking = true;
                PreviousLock = Cursor.lockState;
                PreviousVisibility = Cursor.visible;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Vector2 delta = mouse.delta.ReadValue();
                Transform pivot = GTPlayer.Instance.turnParent.transform;
                float yaw = delta.x * MouseSensitivity;
                float newPitch = Mathf.Clamp(Pitch - delta.y * MouseSensitivity, -80f, 80f);
                float pitchDelta = newPitch - Pitch;
                pivot.RotateAround(head.position, Vector3.up, yaw);
                pivot.RotateAround(head.position, head.right, pitchDelta);
                Pitch = newPitch;
            }
        }
        else
        {
            StopLooking();
        }

        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 direction = Vector3.zero;
        if (keyboard.wKey.isPressed) direction += forward;
        if (keyboard.sKey.isPressed) direction -= forward;
        if (keyboard.dKey.isPressed) direction += right;
        if (keyboard.aKey.isPressed) direction -= right;
        if (keyboard.spaceKey.isPressed) direction += Vector3.up;
        if (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed) direction -= Vector3.up;
        bool shift = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        float speed = FlySpeed * (shift ? ShiftMultiplier : 1f);
        rb.linearVelocity = Vector3.zero;
        if (direction.sqrMagnitude > 0f) GTPlayer.Instance.transform.position += direction.normalized * speed * Time.deltaTime;
    }

    [MenuDisable("WASD Fly")]
    public static void WASDDisable()
    {
        StopLooking();
        if (GTPlayer.Instance != null)
        {
            Transform head = GTPlayer.Instance.headCollider.transform;
            GTPlayer.Instance.turnParent.transform.RotateAround(head.position, head.right, -Pitch);
        }
        Pitch = 0f;
        if (GorillaTagger.Instance != null) GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;

    }



    [MenuButton("Forzone", ButtonType.Toggle, Category = "Movement", Order = 17, runOption = RunOption.FixedUpdate, Description = "Forzone ice idk.")]
    public static void UpdateFrozone()
    {
        if (GTPlayer.Instance == null || Time.time - LastSpawn < 0.1f) return;
        if (!InputLib.LeftGrab && !InputLib.RightGrab) return;
        LastSpawn = Time.time;

        if (InputLib.LeftGrab)
            CreateForzonPlatfrom(GTPlayer.Instance.LeftHand.controllerTransform.position + GTPlayer.Instance.LeftHand.controllerTransform.right * 0.05f, GTPlayer.Instance.LeftHand.controllerTransform.rotation);

        if (InputLib.RightGrab)
            CreateForzonPlatfrom(GTPlayer.Instance.RightHand.controllerTransform.position - GTPlayer.Instance.RightHand.controllerTransform.right * 0.05f, GTPlayer.Instance.RightHand.controllerTransform.rotation);
    }

    [MenuButton("Platforms", ButtonType.Toggle, Category = "Movement", Order = 18, runOption = RunOption.Update, Description = "hand platfroms.")]
    public static void UpdatePlatforms()
    {
        bool leftGrip = InputLib.LeftGrab;
        bool rightGrip = InputLib.RightGrab;
        if (leftGrip && LeftPlatform == null) LeftPlatform = CreateLeftPlatform();
        if (rightGrip && RightPlatform == null) RightPlatform = CreateRightPlatfrom();
        if (leftGrip && LeftPlatform != null && !Leftspawn)
        {
            LeftPlatform.transform.position = GTPlayer.Instance.LeftHand.controllerTransform.position;
            LeftPlatform.transform.rotation = GTPlayer.Instance.LeftHand.controllerTransform.rotation;
            Leftspawn = true;
        }
        if (rightGrip && RightPlatform != null && !Rightspawn)
        {
            RightPlatform.transform.position = GTPlayer.Instance.RightHand.controllerTransform.position;
            RightPlatform.transform.rotation = GTPlayer.Instance.RightHand.controllerTransform.rotation;
            Rightspawn = true;
        }

        if (!leftGrip && LeftPlatform != null)
        {
            GameObject.Destroy(LeftPlatform);
            LeftPlatform = null;
            Leftspawn = false;
        }
        if (!rightGrip && RightPlatform != null)
        {
            GameObject.Destroy(RightPlatform);
            RightPlatform = null;
            Rightspawn = false;
        }
    }

    [MenuDisable("Platforms")]
    public static void DisablePlatforms()
    {
        if (LeftPlatform != null) GameObject.Destroy(LeftPlatform);
        if (RightPlatform != null) GameObject.Destroy(RightPlatform);
        RightPlatform = null;
        LeftPlatform = null;
        Leftspawn = false;
        Rightspawn = false;
    }

    [MenuEnable("Platforms")]
    public static void EnablePlatfroms()
    {
        LeftPlatform = null;
        RightPlatform = null;
        Leftspawn = false;
        Rightspawn = false;
    }

    [MenuButton("Platform Spam", ButtonType.Toggle, Category = "Movement", Order = 19)]
    public static void SpamPlatfrom()
    {
        if (InputLib.RightGrab)
        {
            GameObject PSpam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(PSpam.GetComponent<BoxCollider>());
            PSpam.GetComponent<Renderer>().material.color = Color.black;
            PSpam.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
            PSpam.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
            PSpam.transform.position = GTPlayer.Instance.RightHand.controllerTransform.position;
            PSpam.transform.rotation = GTPlayer.Instance.RightHand.controllerTransform.rotation;
            GameObject.Destroy(PSpam, 1f);
        }
    }

    [MenuButton("No Tag Freeze", ButtonType.Toggle, Category = "Movement", Order = 20)]
    public static void NTF() => GTPlayer.Instance.disableMovement = false;

    [MenuButton("Tag Freeze", ButtonType.Toggle, Category = "Movement", Order = 21)]
    public static void ForceTagFreeze() => GTPlayer.Instance.disableMovement = true;


    private static GameObject CreateLeftPlatform()
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject.Destroy(platform.GetComponent<Rigidbody>());
        GameObject.Destroy(platform.GetComponent<Renderer>());
        platform.transform.localScale = new Vector3(0.25f, 0.3f, 0.25f);
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject.Destroy(visual.GetComponent<Rigidbody>());
        visual.transform.SetParent(platform.transform);
        visual.transform.rotation = Quaternion.identity;
        visual.transform.localScale = new Vector3(0.1f, 1f, 1f);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.shader = Shader.Find("GorillaTag/UberShader");
        renderer.material.color = GorillaTagger.Instance.offlineVRRig.playerColor;
        visual.transform.localPosition = new Vector3(0.02f, 0f, 0f);
        return platform;
    }

    private static GameObject CreateRightPlatfrom() // Platfrom - lol
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject.Destroy(platform.GetComponent<Rigidbody>());
        GameObject.Destroy(platform.GetComponent<Renderer>());
        platform.transform.localScale = new Vector3(0.25f, 0.3f, 0.25f);
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject.Destroy(visual.GetComponent<Rigidbody>());
        visual.transform.SetParent(platform.transform);
        visual.transform.rotation = Quaternion.identity;
        visual.transform.localScale = new Vector3(0.1f, 1f, 1f);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.shader = Shader.Find("GorillaTag/UberShader");
        renderer.material.color = GorillaTagger.Instance.offlineVRRig.playerColor;
        visual.transform.localPosition = new Vector3(-0.02f, 0f, 0f);
        return platform;
    }

    private static void CreateForzonPlatfrom(Vector3 pos, Quaternion rot)
    {
        GameObject Plat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Plat.transform.position = pos;
        Plat.transform.rotation = rot;
        Plat.transform.localScale = new Vector3(0.015f, 0.28f, 0.28f);
        Renderer ren = Plat.GetComponent<Renderer>();
        ren.material.shader = Shader.Find("GorillaTag/UberShader");
        ren.transform.localScale = new Vector3(0.015f, 0.28f, 0.28f);
        GorillaSurfaceOverride sur = Plat.AddComponent<GorillaSurfaceOverride>();
        sur.overrideIndex = 61;
        GameObject.Destroy(Plat, 1f);
    }

    private static void StopLooking()
    {
        if (!Looking) return;

        Looking = false;
        Cursor.lockState = PreviousLock;
        Cursor.visible = PreviousVisibility;
    }
}
