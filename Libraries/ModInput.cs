// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2026 Astra

namespace ChudOS.Libraries;

internal static class ModInput
{
    internal static bool Held(int input)
    {
        if (ControllerInputPoller.instance == null) return false;
        return input switch
        {
            0 => InputLib.RightTrigger,
            1 => InputLib.LeftTrigger,
            2 => InputLib.RightGrab,
            3 => InputLib.LeftGrab,
            4 => InputLib.RightControllerAButton,
            5 => InputLib.RightControllerBButton,
            6 => InputLib.LeftControllerXButton,
            7 => InputLib.LeftControllerYButton,
            8 => InputLib.RightJoystickClick,
            9 => InputLib.LeftJoystickClick,
            _ => false
        };
    }
}
