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
        EntranceManager.SpawnDoorPortal(__instance);
    }
}
