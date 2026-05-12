using com.github.zehsteam.ImmersiveEntrance.Helpers;
using DunGen.Graph;
using System.Collections.Generic;
using System.Linq;

namespace com.github.zehsteam.ImmersiveEntrance.Objects.PortalSettingTypes;

public class PortalSettingsDatabase<T> where T : PortalSettings
{
    public IReadOnlyList<T> Entries => _entries;

    protected readonly List<T> _entries = [];

    public virtual bool AddEntry(T entry)
    {
        if (_entries.Contains(entry))
            return false;

        _entries.Add(entry);
        return true;
    }

    public virtual bool RemoveEntry(T entry)
    {
        return _entries.Remove(entry);
    }

    public virtual bool ContainsEntry(T entry)
    {
        return _entries.Contains(entry);
    }
}

public class MoonPortalSettingsDatabase : PortalSettingsDatabase<MoonPortalSettings>
{
    public bool AddEntry(SelectableLevel level, PortalSettings.DefaultValues? defaultValues = null)
    {
        if (!level.planetHasTime)
            return false;

        if (ContainsEntry(level))
            return false;

        return AddEntry(new MoonPortalSettings(level.PlanetName, defaultValues ?? new PortalSettings.DefaultValues()));
    }

    public void BindConfigs()
    {
        List<MoonPortalSettings> sortedEntries = [.. _entries.OrderBy(x => x.GetStrippedPlanetName())];

        foreach (var entry in sortedEntries)
        {
            entry.BindConfigs();
        }
    }

    public MoonPortalSettings GetEntry(SelectableLevel level)
    {
        return Entries.FirstOrDefault(x => x.Matches(level));
    }

    public bool TryGetEntry(SelectableLevel level, out MoonPortalSettings settings)
    {
        settings = GetEntry(level);
        return settings != null;
    }

    public bool ContainsEntry(SelectableLevel level)
    {
        return TryGetEntry(level, out _);
    }

    public MoonPortalSettings GetEntryForCurrentMoon()
    {
        return GetEntry(StartOfRound.Instance?.currentLevel);
    }

    public bool TryGetEntryForCurrentMoon(out MoonPortalSettings settings)
    {
        settings = GetEntryForCurrentMoon();
        return settings != null;
    }
}

public class InteriorPortalSettingsDatabase : PortalSettingsDatabase<InteriorPortalSettings>
{
    public bool AddEntry(InteriorType interiorType, InteriorPortalSettings.InteriorDefaultValues? interiorDefaultValues = null, PortalSettings.DefaultValues? defaultValues = null)
    {
        interiorDefaultValues ??= new InteriorPortalSettings.InteriorDefaultValues();
        defaultValues ??= new PortalSettings.DefaultValues();

        return AddEntry(new InteriorPortalSettings(interiorType, interiorDefaultValues.Value, defaultValues.Value));
    }

    public bool AddEntry(DungeonFlow dungeonFlow, InteriorPortalSettings.InteriorDefaultValues? interiorDefaultValues = null, PortalSettings.DefaultValues? defaultValues = null)
    {
        interiorDefaultValues ??= new InteriorPortalSettings.InteriorDefaultValues();
        defaultValues ??= new PortalSettings.DefaultValues();

        InteriorType interiorType = InteriorHelper.GetInteriorType(dungeonFlow);

        if (interiorType == InteriorType.Unknown)
        {
            return AddEntry(new InteriorPortalSettings(dungeonFlow.name, interiorDefaultValues.Value, defaultValues.Value));
        }

        return AddEntry(new InteriorPortalSettings(interiorType, interiorDefaultValues.Value, defaultValues.Value));
    }

    public void BindConfigs()
    {
        List<InteriorPortalSettings> sortedEntries = [.. _entries.OrderBy(x => x.GetInteriorName())];

        foreach (var entry in sortedEntries)
        {
            entry.BindConfigs();
        }
    }

    public InteriorPortalSettings GetEntry(DungeonFlow dungeonFlow)
    {
        return Entries.FirstOrDefault(x => x.Matches(dungeonFlow));
    }

    public bool TryGetEntry(DungeonFlow dungeonFlow, out InteriorPortalSettings settings)
    {
        settings = GetEntry(dungeonFlow);
        return settings != null;
    }

    public bool ContainsEntry(DungeonFlow dungeonFlow)
    {
        return TryGetEntry(dungeonFlow, out _);
    }

    public InteriorPortalSettings GetEntryForCurrentInterior()
    {
        return GetEntry(InteriorHelper.GetCurrentDungeonFlow());
    }

    public bool TryGetEntryForCurrentInterior(out InteriorPortalSettings settings)
    {
        settings = GetEntryForCurrentInterior();
        return settings != null;
    }
}
