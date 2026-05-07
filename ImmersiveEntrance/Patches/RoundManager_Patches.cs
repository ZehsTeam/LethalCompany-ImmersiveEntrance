using com.github.zehsteam.ImmersiveEntrance.Helpers;
using com.github.zehsteam.ImmersiveEntrance.Managers;
using HarmonyLib;

namespace com.github.zehsteam.ImmersiveEntrance.Patches;

[HarmonyPatch(typeof(RoundManager))]
internal static class RoundManager_Patches
{
    [HarmonyPatch(nameof(RoundManager.FinishGeneratingNewLevelClientRpc))]
    [HarmonyPostfix]
    private static void FinishGeneratingNewLevelClientRpc_Patch(RoundManager __instance)
    {
        if (!NetworkUtils.IsExecutingRPCMethod(__instance))
            return;

        Logger.LogInfo($"Current moon is {OutsideHelper.GetCurrentMoonName()}");
        Logger.LogInfo($"Current interior is {InteriorHelper.GetCurrentInteriorName()}");

        EntranceManager.LinkMainEntrancePortals();
    }
}
