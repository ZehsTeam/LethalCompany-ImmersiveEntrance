using com.github.zehsteam.ImmersiveEntrance.Objects;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance;

internal static class Assets
{
    public static readonly string AssetBundleFileName = "immersiveentrance_assets";
    public static AssetBundle AssetBundle { get; private set; }
    public static bool IsLoaded { get; private set; }

    // Prefabs
    public static GameObject DoorPortalPrefab { get; private set; }

    // Shaders
    public static Shader PosterizeShader { get; private set; }

    // EntranceDoorReplacements
    public static EntranceDoorReplacement MansionEntranceDoorReplacement { get; private set; }
    
    public static void Load()
    {
        string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string assetBundlePath = Path.Combine(pluginFolder, AssetBundleFileName);

        if (!File.Exists(assetBundlePath))
        {
            Logger.LogFatal($"Failed to load assets. AssetBundle file could not be found at path \"{assetBundlePath}\". Make sure the \"{AssetBundleFileName}\" file is in the same folder as the mod's DLL file.");
            return;
        }

        AssetBundle = AssetBundle.LoadFromFile(assetBundlePath);

        if (AssetBundle == null)
        {
            Logger.LogFatal($"Failed to load assets. AssetBundle is null.");
            return;
        }

        OnAssetBundleLoaded(AssetBundle);

        IsLoaded = true;
    }

    private static void OnAssetBundleLoaded(AssetBundle assetBundle)
    {
        // Prefabs
        DoorPortalPrefab = LoadAsset<GameObject>("DoorPortal", assetBundle);

        // Shaders
        PosterizeShader = LoadAsset<Shader>("ImmersiveEntrancePosterize", assetBundle);

        // EntranceDoorReplacements
        MansionEntranceDoorReplacement = LoadAsset<EntranceDoorReplacement>("MansionEntranceDoorReplacement", assetBundle);
    }

    private static T LoadAsset<T>(string name, AssetBundle assetBundle) where T : Object
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Logger.LogError($"Failed to load asset of type \"{typeof(T).Name}\" from AssetBundle. Name is null or whitespace.");
            return null;
        }

        if (assetBundle == null)
        {
            Logger.LogError($"Failed to load asset of type \"{typeof(T).Name}\" with name \"{name}\" from AssetBundle. AssetBundle is null.");
            return null;
        }

        T asset = assetBundle.LoadAsset<T>(name);

        if (asset == null)
        {
            Logger.LogError($"Failed to load asset of type \"{typeof(T).Name}\" with name \"{name}\" from AssetBundle. No asset found with that type and name.");
            return null;
        }

        return asset;
    }

    private static bool TryLoadAsset<T>(string name, AssetBundle assetBundle, out T asset) where T : Object
    {
        asset = LoadAsset<T>(name, assetBundle);
        return asset != null;
    }
}
