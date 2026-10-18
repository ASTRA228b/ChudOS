using ChudOS.Stuff;
using ChudOS.Core.Managers;


namespace ChudOS.Mods.Weather;

public static class WeatherButtons
{
    [MenuButton("Time", ButtonType.Option, Category = "Weather", Order = 0, Options = new[] { "Morning", "10AM", "Day", "Evening", "Night" })]
    public static void Time()
    {
        var button = ButtonManager.GetButton("Time");
        if (button == null) return;
        var manager = BetterDayNightManager.instance;
        if (manager == null) return;
        string mode = button.Option;
        int index = button.OptionIndex;
        switch (index)
        {
            case 0: manager.SetTimeOfDay(1, true); break; // Morning
            case 1: manager.SetTimeOfDay(3, true); break; // 10AM
            case 2: manager.SetTimeOfDay(4, true); break; // Day
            case 3: manager.SetTimeOfDay(6, true); break; // Evening 
            case 4: manager.SetTimeOfDay(0, true); break; // Night
        }
    }

    [MenuButton("No Rain", ButtonType.Toggle, Category = "Weather", Order = 1, runOption = RunOption.Once)]
    public static void NoRain() => WeatherHelpers.StopRain();

    [MenuDisable("No Rain")]
    public static void DisableNoRain() => WeatherHelpers.StartRain();
}
