using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.MonoBehaviours;
using GameNetcodeStuff;
using HarmonyLib;

namespace com.github.zehsteam.PeekInside.Patches;

[HarmonyPatch(typeof(MatchLocalPlayerPosition))]
internal static class MatchLocalPlayerPosition_Patches
{
    [HarmonyPatch(nameof(MatchLocalPlayerPosition.LateUpdate))]
    [HarmonyPrefix]
    private static bool LateUpdate_Patch(MatchLocalPlayerPosition __instance)
    {
        if (!PlayerUtils.TryGetLocalPlayerScript(out PlayerControllerB playerScript))
            return true;

        if (!playerScript.isInsideFactory) // Default behaviour
            return true;

        if (DoorPortal.TryGetRenderingInstance(out DoorPortal doorPortal))
        {
            // Follow the rendering portal camera
            __instance.transform.position = doorPortal.PortalCamera.transform.position;
            return false;
        }

        // Do nothing
        return false;
    }
}
