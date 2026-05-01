using com.github.zehsteam.PeekInside.Managers;
using HarmonyLib;

namespace com.github.zehsteam.PeekInside.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal static class StartOfRound_Patches
{
    [HarmonyPatch(nameof(StartOfRound.ShipHasLeft))]
    [HarmonyPostfix]
    private static void ShipHasLeft_Patch()
    {
        EntranceManager.OnShipHasLeft();
    }
}
