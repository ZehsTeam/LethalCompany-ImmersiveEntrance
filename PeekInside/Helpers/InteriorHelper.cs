using com.github.zehsteam.PeekInside.Extensions;
using com.github.zehsteam.PeekInside.Objects;
using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace com.github.zehsteam.PeekInside.Helpers;

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



    public static GameObject GetDoorViewBlocker(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return null;

        if (!TryGetVisualDoorsContainer(entranceTeleport, out Transform container))
            return null;

        if (container.TryFind("LightBehindDoor", out Transform viewBlocker))
        {
            return viewBlocker.gameObject;
        }

        return null;
    }

    public static List<GameObject> GetDoorObjects(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return [];

        if (!TryGetVisualDoorsContainer(entranceTeleport, out Transform container))
            return [];

        string[] names = [
            // Facility/Mineshaft
            "SteelDoorFake", "SteelDoorFake (1)", "DoorFrame",

            // Manor
            "DoorMesh", "DoorMesh (1)", "WideDoorFrame (1)"
        ];

        List<GameObject> doorObjects = [];

        foreach (var child in container.GetChildren())
        {
            if (names.Contains(child.name, StringComparer.OrdinalIgnoreCase))
            {
                doorObjects.Add(child.gameObject);
            }
        }

        return doorObjects;
    }

    public static Transform GetVisualDoorsContainer(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport.thisEntranceAnimator != null)
        {
            return entranceTeleport.thisEntranceAnimator.transform;
        }

        return GameObject.FindGameObjectWithTag("InsideEntranceDoor")?.transform ?? null;
    }

    public static bool TryGetVisualDoorsContainer(EntranceTeleport entranceTeleport, out Transform transform)
    {
        transform = GetVisualDoorsContainer(entranceTeleport);
        return transform != null;
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
        return RoundManager.Instance.dungeonFlowTypes[RoundManager.Instance.currentDungeonType].dungeonFlow;
    }
}
