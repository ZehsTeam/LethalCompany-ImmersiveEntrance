using com.github.zehsteam.ImmersiveEntrance.Objects;
using com.github.zehsteam.ImmersiveEntrance.Objects.PortalSettingTypes;
using DunGen.Graph;
using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Managers;

internal static class PortalSettingsManager
{
    public static MoonPortalSettingsDatabase MoonDatabase { get; private set; } = new();
    public static InteriorPortalSettingsDatabase InteriorDatabase { get; private set; } = new();

    private static readonly MoonPortalSettingsDatabase _predefinedMoonDatabase = new();
    private static readonly InteriorPortalSettingsDatabase _predefinedInteriorDatabase = new();

    private static bool _initialized;

    static PortalSettingsManager()
    {
        AddPredefinedEntries();
    }

    private static void AddPredefinedEntries()
    {
        AddPredefinedMoonEntries();
        AddPredefinedInteriorEntries();

        // Modded
        AddMonesInteriorsSettings();
        AddSlaughterhouseSettings();
    }

    #region Predefined Vanilla Entries
    private static void AddPredefinedMoonEntries()
    {
        _predefinedMoonDatabase.AddEntry(new MoonPortalSettings(
            planetName: "41 Experimentation",
            new PortalSettings.DefaultValues(
                useDynamicPivot: false,
                pivotPositionOffset: new Vector3(0.02f, 0.03417349f, 0.203743f)
            )
        ));

        _predefinedMoonDatabase.AddEntry(new MoonPortalSettings(
            planetName: "56 Vow",
            new PortalSettings.DefaultValues(
                pivotPositionOffset: new Vector3(0.04f, 0f, 0f)
            )
        ));

        _predefinedMoonDatabase.AddEntry(new MoonPortalSettings(
            planetName: "85 Rend",
            new PortalSettings.DefaultValues(
                viewDistance: 75f
            )
        ));

        _predefinedMoonDatabase.AddEntry(new MoonPortalSettings(
            planetName: "7 Dine",
            new PortalSettings.DefaultValues(
                pivotPositionOffset: new Vector3(0.06f, 0f, 0f),
                screenCrop: new Padding(left: 0f, right: 0f, top: 0.03f, bottom: 0f)
            )
        ));
    }

    private static void AddPredefinedInteriorEntries()
    {
        _predefinedInteriorDatabase.AddEntry(new InteriorPortalSettings(
            InteriorType.Facility,
            new InteriorPortalSettings.InteriorDefaultValues(),
            new PortalSettings.DefaultValues(
                useDynamicPivot: false,
                pivotPositionOffset: new Vector3(-0.1f, 0.03f, 0.067f) // GET Y POS AGAIN
            )
        ));

        _predefinedInteriorDatabase.AddEntry(new InteriorPortalSettings(
            InteriorType.Mansion,
            new InteriorPortalSettings.InteriorDefaultValues(
                getDoorReplacement: () => Assets.MansionEntranceDoorReplacement
            ),
            new PortalSettings.DefaultValues(
                pivotPositionOffset: new Vector3(-0.011f, 0f, 0f),
                screenCrop: new Padding(left: 0.05f, right: 0.05f, top: 0.025f, bottom: 0f)
            )
        ));

        _predefinedInteriorDatabase.AddEntry(new InteriorPortalSettings(
            InteriorType.Mineshaft,
            new InteriorPortalSettings.InteriorDefaultValues(),
            new PortalSettings.DefaultValues(
                pivotPositionOffset: new Vector3(-0.09f, 0f, 0f),
                screenCrop: new Padding(left: 0f, right: 0f, top: 0.02f, bottom: 0f),
                viewDistance: 30f
            )
        ));
    }
    #endregion

    #region Predefined Modded Entries
    private static void AddMonesInteriorsSettings()
    {
        // TODO: Test this
        _predefinedInteriorDatabase.AddEntry(new InteriorPortalSettings(
            dungeonFlowName: "EndlessHallDunFlow",
            new InteriorPortalSettings.InteriorDefaultValues(),
            new PortalSettings.DefaultValues(
                pivotPositionOffset: new Vector3(-0.055f, 0f, 0f),
                screenCrop: new Padding(left: 0.026f, right: 0.026f, top: 0.026f, bottom: 0f)
            )
        ));

        // TODO: Test this
        _predefinedInteriorDatabase.AddEntry(new InteriorPortalSettings(
            dungeonFlowName: "HauntedHotelDunFlow",
            new InteriorPortalSettings.InteriorDefaultValues(
                getDoorReplacement: () => Assets.MansionEntranceDoorReplacement
            ),
            new PortalSettings.DefaultValues(
                pivotPositionOffset: new Vector3(-0.011f, 0f, 0f),
                screenCrop: new Padding(left: 0.05f, right: 0.05f, top: 0.025f, bottom: 0f)
            )
        ));

        // Need to add more
    }

    private static void AddSlaughterhouseSettings()
    {
        _predefinedInteriorDatabase.AddEntry(new InteriorPortalSettings(
            dungeonFlowName: "SlaughterhouseFlow",
            new InteriorPortalSettings.InteriorDefaultValues(),
            new PortalSettings.DefaultValues(
                viewDistance: 100f
            )
        ));
    }
    #endregion

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        PopulateEntries();
        BindConfigs();
    }

    private static void PopulateEntries()
    {
        PopulateMoonEntries();
        PopulateInteriorEntries();
    }

    private static void BindConfigs()
    {
        InteriorDatabase.BindConfigs();
        MoonDatabase.BindConfigs();
    }

    #region Populate Entries
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

    private static bool AddMoonEntry(SelectableLevel level)
    {
        if (_predefinedMoonDatabase.TryGetEntry(level, out MoonPortalSettings predefinedSettings))
        {
            return MoonDatabase.AddEntry(predefinedSettings);
        }

        return MoonDatabase.AddEntry(level);
    }

    private static bool AddInteriorEntry(DungeonFlow dungeonFlow)
    {
        if (_predefinedInteriorDatabase.TryGetEntry(dungeonFlow, out InteriorPortalSettings predefinedSettings))
        {
            return InteriorDatabase.AddEntry(predefinedSettings);
        }

        return InteriorDatabase.AddEntry(dungeonFlow);
    }
    #endregion
}
