using BepInEx.Configuration;
using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.MonoBehaviours;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.Objects.PortalSettingTypes;

internal abstract class PortalSettings
{
    public struct DefaultValues
    {
        public bool Enabled { get; set; }
        public bool UseDynamicPivot { get; set; }
        public Vector3 PivotPositionOffset { get; set; }
        public Padding ScreenCrop { get; set; }
        public bool UseViewDistance { get; set; }
        public float ViewDistance { get; set; }

        public DefaultValues()
        {
            Enabled = true;
            UseDynamicPivot = true;
            ViewDistance = 50f;
        }

        public DefaultValues(bool enabled = true, bool useDynamicPivot = true, Vector3? pivotPositionOffset = null, Padding? screenCrop = null, float? viewDistance = null)
        {
            Enabled = enabled;
            UseDynamicPivot = useDynamicPivot;
            PivotPositionOffset = pivotPositionOffset ?? Vector3.zero;
            ScreenCrop = screenCrop ?? new Padding();
            UseViewDistance = viewDistance.HasValue;
            ViewDistance = viewDistance ?? 50f;
        }
    }

    public ConfigEntry<bool> Enabled { get; private set; }
    public bool UseDynamicPivot => _defaultValues.UseDynamicPivot;
    public Vector3 PivotPositionOffset => _defaultValues.PivotPositionOffset;
    public Padding ScreenCrop => _defaultValues.ScreenCrop;
    public ConfigEntry<bool> UseViewDistance { get; private set; }
    public ConfigEntry<float> ViewDistance { get; private set; }

    private DefaultValues _defaultValues;

    public abstract string ConfigSection { get; }

    public PortalSettings(DefaultValues defaultValues)
    {
        _defaultValues = defaultValues;
    }

    public virtual void BindConfigs()
    {
        Enabled =         ConfigHelper.Bind(ConfigSection, "Enabled",         defaultValue: _defaultValues.Enabled,         $"Enable this portal. If disabled, will also disable the other portal.");
        UseViewDistance = ConfigHelper.Bind(ConfigSection, "UseViewDistance", defaultValue: _defaultValues.UseViewDistance, "If enabled, this portal will use the view distance in this config instead of the global view distance config.");
        ViewDistance =    ConfigHelper.Bind(ConfigSection, "ViewDistance",    defaultValue: _defaultValues.ViewDistance,    "The distance you can see through this portal. Requires UseViewDistance to be enabled.");

        UseViewDistance.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();
        ViewDistance.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();
    }
}
