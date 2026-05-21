using BepInEx.Bootstrap;
using CullFactory.Behaviours.API;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Dependencies.CullFactoryMod;

internal static class CullFactoryProxy
{
    public const string PLUGIN_GUID = CullFactory.Plugin.Guid;
    public static bool IsInstalled => Chainloader.PluginInfos.ContainsKey(PLUGIN_GUID);

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void DisableCullingForCamera(Camera camera)
    {
        if (camera == null)
            return;

        if (!camera.TryGetComponent(out CameraCullingOptions options))
        {
            options = camera.gameObject.AddComponent<CameraCullingOptions>();
        }

        options.DisableCulling = true;
    }
}
