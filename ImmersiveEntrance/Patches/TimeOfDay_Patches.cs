using com.github.zehsteam.ImmersiveEntrance.Extensions;
using com.github.zehsteam.ImmersiveEntrance.Helpers;
using com.github.zehsteam.ImmersiveEntrance.Helpers.IL;
using com.github.zehsteam.ImmersiveEntrance.Managers;
using com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace com.github.zehsteam.ImmersiveEntrance.Patches;

[HarmonyPatch(typeof(TimeOfDay))]
internal static class TimeOfDay_Patches
{
    [HarmonyPatch(nameof(TimeOfDay.Start))]
    [HarmonyPostfix]
    private static void Start_Patch(TimeOfDay __instance)
    {
        __instance.gameObject.AddComponent<WeatherAudioManager>();
    }

    [HarmonyPatch(nameof(TimeOfDay.SetInsideLightingDimness))]
    [HarmonyPostfix]
    private static void SetInsideLightingDimness_Patch(TimeOfDay __instance)
    {
        HDAdditionalLightData indirectLightData = __instance.indirectLightData;

        // Null check here since using the Teleporter in orbit calls the SetInsideLightingDimness method and indirectLightData is null since there is no moon scene 
        if (indirectLightData == null)
            return;

        float value = PlayerUtils.IsLocalPlayerCameraInsideInterior() ? 0f : 1f;

        indirectLightData.lightDimmer = value;
    }

    [HarmonyPatch(nameof(TimeOfDay.SetWeatherEffects))]
    [HarmonyPrefix]
    private static void SetWeatherEffects_Patch(TimeOfDay __instance)
    {
        if (!ConfigManager.PortalGraphics_WeatherEffectsEnabled.Value)
            return;

        if (LevelHelper.IsForceWeatherEffectsEnabled)
        {
            __instance.SetCurrentLevelWeatherEnabled(true);
        }
    }

    [HarmonyPatch(nameof(TimeOfDay.SetWeatherEffects))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> SetWeatherEffects_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var injector = new ILInjector(instructions);

        injector
            .Find([
                ILMatcher.Call(typeof(StartOfRound).GetProperty(nameof(StartOfRound.Instance)).GetMethod),
                ILMatcher.Ldfld(typeof(StartOfRound).GetField(nameof(StartOfRound.spectateCamera))),
                ILMatcher.Callvirt(typeof(Component).GetProperty(nameof(Component.transform)).GetMethod),
                ILMatcher.Callvirt(typeof(Transform).GetProperty(nameof(Transform.position)).GetMethod),
                ILMatcher.Stloc().CaptureAs(out var storePosition1),
            ]);

        if (injector.IsValid)
        {
            injector.ReplaceLastMatch([
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LevelHelper), nameof(LevelHelper.GetPositionForWeatherEffects))),
                storePosition1,
            ]);
        }
        else
        {
            Logger.LogError($"[{nameof(TimeOfDay_Patches)}] {nameof(SetWeatherEffects_Transpiler)}: Failed to run! Could not match instructions.");
            return instructions;
        }

        injector
            .Find([
                ILMatcher.Call(typeof(StartOfRound).GetProperty(nameof(StartOfRound.Instance)).GetMethod),
                ILMatcher.Ldfld(typeof(StartOfRound).GetField(nameof(StartOfRound.localPlayerController))),
                ILMatcher.Callvirt(typeof(Component).GetProperty(nameof(Component.transform)).GetMethod),
                ILMatcher.Callvirt(typeof(Transform).GetProperty(nameof(Transform.position)).GetMethod),
                ILMatcher.Stloc().CaptureAs(out var storePosition2),
            ]);

        if (injector.IsValid)
        {
            injector.ReplaceLastMatch([
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LevelHelper), nameof(LevelHelper.GetPositionForWeatherEffects))),
                storePosition2,
            ]);
        }
        else
        {
            Logger.LogError($"[{nameof(TimeOfDay_Patches)}] {nameof(SetWeatherEffects_Transpiler)}: Failed to run! Could not match instructions.");
            return instructions;
        }

        return injector.ReleaseInstructions();
    }
}
