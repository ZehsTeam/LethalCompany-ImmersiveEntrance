using com.github.zehsteam.ImmersiveEntrance.Helpers;
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
        CustomPassHelper.ReplaceVanillaCustomPass();
    }

    [HarmonyPatch(nameof(StartOfRound.EndOfGame))]
    [HarmonyPostfix]
    private static void EndOfGame_Patch()
    {
        EntranceManager.Reset();
        LevelHelper.Reset();
    }

    [HarmonyPatch(nameof(StartOfRound.OnLocalDisconnect))]
    [HarmonyPostfix]
    private static void OnLocalDisconnect_Patch()
    {
        EntranceManager.Reset();
        LevelHelper.Reset();
    }

    // Hopefully this will help with CullFactory compatibility? Not too sure
    //[HarmonyPatch(nameof(StartOfRound.SetOcclusionCullerToPosition))]
    //[HarmonyPrefix]
    //private static void SetOcclusionCullerToPosition_Patch(ref Vector3 setToPosition)
    //{
    //    if (!DoorPortal.TryGetRenderingInstance(out DoorPortal doorPortal))
    //        return;

    //    if (doorPortal.IsOutside)
    //        return;

    //    setToPosition = doorPortal.transform.position;
    //}
}
