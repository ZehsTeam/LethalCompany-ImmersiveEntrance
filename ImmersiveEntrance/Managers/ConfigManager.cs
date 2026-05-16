using BepInEx.Configuration;
using com.github.zehsteam.ImmersiveEntrance.Helpers;
using com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;
using com.github.zehsteam.ImmersiveEntrance.Objects;
using System.Text;

namespace com.github.zehsteam.ImmersiveEntrance.Managers;

internal static class ConfigManager
{
    public static ConfigFile ConfigFile { get; private set; }

    // Misc
    public static ConfigEntry<bool> Misc_ExtendedLogging { get; private set; }

    // Portal
    public static ConfigEntry<bool> Portal_Enabled { get; private set; }
    public static ConfigEntry<float> Portal_ActivationRange { get; private set; }

    // Portal Graphics
    public static ConfigEntry<PixelResolutionType> PortalGraphics_PixelResolution { get; private set; }
    public static ConfigEntry<float> PortalGraphics_OutsideViewDistance { get; private set; }
    public static ConfigEntry<float> PortalGraphics_InsideViewDistance { get; private set; }
    public static ConfigEntry<bool> PortalGraphics_FogEnabled { get; private set; }
    public static ConfigEntry<bool> PortalGraphics_CustomPassEnabled { get; private set; }

    // Debug
    public static ConfigEntry<bool> Debug_HideDoorObjects { get; private set; }
    public static ConfigEntry<bool> Debug_ExcludeFogBehindScreen { get; private set; }
    public static ConfigEntry<float> Debug_MaxNearClipPlane { get; private set; }
    public static ConfigEntry<NearClipPlaneMode> Debug_NearClipPlaneMode { get; private set; }
    public static ConfigEntry<bool> Debug_UseSimulatedDeviceDepth { get; private set; }

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
        Portal_Enabled =         ConfigHelper.Bind("Portal", "Enabled",         defaultValue: true, "Enable portals!");
        Portal_ActivationRange = ConfigHelper.Bind("Portal", "ActivationRange", defaultValue: 10f,  "The distance in meters the player needs to be within a portal for it to render.",
            acceptableValues: new AcceptableValueRange<float>(1f, 50f));

        // Portal Graphics
        var descriptionBuilder = new StringBuilder();
        descriptionBuilder.AppendLine("The rendering pixel resolution of portals.");
        descriptionBuilder.AppendLine("PlayerCamera = Automatically adjusts to the player camera's rendering resolution.");
        descriptionBuilder.AppendLine("Default = 860x520.");
        descriptionBuilder.AppendLine("Performance = 620x364.");
        descriptionBuilder.AppendLine("UltraPerformance = 400x260.");
        descriptionBuilder.AppendLine("Retro = 186x104.");

        PortalGraphics_PixelResolution =     ConfigHelper.Bind("Portal Graphics", "PixelResolution",     defaultValue: PixelResolutionType.PlayerCamera, descriptionBuilder.ToString());
        PortalGraphics_OutsideViewDistance = ConfigHelper.Bind("Portal Graphics", "OutsideViewDistance", defaultValue: 250f, "The distance you can see through an outside portal.",
            acceptableValues: new AcceptableValueRange<float>(0.06f, 250f));
        PortalGraphics_InsideViewDistance =  ConfigHelper.Bind("Portal Graphics", "InsideViewDistance",  defaultValue: 50f,  "The distance you can see through an inside portal.",
            acceptableValues: new AcceptableValueRange<float>(0.06f, 100f));
        PortalGraphics_FogEnabled =          ConfigHelper.Bind("Portal Graphics", "FogEnabled",          defaultValue: true, "Enable the rendering of fog.");
        PortalGraphics_CustomPassEnabled =   ConfigHelper.Bind("Portal Graphics", "CustomPassEnabled",   defaultValue: true, "Enable custom passes to render. This applies the Lethal Company signature shading to everything being viewed.");

        PortalGraphics_OutsideViewDistance.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();
        PortalGraphics_InsideViewDistance.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();
        PortalGraphics_FogEnabled.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();
        PortalGraphics_CustomPassEnabled.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();

        // Debug
        Debug_HideDoorObjects =  ConfigHelper.Bind("Debug", "HideDoorObjects",  defaultValue: false, "If enabled, will hide all of the door meshes and only show the portal screen.");
        Debug_ExcludeFogBehindScreen = ConfigHelper.Bind("Debug", "ExcludeFogBehindScreen",  defaultValue: false, "If enabled, will exclude fog that is behind the portal screen from rendering.");
        Debug_MaxNearClipPlane = ConfigHelper.Bind("Debug", "MaxNearClipPlane", defaultValue: 1f,    "The max value portal cameras can have their near clip plane set to.",
            acceptableValues: new AcceptableValueRange<float>(0.01f, 10f));
        Debug_NearClipPlaneMode = ConfigHelper.Bind("Debug", "NearClipPlaneMode", defaultValue: NearClipPlaneMode.Normal, "The method the portal cameras use for calculating their near clip plane.");
        Debug_UseSimulatedDeviceDepth = ConfigHelper.Bind("Debug", "UseSimulatedDeviceDepth", defaultValue: true, "");

        Debug_HideDoorObjects.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();
        Debug_ExcludeFogBehindScreen.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();
    }
}
