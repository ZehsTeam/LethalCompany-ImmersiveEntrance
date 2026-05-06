using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.Managers;
using HarmonyLib;

namespace com.github.zehsteam.PeekInside.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal static class StartOfRound_Patches
{
    [HarmonyPatch(nameof(StartOfRound.Start))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Start_Patch()
    {
        PortalSettingsManager.Initialize();
    }

    [HarmonyPatch(nameof(StartOfRound.EndOfGame))]
    [HarmonyPostfix]
    private static void EndOfGame_Patch()
    {
        EntranceManager.Reset();
        OutsideHelper.Reset();
    }

    [HarmonyPatch(nameof(StartOfRound.OnLocalDisconnect))]
    [HarmonyPostfix]
    private static void OnLocalDisconnect_Patch()
    {
        EntranceManager.Reset();
        OutsideHelper.Reset();
    }
}
