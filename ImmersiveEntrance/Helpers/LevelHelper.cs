using com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;
using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Helpers;

internal static class LevelHelper
{
    public static bool IsForceWeatherEffectsEnabled { get; private set; }

    private static GameObject _environment;

    public static void Reset()
    {
        IsForceWeatherEffectsEnabled = false;
    }

    public static void SetSunAndSkyEnabledThisFrame(bool value)
    {
        SetSunEnabledThisFrame(value);
        SetSkyEnabledThisFrame(value);
    }

    private static void SetSunEnabledThisFrame(bool value)
    {
        TimeOfDay timeOfDay = TimeOfDay.Instance;

        if (timeOfDay == null)
            return;

        timeOfDay.sunDirect?.enabled = value;
        timeOfDay.sunIndirect?.enabled = value;
        timeOfDay.indirectLightData?.lightDimmer = value ? 1f : 0f;
    }

    private static void SetSkyEnabledThisFrame(bool value)
    {
        StartOfRound startOfRound = StartOfRound.Instance;

        if (startOfRound == null)
            return;

        startOfRound.blackSkyVolume?.weight = value ? 0f : 1f;
    }

    public static void SetWeatherEffectsEnabled(bool value)
    {
        TimeOfDay timeOfDay = TimeOfDay.Instance;

        if (timeOfDay == null)
            return;

        if (timeOfDay.currentLevelWeather == LevelWeatherType.None)
            return;

        int currentLevelWeather = (int)TimeOfDay.Instance.currentLevelWeather;

        TimeOfDay.Instance.effects[currentLevelWeather].effectEnabled = value;
    }

    public static void SetForceWeatherEffectsEnabled(bool value)
    {
        IsForceWeatherEffectsEnabled = value;

        if (!value)
        {
            bool isInsideInterior = PlayerUtils.IsLocalPlayerCameraInsideInterior();

            if (isInsideInterior)
            {
                TimeOfDay.Instance?.DisableAllWeather();
            }
        }
    }

    public static Vector3 GetPositionForWeatherEffects()
    {
        bool isInsideInterior = PlayerUtils.IsLocalPlayerCameraInsideInterior();

        if (isInsideInterior)
        {
            if (DoorPortal.TryGetRenderingInstance(out DoorPortal doorPortal))
            {
                return doorPortal.transform.position;
            }
            
            return Vector3.zero;
        }

        if (PlayerUtils.TryGetLocalPlayerCamera(out Camera playerCamera))
        {
            return playerCamera.transform.position;
        }

        return Vector3.zero;
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
