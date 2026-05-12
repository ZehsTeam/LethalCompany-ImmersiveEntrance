using System;

namespace com.github.zehsteam.ImmersiveEntrance.Objects.PortalSettingTypes;

public class MoonPortalSettings : PortalSettings
{
    public string PlanetName { get; private set; }

    public override string ConfigSection => $"Moon: {PlanetName}";

    public MoonPortalSettings(string planetName, DefaultValues defaultValues) : base (defaultValues)
    {
        PlanetName = planetName;
    }

    public bool Matches(SelectableLevel level)
    {
        return PlanetName.Equals(level.PlanetName, StringComparison.OrdinalIgnoreCase);
    }

    public string GetStrippedPlanetName()
    {
        if (!PlanetName.Contains(" "))
            return PlanetName;

        return PlanetName.Substring(PlanetName.IndexOf(" "));
    }
}
