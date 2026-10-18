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

using UnityEngine;

namespace ChudOS.Mods.Weather;

public class WeatherHelpers
{
    private static BetterDayNightManager.WeatherType[]? OriginalWeatherCycle;

    public static void StopRain()
    {
        BetterDayNightManager manager = BetterDayNightManager.instance;
        if (manager == null || manager.weatherCycle == null || OriginalWeatherCycle != null) return;

        OriginalWeatherCycle = (BetterDayNightManager.WeatherType[])manager.weatherCycle.Clone();

        for (int i = 0; i < manager.weatherCycle.Length; i++)
            manager.weatherCycle[i] = BetterDayNightManager.WeatherType.None;
    }

    public static void StartRain()
    {
        BetterDayNightManager manager = BetterDayNightManager.instance;
        if (manager == null || manager.weatherCycle == null || OriginalWeatherCycle == null) return;

        if (manager.weatherCycle.Length == OriginalWeatherCycle.Length)
            OriginalWeatherCycle.CopyTo(manager.weatherCycle, 0);

        OriginalWeatherCycle = null;
    }
}
