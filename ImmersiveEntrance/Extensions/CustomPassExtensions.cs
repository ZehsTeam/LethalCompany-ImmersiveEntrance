using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace com.github.zehsteam.ImmersiveEntrance.Extensions;

internal static class CustomPassExtensions
{
    private static readonly FieldInfo _ownerField = AccessTools.Field(typeof(CustomPass), "owner");

    public static CustomPassVolume GetCustomPassVolume(this CustomPass customPass)
    {
        return _ownerField.GetValue(customPass) as CustomPassVolume;
    }

    public static Camera GetTargetCamera(this CustomPass customPass)
    {
        return GetCustomPassVolume(customPass).targetCamera;
    }

    public static bool TryGetTargetCamera(this CustomPass customPass, out Camera targetCamera)
    {
        targetCamera = GetTargetCamera(customPass);
        return targetCamera != null;
    }
}
