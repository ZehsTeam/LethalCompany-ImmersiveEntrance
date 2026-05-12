using BepInEx;
using BepInEx.Configuration;
using com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

namespace com.github.zehsteam.ImmersiveEntrance.Helpers;

internal static class Utils
{
    public static string GetPluginDirectoryPath()
    {
        return Path.GetDirectoryName(Plugin.Instance.Info.Location);
    }

    public static string GetConfigDirectoryPath()
    {
        return Paths.ConfigPath;
    }

    public static string GetPluginPersistentDataPath()
    {
        return Path.Combine(Application.persistentDataPath, MyPluginInfo.PLUGIN_NAME);
    }

    public static ConfigFile CreateConfigFile(BaseUnityPlugin plugin, string path, string name = null, bool saveOnInit = false)
    {
        BepInPlugin metadata = MetadataHelper.GetMetadata(plugin);
        name ??= metadata.GUID;
        name += ".cfg";
        return new ConfigFile(Path.Combine(path, name), saveOnInit, metadata);
    }

    public static ConfigFile CreateLocalConfigFile(BaseUnityPlugin plugin, string name = null, bool saveOnInit = false)
    {
        return CreateConfigFile(plugin, GetConfigDirectoryPath(), name, saveOnInit);
    }

    public static ConfigFile CreateGlobalConfigFile(BaseUnityPlugin plugin, string name = null, bool saveOnInit = false)
    {
        string path = GetPluginPersistentDataPath();
        name ??= "global";
        return CreateConfigFile(plugin, path, name, saveOnInit);
    }

    public static bool RollPercentChance(float percent)
    {
        if (percent <= 0f) return false;
        if (percent >= 100f) return true;
        return Random.value * 100f <= percent;
    }

    public static void InvokeNextFrame(Action action)
    {
        IEnumerator Coroutine()
        {
            yield return null;
            action?.Invoke();
        }

        CoroutineRunner.Start(Coroutine());
    }

    public static void InvokeAfterDelay(Action action, TimeSpan timeSpan)
    {
        IEnumerator Coroutine()
        {
            yield return new WaitForSeconds((float)timeSpan.TotalSeconds);
            action?.Invoke();
        }

        CoroutineRunner.Start(Coroutine());
    }
}
