using com.github.zehsteam.PeekInside.Helpers;
using GameNetcodeStuff;
using HarmonyLib;

namespace com.github.zehsteam.PeekInside.Patches;

[HarmonyPatch(typeof(MatchLocalPlayerPosition))]
internal static class MatchLocalPlayerPosition_Patches
{
    [HarmonyPatch(nameof(MatchLocalPlayerPosition.LateUpdate))]
    [HarmonyPrefix]
    private static bool LateUpdate_Patch()
    {
        if (!PlayerUtils.TryGetLocalPlayerScript(out PlayerControllerB playerScript))
            return true;

        return !playerScript.isInsideFactory;
    }
}
