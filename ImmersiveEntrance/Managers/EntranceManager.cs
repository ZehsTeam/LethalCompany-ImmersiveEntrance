using com.github.zehsteam.ImmersiveEntrance.Extensions;
using com.github.zehsteam.ImmersiveEntrance.Helpers;
using com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;
using com.github.zehsteam.ImmersiveEntrance.Objects;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.github.zehsteam.ImmersiveEntrance.Managers;

internal static class EntranceManager
{
    public static IReadOnlyDictionary<EntranceTeleport, DoorPortal> EntranceToPortals => _entranceToPortals;
    private static readonly Dictionary<EntranceTeleport, DoorPortal> _entranceToPortals = [];

    public static MainEntranceData OutsideMainEntrance { get; private set; }
    public static MainEntranceData InsideMainEntrance { get; private set; }

    private static bool _linkedMainEntrancePortals;

    public static void Reset()
    {
        OutsideMainEntrance?.Reset();
        InsideMainEntrance?.Reset();

        _linkedMainEntrancePortals = false;

        foreach (var kvp in _entranceToPortals)
        {
            DoorPortal doorPortal = kvp.Value;
            if (doorPortal == null) continue;

            Object.Destroy(doorPortal.gameObject);

            Logger.LogInfo($"[{nameof(EntranceManager)}] Reset() Destroyed DoorPortal {kvp.Key?.GetLogInfo()}", extended: true);
        }

        _entranceToPortals.Clear();
    }

    public static void SpawnDoorPortal(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return;

        if (!entranceTeleport.IsMainEntrance())
            return;

        Logger.LogInfo($"[{nameof(EntranceManager)}] Attempting to spawn main entrance door portal! {entranceTeleport.GetLogInfo()}");

        if (_entranceToPortals.ContainsKey(entranceTeleport))
        {
            Logger.LogWarning($"[{nameof(EntranceManager)}] Failed to spawn main entrance door portal. This EntranceTeleport is already in use!");
            return;
        }

        if (_entranceToPortals.Count >= 2)
        {
            Logger.LogError($"[{nameof(EntranceManager)}] Failed to spawn main entrance door portal. There are already {_entranceToPortals.Count} entrance portals.");
            return;
        }

        MainEntranceData mainEntrance;

        if (entranceTeleport.IsOutside())
        {
            OutsideMainEntrance ??= new();
            OutsideMainEntrance.Reset();
            mainEntrance = OutsideMainEntrance;
            mainEntrance.EntranceTeleport = entranceTeleport;
            mainEntrance.DoorViewBlocker = OutsideHelper.GetDoorViewBlocker(entranceTeleport);
            mainEntrance.DoorObjects = OutsideHelper.GetDoorObjects(entranceTeleport);
        }
        else
        {
            InsideMainEntrance ??= new();
            InsideMainEntrance.Reset();
            mainEntrance = InsideMainEntrance;
            mainEntrance.EntranceTeleport = entranceTeleport;
            mainEntrance.DoorViewBlocker = InteriorHelper.GetDoorViewBlocker(entranceTeleport);
            mainEntrance.DoorObjects = InteriorHelper.GetDoorObjects(entranceTeleport);
        }

        if (mainEntrance.HasDoorViewBlocker)
        {
            Logger.LogInfo($"[{nameof(EntranceManager)}] Successfully found main entrance door view blocker! {entranceTeleport.GetLogInfo()}");
        }
        else
        {
            Logger.LogWarning($"[{nameof(EntranceManager)}] Failed to spawn main entrance door portal. Could not find main entrance door view blocker. {entranceTeleport.GetLogInfo()}");
            mainEntrance?.Reset();
            return;
        }

        GameObject gameObject = Object.Instantiate(Assets.DoorPortalPrefab, entranceTeleport.transform);
        gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        Vector3 scale = Assets.DoorPortalPrefab.transform.localScale;
        gameObject.transform.SetLossyScale(scale);

        DoorPortal doorPortal = gameObject.GetComponent<DoorPortal>();

        mainEntrance.DoorPortal = doorPortal;

        _entranceToPortals.Add(entranceTeleport, doorPortal);

        doorPortal.SetMainEntranceData(mainEntrance);

        Logger.LogInfo($"[{nameof(EntranceManager)}] Successfully spawned main entrance door portal! {entranceTeleport.GetLogInfo()}");
    }

    public static void LinkMainEntrancePortals()
    {
        if (_linkedMainEntrancePortals) return;
        _linkedMainEntrancePortals = true;

        Logger.LogInfo($"[{nameof(EntranceManager)}] Attempting to link main entrance portals.");
        Logger.LogInfo($"[{nameof(EntranceManager)}] There are {_entranceToPortals.Count} entrance portals.", extended: true);

        if (OutsideMainEntrance == null)
        {
            Logger.LogError($"[{nameof(EntranceManager)}] Failed to link main entrance portals. {nameof(OutsideMainEntrance)} is null.");
            return;
        }

        if (InsideMainEntrance == null)
        {
            Logger.LogError($"[{nameof(EntranceManager)}] Failed to link main entrance portals. {nameof(InsideMainEntrance)} is null.");
            return;
        }

        bool success = true;

        if (!OutsideMainEntrance.HasDoorPortal)
        {
            Logger.LogError($"[{nameof(EntranceManager)}] Failed to link main entrance portals. Outside door portal was not spawned.");
            success = false;
        }

        if (!InsideMainEntrance.HasDoorPortal)
        {
            Logger.LogError($"[{nameof(EntranceManager)}] Failed to link main entrance portals. Inside door portal was not spawned.");
            success = false;
        }

        if (!success)
        {
            Logger.LogError($"[{nameof(EntranceManager)}] Failed to link main entrance portals.");

            if (OutsideMainEntrance.HasDoorPortal)
            {
                Object.Destroy(OutsideMainEntrance.DoorPortal.gameObject);
                Logger.LogInfo($"[{nameof(EntranceManager)}] Despawned outside door portal.");
            }

            if (InsideMainEntrance.HasDoorPortal)
            {
                Object.Destroy(InsideMainEntrance.DoorPortal.gameObject);
                Logger.LogInfo($"[{nameof(EntranceManager)}] Despawned inside door portal.");
            }

            return;
        }

        OutsideMainEntrance.DoorPortal.LinkPortal(InsideMainEntrance.DoorPortal);
        InsideMainEntrance.DoorPortal.LinkPortal(OutsideMainEntrance.DoorPortal);
    }
}
