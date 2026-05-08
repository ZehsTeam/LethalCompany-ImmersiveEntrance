using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Helpers;

internal static class LevelHelper
{
    private static GameObject _environment;

    public static void SetSunEnabled(bool value)
    {
        TimeOfDay timeOfDay = TimeOfDay.Instance;

        if (timeOfDay == null)
            return;

        timeOfDay.sunDirect?.enabled = value;
        timeOfDay.sunIndirect?.enabled = value;
        timeOfDay.indirectLightData?.lightDimmer = value ? 1f : 0f;
    }

    public static string GetCurrentMoonName()
    {
        return StartOfRound.Instance?.currentLevel?.PlanetName;
    }

    public static bool TryGetEnvironment(out GameObject result)
    {
        if (_environment != null)
        {
            result = _environment;
            return true;
        }

        _environment = GameObject.FindWithTag("OutsideLevelNavMesh");
        result = _environment;
        return result != null;
    }
}
