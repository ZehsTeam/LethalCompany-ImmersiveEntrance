using BepInEx.Configuration;
using com.github.zehsteam.PeekInside.Helpers;

namespace com.github.zehsteam.PeekInside.Managers;

internal static class ConfigManager
{
    public static ConfigFile ConfigFile { get; private set; }

    // Misc
    public static ConfigEntry<bool> Misc_ExtendedLogging { get; private set; }

    // Door Portals
    public static ConfigEntry<bool> DoorPortals_Enabled { get; private set; }
    public static ConfigEntry<float> DoorPortals_ActiveRange { get; private set; }

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

        // Door Portals
        DoorPortals_Enabled =     ConfigHelper.Bind("Door Portals", "Enabled",     defaultValue: true, "");
        DoorPortals_ActiveRange = ConfigHelper.Bind("Door Portals", "ActiveRange", defaultValue: 10f,  "");

        // Debug
        Debug_HideDoorObjects = ConfigHelper.Bind("Debug", "HideDoorObjects", defaultValue: false, "");
    }
}
