using BepInEx.Bootstrap;
using com.github.zehsteam.ImmersiveEntrance.Dependencies.CullFactoryMod.Patches;
using com.github.zehsteam.ImmersiveEntrance.Managers;
using CullFactory.Behaviours.CullingMethods;
using HarmonyLib;
using System;
using System.Runtime.CompilerServices;

namespace com.github.zehsteam.ImmersiveEntrance.Dependencies.CullFactoryMod;

internal static class CullFactoryProxy
{
    public const string PLUGIN_GUID = CullFactory.Plugin.Guid;
    public static bool IsInstalled => Chainloader.PluginInfos.ContainsKey(PLUGIN_GUID);

    public static bool OverrideCulling => ConfigManager.Portal_OverrideCullFactory?.Value ?? false;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void PatchAll(Harmony harmony)
    {
        try
        {
            harmony.PatchAll(typeof(Config_Patches));

            Logger.LogInfo($"Applied CullFactory patches.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to apply CullFactory patches. {ex}");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void DisableCullFactory()
    {
        if (!OverrideCulling)
            return;

        if (CullingMethod.Instance == null)
            return;

        CullingMethod.Instance.gameObject.SetActive(false);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void EnableCullFactory()
    {
        if (!OverrideCulling)
            return;

        if (CullingMethod.Instance == null)
            return;

        CullingMethod.Instance.gameObject.SetActive(true);
    }
}
