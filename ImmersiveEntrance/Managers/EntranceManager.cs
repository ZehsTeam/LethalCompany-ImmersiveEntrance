using com.github.zehsteam.ImmersiveEntrance.Extensions;
using com.github.zehsteam.ImmersiveEntrance.Helpers;
using com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;
using com.github.zehsteam.ImmersiveEntrance.Objects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.github.zehsteam.ImmersiveEntrance.Managers;

internal static class EntranceManager
{
    public static MainEntranceData OutsideMainEntrance { get; private set; }
    public static MainEntranceData InsideMainEntrance { get; private set; }

    private static bool _linkedMainEntrancePortals;

    public static void Reset()
    {
        OutsideMainEntrance?.Reset();
        InsideMainEntrance?.Reset();

        _linkedMainEntrancePortals = false;
    }

    public static void SpawnDoorPortal(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return;

        if (!entranceTeleport.IsMainEntrance())
            return;

        Logger.LogInfo($"[{nameof(EntranceManager)}] Attempting to spawn main entrance door portal! {entranceTeleport.GetLogInfo()}");
        
        MainEntranceData mainEntrance;

        if (entranceTeleport.IsOutside())
        {
            OutsideMainEntrance ??= new();
            OutsideMainEntrance.Reset();
            mainEntrance = OutsideMainEntrance;
        }
        else
        {
            InsideMainEntrance ??= new();
            InsideMainEntrance.Reset();
            mainEntrance = InsideMainEntrance;
        }

        mainEntrance.EntranceTeleport = entranceTeleport;
        mainEntrance.EntranceObjects = EntranceObjectsHelper.GetEntranceObjects(entranceTeleport);

        if (!mainEntrance.EntranceObjects.IsValid())
        {
            Logger.LogWarning($"[{nameof(EntranceManager)}] Failed to spawn main entrance door portal. Could not find all entrance objects. {entranceTeleport.GetLogInfo()}");
            mainEntrance.EntranceObjects.LogMissingObjects();
            mainEntrance?.Reset();
            return;
        }

        GameObject gameObject = Object.Instantiate(Assets.DoorPortalPrefab, entranceTeleport.transform);
        gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        Vector3 scale = Assets.DoorPortalPrefab.transform.localScale;
        gameObject.transform.SetLossyScale(scale);

        DoorPortal doorPortal = gameObject.GetComponent<DoorPortal>();

        mainEntrance.DoorPortal = doorPortal;

        doorPortal.SetMainEntranceData(mainEntrance);

        Logger.LogInfo($"[{nameof(EntranceManager)}] Successfully spawned main entrance door portal! {entranceTeleport.GetLogInfo()}");

        EntranceDoorReplacementManager.ReplaceDoor(mainEntrance);
    }

    public static void LinkMainEntrancePortals()
    {
        if (_linkedMainEntrancePortals) return;
        _linkedMainEntrancePortals = true;

        Logger.LogInfo($"[{nameof(EntranceManager)}] Attempting to link main entrance portals.");

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

        Logger.LogInfo($"[{nameof(EntranceManager)}] Successfully linked main entrance portals!");
    }
}
