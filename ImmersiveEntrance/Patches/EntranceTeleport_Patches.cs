using com.github.zehsteam.ImmersiveEntrance.Managers;
using HarmonyLib;

namespace com.github.zehsteam.ImmersiveEntrance.Patches;

[HarmonyPatch(typeof(EntranceTeleport))]
internal static class EntranceTeleport_Patches
{
    [HarmonyPatch(nameof(EntranceTeleport.Awake))]
    [HarmonyPostfix]
    private static void Awake_Patch(EntranceTeleport __instance)
    {
        EntranceDoorReplacementManager.ReplaceDoor(__instance);
        EntranceManager.SpawnDoorPortal(__instance);
    }
}
