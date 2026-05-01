using com.github.zehsteam.PeekInside.Helpers;
using HarmonyLib;

namespace com.github.zehsteam.PeekInside.Patches;

[HarmonyPatch(typeof(RoundManager))]
internal static class RoundManager_Patches
{
    [HarmonyPatch(nameof(RoundManager.FinishGeneratingNewLevelClientRpc))]
    [HarmonyPostfix]
    private static void FinishGeneratingNewLevelClientRpc_Patch(RoundManager __instance)
    {
        if (!NetworkUtils.IsExecutingRPCMethod(__instance))
            return;

        EntranceTeleport_Patches.LinkPortals();
    }
}
