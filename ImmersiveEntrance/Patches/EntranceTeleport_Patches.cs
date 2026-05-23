using com.github.zehsteam.ImmersiveEntrance.Extensions;
using com.github.zehsteam.ImmersiveEntrance.Helpers;
using com.github.zehsteam.ImmersiveEntrance.Managers;
using HarmonyLib;
using System.Text;

namespace com.github.zehsteam.ImmersiveEntrance.Patches;

[HarmonyPatch(typeof(EntranceTeleport))]
internal static class EntranceTeleport_Patches
{
    [HarmonyPatch(nameof(EntranceTeleport.Awake))]
    [HarmonyPostfix]
    private static void Awake_Patch(EntranceTeleport __instance)
    {
        if (Logger.IsExtendedLoggingEnabled)
        {
            var building = new StringBuilder();

            building.AppendLine($"| {nameof(EntranceTeleport)}.{nameof(EntranceTeleport.Awake)}();");
            building.AppendLine($"| ");
            building.AppendLine($"| {__instance.GetLogInfo()}");
            building.AppendLine($"| ");
            building.AppendLine($"| Moon: \"{LevelHelper.GetCurrentMoonName()}\"");
            building.AppendLine($"| Interior: \"{InteriorHelper.GetCurrentInteriorName()}\"");
            building.AppendLine($"| ");
            building.AppendLine($"| Scene: \"{__instance.gameObject.scene.name}\"");
            building.AppendLine($"| Hierarchy path: \"{__instance.transform.GetHierarchyPath()}\"");

            Logger.LogInfo($"\n\n{building.ToString().Trim()}\n");
        }

        EntranceManager.SpawnDoorPortal(__instance);
    }
}
