using com.github.zehsteam.ImmersiveEntrance.Helpers;
using com.github.zehsteam.ImmersiveEntrance.Objects;
using com.github.zehsteam.ImmersiveEntrance.Objects.PortalSettingTypes;
using DunGen.Graph;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Managers;

internal static class PortalSettingsManager
{
    public static IReadOnlyList<MoonPortalSettings> MoonEntries => _moonEntries;
    private static readonly List<MoonPortalSettings> _moonEntries = [];
    private static readonly List<MoonPortalSettings> _predefinedMoonEntries = [];

    public static IReadOnlyList<InteriorPortalSettings> InteriorEntries => _interiorEntries;
    private static readonly List<InteriorPortalSettings> _interiorEntries = [];
    private static readonly List<InteriorPortalSettings> _predefinedInteriorEntries = [];

    private static bool _initialized;

    static PortalSettingsManager()
    {
        // Predefined Moon Entries
        _predefinedMoonEntries.Add(new MoonPortalSettings(
            planetName: "85 Rend",
            new PortalSettings.DefaultValues(
                viewDistance: 75f
            )
        ));

        // Predefined Interior Entries
        _predefinedInteriorEntries.Add(new InteriorPortalSettings(
            InteriorType.Facility,
            new InteriorPortalSettings.InteriorDefaultValues(),
            new PortalSettings.DefaultValues(
                useDynamicPivot: false,
                pivotPositionOffset: new Vector3(0f, 0f, -0.133f)
            )
        ));

        _predefinedInteriorEntries.Add(new InteriorPortalSettings(
            InteriorType.Mansion,
            new InteriorPortalSettings.InteriorDefaultValues(
                getDoorReplacement: () => Assets.MansionEntranceDoorReplacement
            ),
            new PortalSettings.DefaultValues(
                pivotPositionOffset: new Vector3(-0.038f, 0f, 0f),
                screenCrop: new Padding(left: 0.05f, right: 0.05f, top: 0.025f, bottom: 0f)
            )
        ));
        
        _predefinedInteriorEntries.Add(new InteriorPortalSettings(
            InteriorType.Mineshaft,
            new InteriorPortalSettings.InteriorDefaultValues(),
            new PortalSettings.DefaultValues(
                pivotPositionOffset: new Vector3(-0.105f, 0f, 0f),
                viewDistance: 30f
            )
        ));

        AddMonesInteriorsSettings();
    }

    #region Modded Interior Settings
    private static void AddMonesInteriorsSettings()
    {
        // TODO: Test this
        _predefinedInteriorEntries.Add(new InteriorPortalSettings(
            dungeonFlowName: "EndlessHallDunFlow",
            new InteriorPortalSettings.InteriorDefaultValues(),
            new PortalSettings.DefaultValues(
                pivotPositionOffset: new Vector3(-0.055f, 0f, 0f),
                screenCrop: new Padding(left: 0.026f, right: 0.026f, top: 0.026f, bottom: 0f)
            )
        ));

        // Need to add more
    }
    #endregion

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        PopulateEntries();
        BindConfigs();
    }

    #region Register
    private static void PopulateEntries()
    {
        PopulateMoonEntries();
        PopulateInteriorEntries();
    }

    private static void PopulateMoonEntries()
    {
        if (StartOfRound.Instance == null)
        {
            Logger.LogError($"[{nameof(PortalSettingsManager)}] Failed to populate moon entries. StartOfRound instance is null.");
            return;
        }

        foreach (var level in StartOfRound.Instance.levels)
        {
            AddMoonEntry(level);
        }
    }

    private static void PopulateInteriorEntries()
    {
        if (RoundManager.Instance == null)
        {
            Logger.LogError($"[{nameof(PortalSettingsManager)}] Failed to populate interior entries. RoundManager instance is null.");
            return;
        }

        foreach (var indoorMapType in RoundManager.Instance.dungeonFlowTypes)
        {
            AddInteriorEntry(indoorMapType.dungeonFlow);
        }
    }

    private static void AddMoonEntry(SelectableLevel level)
    {
        if (!level.planetHasTime)
            return;

        if (_moonEntries.Any(x => x.Matches(level)))
            return;

        MoonPortalSettings predefinedSettings = _predefinedMoonEntries.FirstOrDefault(x => x.Matches(level));

        if (predefinedSettings != null)
        {
            _moonEntries.Add(predefinedSettings);
            return;
        }

        _moonEntries.Add(new MoonPortalSettings(level.PlanetName, new PortalSettings.DefaultValues()));
    }

    private static void AddInteriorEntry(DungeonFlow dungeonFlow)
    {
        if (_interiorEntries.Any(x => x.Matches(dungeonFlow)))
            return;

        InteriorPortalSettings predefinedSettings = _predefinedInteriorEntries.FirstOrDefault(x => x.Matches(dungeonFlow));

        if (predefinedSettings != null)
        {
            _interiorEntries.Add(predefinedSettings);
            return;
        }

        var interiorDefaultValues = new InteriorPortalSettings.InteriorDefaultValues();
        var defaultValues = new PortalSettings.DefaultValues();

        InteriorType interiorType = InteriorHelper.GetInteriorType(dungeonFlow);

        if (interiorType == InteriorType.Unknown)
        {
            _interiorEntries.Add(new InteriorPortalSettings(dungeonFlow.name, interiorDefaultValues, defaultValues));
            return;
        }

        _interiorEntries.Add(new InteriorPortalSettings(interiorType, interiorDefaultValues, defaultValues));
    }
    #endregion

    #region Config
    private static void BindConfigs()
    {
        BindInteriorConfigs();
        BindMoonConfigs();
    }

    private static void BindMoonConfigs()
    {
        List<MoonPortalSettings> sortedEntries = [.. _moonEntries.OrderBy(x => x.GetStrippedPlanetName())];

        foreach (var entry in sortedEntries)
        {
            entry.BindConfigs();
        }
    }

    private static void BindInteriorConfigs()
    {
        List<InteriorPortalSettings> sortedEntries = [.. _interiorEntries.OrderBy(x => x.GetInteriorName())];

        foreach (var entry in sortedEntries)
        {
            entry.BindConfigs();
        }
    }
    #endregion

    #region Get
    public static MoonPortalSettings GetCurrentMoonSettings()
    {
        return GetMoonSettings(StartOfRound.Instance?.currentLevel);
    }

    public static InteriorPortalSettings GetCurrentInteriorSettings()
    {
        return GetInteriorSettings(InteriorHelper.GetCurrentDungeonFlow());
    }

    public static MoonPortalSettings GetMoonSettings(SelectableLevel level)
    {
        return _moonEntries.FirstOrDefault(x => x.Matches(level));
    }

    public static InteriorPortalSettings GetInteriorSettings(DungeonFlow dungeonFlow)
    {
        return _interiorEntries.FirstOrDefault(x => x.Matches(dungeonFlow));
    }
    #endregion
}
