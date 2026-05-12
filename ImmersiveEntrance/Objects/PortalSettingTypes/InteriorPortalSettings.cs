using BepInEx.Configuration;
using com.github.zehsteam.ImmersiveEntrance.Helpers;
using DunGen.Graph;
using System;

namespace com.github.zehsteam.ImmersiveEntrance.Objects.PortalSettingTypes;

public class InteriorPortalSettings : PortalSettings
{
    public struct InteriorDefaultValues
    {
        public Func<EntranceDoorReplacement> GetDoorReplacement { get; set; }

        public InteriorDefaultValues()
        {

        }

        public InteriorDefaultValues(Func<EntranceDoorReplacement> getDoorReplacement = null)
        {
            GetDoorReplacement = getDoorReplacement;
        }
    }

    public InteriorType InteriorType { get; private set; } = InteriorType.Unknown;

    /// <summary>
    /// Only assign when InteriorType is Unknown
    /// </summary>
    public string DungeonFlowName { get; private set; }

    public bool HasDoorReplacement => GetDoorReplacement != null;
    public Func<EntranceDoorReplacement> GetDoorReplacement => _interiorDefaultValues.GetDoorReplacement;
    public ConfigEntry<bool> ReplaceDoor { get; private set; }

    public override string ConfigSection => $"Interior: {GetInteriorName()}";

    private InteriorDefaultValues _interiorDefaultValues;

    private bool _boundConfigs;

    public InteriorPortalSettings(InteriorType interiorType, InteriorDefaultValues interiorDefaultValues, DefaultValues defaultValues) : base(defaultValues)
    {
        InteriorType = interiorType;
        _interiorDefaultValues = interiorDefaultValues;
    }

    public InteriorPortalSettings(string dungeonFlowName, InteriorDefaultValues interiorDefaultValues, DefaultValues defaultValues) : base(defaultValues)
    {
        InteriorType = InteriorType.Unknown;
        DungeonFlowName = dungeonFlowName;
        _interiorDefaultValues = interiorDefaultValues;
    }

    public override void BindConfigs()
    {
        if (_boundConfigs) return;
        _boundConfigs = true;

        base.BindConfigs();

        if (HasDoorReplacement)
        {
            ReplaceDoor = ConfigHelper.Bind(ConfigSection, "ReplaceDoor", defaultValue: true, "If enabled, will replace the door model.");
        }
    }

    public bool Matches(DungeonFlow dungeonFlow)
    {
        InteriorType interiorType = InteriorHelper.GetInteriorType(dungeonFlow);

        if (interiorType == InteriorType.Unknown)
        {
            return dungeonFlow.name.Equals(DungeonFlowName, StringComparison.OrdinalIgnoreCase);
        }

        return interiorType == InteriorType;
    }

    public string GetInteriorName()
    {
        return InteriorType == InteriorType.Unknown ? DungeonFlowName : InteriorType.ToString();
    }
}
