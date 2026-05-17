namespace com.github.zehsteam.ImmersiveEntrance.Extensions;

internal static class TimeOfDayExtensions
{
    public static void SetCurrentLevelWeatherEnabled(this TimeOfDay timeOfDay, bool value)
    {
        if (timeOfDay == null)
            return;

        if (timeOfDay.currentLevelWeather == LevelWeatherType.None)
            return;

        int currentLevelWeatherIndex = (int)timeOfDay.currentLevelWeather;

        timeOfDay.effects[currentLevelWeatherIndex].effectEnabled = value;
    }
}
