using com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;
using CullFactory;
using CullFactory.Data;
using HarmonyLib;

namespace com.github.zehsteam.ImmersiveEntrance.Dependencies.CullFactoryMod.Patches;

[HarmonyPatch(typeof(Config))]
internal static class Config_Patches
{
    [HarmonyPatch(nameof(Config.GetCullingType))]
    [HarmonyPrefix]
    private static bool GetCullingType_Patch(ref CullingType __result)
    {
        if (!CullFactoryProxy.OverrideCulling)
            return true;

        if (!DoorPortal.TryGetRenderingInstance(out DoorPortal doorPortal))
            return true;

        if (doorPortal.IsOutside)
            return true;

        __result = CullingType.None;
        return false;
    }
}
