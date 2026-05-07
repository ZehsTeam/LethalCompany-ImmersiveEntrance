using com.github.zehsteam.ImmersiveEntrance.Managers;
using HarmonyLib;

namespace com.github.zehsteam.ImmersiveEntrance.Patches;

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
    }

    [HarmonyPatch(nameof(StartOfRound.OnLocalDisconnect))]
    [HarmonyPostfix]
    private static void OnLocalDisconnect_Patch()
    {
        EntranceManager.Reset();
    }
}
