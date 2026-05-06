using com.github.zehsteam.PeekInside.Extensions;
using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.MonoBehaviours;
using com.github.zehsteam.PeekInside.Objects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.github.zehsteam.PeekInside.Managers;

internal static class EntranceManager
{
    public static MainEntranceData OutsideMainEntrance { get; private set; } = new();
    public static MainEntranceData InsideMainEntrance { get; private set; } = new();

    private static bool _linkedMainEntrancePortals;

    public static void Reset()
    {
        OutsideMainEntrance.Reset();
        InsideMainEntrance.Reset();

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
            Logger.LogWarning($"[{nameof(EntranceManager)}] Failed to find main entrance door view blocker. {entranceTeleport.GetLogInfo()}");
        }

        Vector3 position = entranceTeleport.transform.position;
        Quaternion rotation = entranceTeleport.transform.rotation;

        GameObject gameObject = Object.Instantiate(Assets.DoorPortalPrefab, position, rotation);
        DoorPortal doorPortal = gameObject.GetComponent<DoorPortal>();

        mainEntrance.DoorPortal = doorPortal;

        doorPortal.SetMainEntranceData(mainEntrance);

        Logger.LogInfo($"[{nameof(EntranceManager)}] Successfully spawned main entrance door portal! {entranceTeleport.GetLogInfo()}");
    }

    public static void LinkMainEntrancePortals()
    {
        if (_linkedMainEntrancePortals) return;
        _linkedMainEntrancePortals = true;

        Logger.LogInfo($"[{nameof(EntranceManager)}] Attempting to link main entrance portals.");

        bool success = true;

        if (!OutsideMainEntrance.HasDoorPortal)
        {
            Logger.LogError($"[{nameof(EntranceManager)}] Failed to link main entrance portal. Outside door portal was not spawned.");
            success = false;
        }

        if (!InsideMainEntrance.HasDoorPortal)
        {
            Logger.LogError($"[{nameof(EntranceManager)}] Failed to link main entrance portal. Inside door portal was not spawned.");
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
