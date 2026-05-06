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
        public bool OverrideViewRange { get; set; }
        public float ViewRange { get; set; }

        public DefaultValues()
        {
            Enabled = true;
            UseDynamicPivot = true;
            ViewRange = 50f;
        }

        public DefaultValues(bool enabled = true, bool useDynamicPivot = true, Vector3? pivotPositionOffset = null, Padding? screenCrop = null, float? viewRange = null)
        {
            Enabled = enabled;
            UseDynamicPivot = useDynamicPivot;
            PivotPositionOffset = pivotPositionOffset ?? Vector3.zero;
            ScreenCrop = screenCrop ?? new Padding();
            OverrideViewRange = viewRange.HasValue;
            ViewRange = viewRange ?? 50f;
        }
    }

    public ConfigEntry<bool> Enabled { get; private set; }
    public bool UseDynamicPivot => _defaultValues.UseDynamicPivot;
    public Vector3 PivotPositionOffset => _defaultValues.PivotPositionOffset;
    public Padding ScreenCrop => _defaultValues.ScreenCrop;
    public ConfigEntry<bool> OverrideViewRange { get; private set; }
    public ConfigEntry<float> ViewRange { get; private set; }

    private DefaultValues _defaultValues;

    public abstract string ConfigSection { get; }

    public PortalSettings(DefaultValues defaultValues)
    {
        _defaultValues = defaultValues;
    }

    public virtual void BindConfigs()
    {
        Enabled =           ConfigHelper.Bind(ConfigSection, "Enabled",           defaultValue: _defaultValues.Enabled,           $"Enables portals.");
        OverrideViewRange = ConfigHelper.Bind(ConfigSection, "OverrideViewRange", defaultValue: _defaultValues.OverrideViewRange, "If enabled, the portal will use the view range defined in the config.");
        ViewRange =         ConfigHelper.Bind(ConfigSection, "ViewRange",         defaultValue: _defaultValues.ViewRange,         "The view range for this portal. Requires OverrideViewRange to be enabled.");

        OverrideViewRange.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();
        ViewRange.SettingChanged += (_, _) => DoorPortal.OnConfigSettingsChanged();
    }
}
