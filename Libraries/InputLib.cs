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

using Valve.VR;
using UnityEngine;

namespace ChudOS.Libraries;

internal class InputLib
{
    public static bool RightGrab => ControllerInputPoller.instance.rightControllerGripFloat > 0.5f;
    public static bool LeftGrab => ControllerInputPoller.instance.leftControllerGripFloat > 0.5f;
    public static bool RightTrigger => ControllerInputPoller.instance.rightControllerTriggerButton;
    public static bool LeftTrigger => ControllerInputPoller.instance.leftControllerTriggerButton;
    // A
    public static bool RightControllerAButton => ControllerInputPoller.instance.rightControllerPrimaryButton;
    // B 
    public static bool RightControllerBButton => ControllerInputPoller.instance.rightControllerSecondaryButton;
    // Y 
    public static bool LeftControllerYButton => ControllerInputPoller.instance.leftControllerSecondaryButton;
    // X
    public static bool LeftControllerXButton => ControllerInputPoller.instance.leftControllerPrimaryButton;
    // Note: on the htc vive wands both L/R The one button counts for both SecondaryButtons and PrimaryButtons
    // Joystick Stuff
    public static bool RightJoystickClick
    {
        get
        {
            if (SteamVR_Actions.gorillaTag_RightJoystickClick == null)
                return false;

            return SteamVR_Actions.gorillaTag_RightJoystickClick.state;
        }
    }

    public static bool LeftJoystickClick
    {
        get
        {
            if (SteamVR_Actions.gorillaTag_LeftJoystickClick == null)
                return false;

            return SteamVR_Actions.gorillaTag_LeftJoystickClick.state;
        }
    }

    public static Vector2 RightJoyStickAxis
    {
        get
        {
            if (SteamVR_Actions.gorillaTag_RightJoystick2DAxis == null)
                return Vector2.zero;

            var axis = SteamVR_Actions.gorillaTag_RightJoystick2DAxis.axis;
            return new Vector2(axis.x, axis.y);
        }
    }

    public static Vector2 LeftJoyStickAxis
    {
        get
        {
            if (SteamVR_Actions.gorillaTag_LeftJoystick2DAxis == null)
                return Vector2.zero;

            var axis = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.axis;
            return new Vector2(axis.x, axis.y);
        }
    }
}