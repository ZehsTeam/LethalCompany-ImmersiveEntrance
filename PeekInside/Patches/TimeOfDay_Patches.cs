using com.github.zehsteam.PeekInside.Helpers;
using HarmonyLib;

namespace com.github.zehsteam.PeekInside.Patches;

[HarmonyPatch(typeof(TimeOfDay))]
internal static class TimeOfDay_Patches
{
    [HarmonyPatch(nameof(TimeOfDay.SetInsideLightingDimness))]
    [HarmonyPostfix]
    private static void SetInsideLightingDimness_Patch(TimeOfDay __instance)
    {
        float value = PlayerUtils.IsLocalPlayerCameraInsideInterior() ? 0f : 1f;
        __instance.indirectLightData.lightDimmer = value;
    }
}
