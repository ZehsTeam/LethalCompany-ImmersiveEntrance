using BepInEx.Configuration;
using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.MonoBehaviours;
using com.github.zehsteam.PeekInside.Objects;
using System.Text;

namespace com.github.zehsteam.PeekInside.Managers;

internal static class ConfigManager
{
    public static ConfigFile ConfigFile { get; private set; }

    // Misc
    public static ConfigEntry<bool> Misc_ExtendedLogging { get; private set; }

    // Portal
    public static ConfigEntry<bool> Portal_Enabled { get; private set; }
    public static ConfigEntry<PixelResolutionType> Portal_PixelResolution { get; private set; }
    public static ConfigEntry<float> Portal_ActivationRange { get; private set; }
    public static ConfigEntry<float> Portal_OutsideViewDistance { get; private set; }
    public static ConfigEntry<float> Portal_InsideViewDistance { get; private set; }

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
        Portal_Enabled = ConfigHelper.Bind("Portal", "Enabled", defaultValue: true, "Enable portals!");

        var descriptionBuilder = new StringBuilder();
        descriptionBuilder.AppendLine("The rendering pixel resolution of portals.");
        descriptionBuilder.AppendLine("PlayerCamera = Automatically adjusts to the player camera's rendering resolution.");
        descriptionBuilder.AppendLine("Default = 860x520.");
        descriptionBuilder.AppendLine("Performance = 620x364.");
        descriptionBuilder.AppendLine("UltraPerformance = 400x260.");
        descriptionBuilder.AppendLine("Retro = 186x104.");

        Portal_PixelResolution =     ConfigHelper.Bind("Portal", "PixelResolution",     defaultValue: PixelResolutionType.PlayerCamera, descriptionBuilder.ToString());
        Portal_ActivationRange =     ConfigHelper.Bind("Portal", "ActivationRange",     defaultValue: 10f,  "The distance in meters the player needs to be within a portal for it to render.");
        Portal_OutsideViewDistance = ConfigHelper.Bind("Portal", "OutsideViewDistance", defaultValue: 250f, "The distance you can see through an outside portal.");
        Portal_InsideViewDistance =  ConfigHelper.Bind("Portal", "InsideViewDistance",  defaultValue: 50f,  "The distance you can see through an inside portal.");

        Portal_OutsideViewDistance.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();
        Portal_InsideViewDistance.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();

        // Debug
        Debug_HideDoorObjects = ConfigHelper.Bind("Debug", "HideDoorObjects", defaultValue: false, "If enabled, will hide all of the door meshes and only show the portal screen.");
    }
}
