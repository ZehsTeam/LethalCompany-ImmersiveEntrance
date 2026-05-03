using com.github.zehsteam.PeekInside.Helpers;
using HarmonyLib;

namespace com.github.zehsteam.PeekInside.Patches;

[HarmonyPatch(typeof(TimeOfDay))]
internal static class TimeOfDay_Patches
{
    [HarmonyPatch(nameof(TimeOfDay.SetInsideLightingDimness))]
    [HarmonyPrefix]
    private static bool SetInsideLightingDimness_Patch()
    {
        return !OutsideHelper.IsOverridingSun;
    }
}
