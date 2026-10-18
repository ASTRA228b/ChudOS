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
