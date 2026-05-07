using com.github.zehsteam.ImmersiveEntrance.Helpers;
using com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;
using HarmonyLib;

namespace com.github.zehsteam.ImmersiveEntrance.Patches;

[HarmonyPatch(typeof(MatchLocalPlayerPosition))]
internal static class MatchLocalPlayerPosition_Patches
{
    [HarmonyPatch(nameof(MatchLocalPlayerPosition.LateUpdate))]
    [HarmonyPrefix]
    private static bool LateUpdate_Patch(MatchLocalPlayerPosition __instance)
    {
        if (!PlayerUtils.IsLocalPlayerCameraInsideInterior()) // Default behaviour
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
