using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.Managers;
using HarmonyLib;

namespace com.github.zehsteam.PeekInside.Patches;

[HarmonyPatch(typeof(EntranceTeleport))]
internal static class EntranceTeleport_Patches
{
    [HarmonyPatch(nameof(EntranceTeleport.Awake))]
    [HarmonyPostfix]
    private static void Awake_Patch(EntranceTeleport __instance)
    {
        Utils.ExecuteAfterDelay(() =>
        {
            EntranceDoorReplacementManager.ReplaceDoor(__instance);
            EntranceManager.SpawnDoorPortal(__instance);
        }, 0.1f);
    }
}
