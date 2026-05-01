using com.github.zehsteam.PeekInside.MonoBehaviours;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.Managers;

internal static class EntranceManager
{
    private static readonly Dictionary<int, List<EntranceTeleport>> _matchingEntrances = [];
    private static readonly Dictionary<EntranceTeleport, DoorPortal> _entranceToPortals = [];

    public static void SpawnDoorPortal(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return;

        Logger.LogInfo($"[{nameof(EntranceManager)}] SpawnDoorPortal() entranceId: {entranceTeleport.entranceId}, isEntranceToBuilding: {entranceTeleport.isEntranceToBuilding}", extended: true);

        if (entranceTeleport.entranceId != 0)
            return;

        Logger.LogInfo($"[{nameof(EntranceManager)}] SpawnDoorPortal() Entrance is valid!", extended: true);

        if (entranceTeleport.isEntranceToBuilding)
        {
            if (!TryRemoveOutsideViewBlocker(entranceTeleport))
                return;
        }
        else
        {
            if (!TryRemoveInsideViewBlocker())
                return;
        }

        Logger.LogInfo($"[{nameof(EntranceManager)}] SpawnDoorPortal() Removed view blockers!", extended: true);

        Vector3 position = entranceTeleport.transform.position;
        Quaternion rotation = entranceTeleport.transform.rotation;

        GameObject gameObject = Object.Instantiate(Assets.DoorPortalPrefab, position, rotation);
        DoorPortal doorPortal = gameObject.GetComponent<DoorPortal>();

        doorPortal.SetEntranceTeleport(entranceTeleport);

        if (_matchingEntrances.TryGetValue(entranceTeleport.entranceId, out List<EntranceTeleport> entranceList))
        {
            entranceList.Add(entranceTeleport);

            Logger.LogInfo($"[{nameof(EntranceManager)}] SpawnDoorPortal() _matchingEntrances added to existing. Total count: {entranceList.Count}", extended: true);
        }
        else
        {
            _matchingEntrances[entranceTeleport.entranceId] = [entranceTeleport];

            Logger.LogInfo($"[{nameof(EntranceManager)}] SpawnDoorPortal() _matchingEntrances started new", extended: true);
        }

        _entranceToPortals.Add(entranceTeleport, doorPortal);
        
        Logger.LogInfo($"[{nameof(EntranceManager)}] SpawnDoorPortal() Spawned prefab!", extended: true);
    }

    private static bool TryRemoveOutsideViewBlocker(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport.thisEntranceAnimator == null)
            return false;

        Transform parentTransform = entranceTeleport.thisEntranceAnimator.transform;

        Transform targetTransform = parentTransform.Find("Plane");
        if (targetTransform == null) return false;

        targetTransform.gameObject.SetActive(false);
        return true;
    }

    private static bool TryRemoveInsideViewBlocker()
    {
        GameObject parentObject = GameObject.Find("Systems/LevelGeneration/LevelGenerationRoot/StartRoom(Clone)/FactoryEntranceTeleVisualDoorsContainer");
        if (parentObject == null) return false;

        Transform targetTransform = parentObject.transform.Find("LightBehindDoor");
        if (targetTransform == null) return false;

        targetTransform.gameObject.SetActive(false);
        return true;
    }

    public static void LinkPortals()
    {
        Logger.LogInfo($"[{nameof(EntranceManager)}] Linking portals!", extended: true);

        foreach (var entranceList in _matchingEntrances.Values)
        {
            if (entranceList.Count != 2)
                continue;

            EntranceTeleport left = entranceList[0];
            EntranceTeleport right = entranceList[1];

            DoorPortal leftPortal = _entranceToPortals.GetValueOrDefault(left);
            DoorPortal rightPortal = _entranceToPortals.GetValueOrDefault(right);

            Logger.LogInfo($"[{nameof(EntranceManager)}] Found two portals to link.", extended: true);

            DoorPortal.LinkPortals(leftPortal, rightPortal);
        }
    }

    public static void OnShipHasLeft()
    {
        _matchingEntrances.Clear();
        _entranceToPortals.Clear();

        Logger.LogInfo($"[{nameof(EntranceManager)}] Reset!", extended: true);
    }
}
