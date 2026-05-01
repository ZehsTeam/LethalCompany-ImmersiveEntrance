using com.github.zehsteam.PeekInside.MonoBehaviours;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.Patches;

[HarmonyPatch(typeof(EntranceTeleport))]
internal static class EntranceTeleport_Patches
{
    private static readonly Dictionary<int, List<EntranceTeleport>> _matchingEntrances = [];
    private static readonly Dictionary<EntranceTeleport, DoorPortal> _entranceToPortals = [];

    [HarmonyPatch(nameof(EntranceTeleport.Awake))]
    [HarmonyPostfix]
    private static void Awake_Patch(EntranceTeleport __instance)
    {
        SpawnDoorPortal(__instance);
    }

    private static void SpawnDoorPortal(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport.entranceId != 0)
            return;

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

        Vector3 position = entranceTeleport.transform.position;
        Quaternion rotation = entranceTeleport.transform.rotation;

        GameObject gameObject = Object.Instantiate(Assets.DoorPortalPrefab, position, rotation);
        DoorPortal doorPortal = gameObject.GetComponent<DoorPortal>();

        if (_matchingEntrances.TryGetValue(entranceTeleport.entranceId, out List<EntranceTeleport> entranceList))
        {
            entranceList.Add(entranceTeleport);
        }
        else
        {
            _matchingEntrances[entranceTeleport.entranceId] = [entranceTeleport];
        }

        _entranceToPortals.Add(entranceTeleport, doorPortal);

        Logger.LogInfo($"[{nameof(EntranceTeleport_Patches)}] Spawned door portal!", extended: true);
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

    public static void OnShipHasLeft()
    {
        _matchingEntrances.Clear();
        _entranceToPortals.Clear();
    }

    public static void LinkPortals()
    {
        foreach (var entranceList in _matchingEntrances.Values)
        {
            if (entranceList.Count != 2)
                continue;

            EntranceTeleport left = entranceList[0];
            EntranceTeleport right = entranceList[1];

            DoorPortal leftPortal = _entranceToPortals.GetValueOrDefault(left);
            DoorPortal rightPortal = _entranceToPortals.GetValueOrDefault(right);

            DoorPortal.LinkPortals(leftPortal, rightPortal);
        }
    }
}
