using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.Managers;
using com.github.zehsteam.PeekInside.MonoBehaviours;
using HarmonyLib;

namespace com.github.zehsteam.PeekInside.Patches;

[HarmonyPatch(typeof(RoundManager))]
internal static class RoundManager_Patches
{
    private static bool _initialized;

    [HarmonyPatch(nameof(RoundManager.FinishGeneratingNewLevelClientRpc))]
    [HarmonyPostfix]
    private static void FinishGeneratingNewLevelClientRpc_Patch(RoundManager __instance)
    {
        if (!NetworkUtils.IsExecutingRPCMethod(__instance))
            return;

        if (_initialized) return;
        _initialized = true;

        EntranceManager.LinkMainEntrancePortals();
        FacilitySunBlocker.Spawn();
    }

    public static void OnEndOfGame()
    {
        _initialized = false;
    }
}
