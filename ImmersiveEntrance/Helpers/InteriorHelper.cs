using com.github.zehsteam.ImmersiveEntrance.Objects;
using DunGen.Graph;
using System;

namespace com.github.zehsteam.ImmersiveEntrance.Helpers;

internal static class InteriorHelper
{
    public static void RenderInterior()
    {
        if (StartOfRound.Instance == null)
            return;

        AdjacentRoomCullingModified occlusionCuller = StartOfRound.Instance.occlusionCuller;
        if (occlusionCuller == null) return;

        if (!occlusionCuller.enabled)
            return;

        occlusionCuller.SetToStartTile();
    }

    public static string GetCurrentInteriorName()
    {
        return GetInteriorName(GetCurrentDungeonFlow());
    }

    public static string GetInteriorName(DungeonFlow dungeonFlow)
    {
        if (dungeonFlow == null)
            return "DungeonFlow is null";

        InteriorType interiorType = GetInteriorType(dungeonFlow);

        if (interiorType == InteriorType.Unknown)
        {
            return dungeonFlow.name;
        }

        return interiorType.ToString();
    }

    public static InteriorType GetCurrentInteriorType()
    {
        return GetInteriorType(GetCurrentDungeonFlow());
    }

    public static InteriorType GetInteriorType(DungeonFlow dungeonFlow)
    {
        string name = dungeonFlow.name;

        if (name.StartsWith("Level1Flow", StringComparison.OrdinalIgnoreCase))
        {
            return InteriorType.Facility;
        }

        if (name.StartsWith("Level2Flow", StringComparison.OrdinalIgnoreCase))
        {
            return InteriorType.Mansion;
        }

        if (name.StartsWith("Level3Flow", StringComparison.OrdinalIgnoreCase))
        {
            return InteriorType.Mineshaft;
        }

        return InteriorType.Unknown;
    }

    public static DungeonFlow GetCurrentDungeonFlow()
    {
        if (RoundManager.Instance == null)
            return null;

        int index = RoundManager.Instance.currentDungeonType;

        IndoorMapType[] array = RoundManager.Instance.dungeonFlowTypes;

        if (index < 0 || index > array.Length - 1)
            return null;

        return array[index].dungeonFlow;
    }
}
