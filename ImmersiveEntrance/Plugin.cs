using BepInEx;
using com.github.zehsteam.ImmersiveEntrance.Dependencies.CullFactoryMod;
using com.github.zehsteam.ImmersiveEntrance.Dependencies.LethalConfigMod;
using com.github.zehsteam.ImmersiveEntrance.Managers;
using com.github.zehsteam.ImmersiveEntrance.Patches;
using HarmonyLib;

namespace com.github.zehsteam.ImmersiveEntrance;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalConfigProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(CullFactoryProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    internal static Plugin Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        ImmersiveEntrance.Logger.Initialize(BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID));
        ImmersiveEntrance.Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} has awoken!");

        _harmony.PatchAll(typeof(StartOfRound_Patches));
        _harmony.PatchAll(typeof(RoundManager_Patches));
        _harmony.PatchAll(typeof(TimeOfDay_Patches));
        _harmony.PatchAll(typeof(MatchLocalPlayerPosition_Patches));
        _harmony.PatchAll(typeof(EntranceTeleport_Patches));

        if (CullFactoryProxy.IsInstalled)
        {
            CullFactoryProxy.PatchAll(_harmony);
        }

        Assets.Load();

        ConfigManager.Initialize(Config);
    }
}
