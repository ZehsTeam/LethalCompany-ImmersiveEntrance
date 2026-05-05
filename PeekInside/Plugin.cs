using BepInEx;
using com.github.zehsteam.PeekInside.Dependencies.LethalConfigMod;
using com.github.zehsteam.PeekInside.Managers;
using com.github.zehsteam.PeekInside.Patches;
using HarmonyLib;

namespace com.github.zehsteam.PeekInside;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalConfigProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    internal static Plugin Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        PeekInside.Logger.Initialize(BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID));
        PeekInside.Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} has awoken!");

        _harmony.PatchAll(typeof(StartOfRound_Patches));
        _harmony.PatchAll(typeof(RoundManager_Patches));
        _harmony.PatchAll(typeof(TimeOfDay_Patches));
        _harmony.PatchAll(typeof(MatchLocalPlayerPosition_Patches));
        _harmony.PatchAll(typeof(EntranceTeleport_Patches));

        Assets.Load();

        ConfigManager.Initialize(Config);
    }
}
