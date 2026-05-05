using BepInEx.Configuration;
using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.MonoBehaviours;

namespace com.github.zehsteam.PeekInside.Managers;

internal static class ConfigManager
{
    public static ConfigFile ConfigFile { get; private set; }

    // Misc
    public static ConfigEntry<bool> Misc_ExtendedLogging { get; private set; }

    // Portal
    public static ConfigEntry<bool> Portal_Enabled { get; private set; }
    public static ConfigEntry<float> Portal_ActivationRange { get; private set; }
    public static ConfigEntry<float> Portal_OutsideViewRange { get; private set; }
    public static ConfigEntry<float> Portal_InsideViewRange { get; private set; }

    // Mansion
    public static ConfigEntry<bool> Mansion_ReplaceInteriorMainEntranceDoors { get; private set; }

    // Debug
    public static ConfigEntry<bool> Debug_HideDoorObjects { get; private set; }

    public static void Initialize(ConfigFile configFile)
    {
        ConfigFile = configFile;
        BindConfigs();
    }

    private static void BindConfigs()
    {
        ConfigHelper.SkipAutoGen();

        // Misc
        Misc_ExtendedLogging = ConfigHelper.Bind("Misc", "ExtendedLogging", defaultValue: false, "Enable extended logging.");

        // Portal
        Portal_Enabled =          ConfigHelper.Bind("Portal", "Enabled",          defaultValue: true, "");
        Portal_ActivationRange =  ConfigHelper.Bind("Portal", "ActivationRange",  defaultValue: 10f,  "");
        Portal_OutsideViewRange = ConfigHelper.Bind("Portal", "OutsideViewRange", defaultValue: 250f, "");
        Portal_InsideViewRange =  ConfigHelper.Bind("Portal", "InsideViewRange",  defaultValue: 50f,  "");

        Portal_OutsideViewRange.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();
        Portal_InsideViewRange.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();

        // Mansion
        Mansion_ReplaceInteriorMainEntranceDoors = ConfigHelper.Bind("Mansion", "ReplaceInteriorMainEntranceDoors", defaultValue: true, "");

        // Debug
        Debug_HideDoorObjects = ConfigHelper.Bind("Debug", "HideDoorObjects", defaultValue: false, "");
    }
}
