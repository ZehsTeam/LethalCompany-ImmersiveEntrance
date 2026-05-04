namespace com.github.zehsteam.PeekInside.Helpers;

internal static class LevelHelper
{
    public static string GetCurrentMoonName()
    {
        return StartOfRound.Instance?.currentLevel?.PlanetName;
    }
}
