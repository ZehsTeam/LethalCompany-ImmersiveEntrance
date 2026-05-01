using com.github.zehsteam.PeekInside.Extensions;
using com.github.zehsteam.PeekInside.MonoBehaviours;
using com.github.zehsteam.PeekInside.Objects;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.github.zehsteam.PeekInside.Managers;

internal static class EntranceManager
{
    public static MainEntranceInfo OutsideMainEntrance { get; private set; } = new();
    public static MainEntranceInfo InsideMainEntrance { get; private set; } = new();

    private static bool _linkedMainEntrancePortals;

    public static void SpawnDoorPortal(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return;

        if (!entranceTeleport.IsMainEntrance())
            return;

        Logger.LogInfo($"[{nameof(EntranceManager)}] Attempting to spawn main entrance door portal! {entranceTeleport.GetLogInfo()}");

        MainEntranceInfo mainEntrance;

        if (entranceTeleport.isEntranceToBuilding)
        {
            OutsideMainEntrance ??= new();
            OutsideMainEntrance.Reset();
            mainEntrance = OutsideMainEntrance;
            mainEntrance.EntranceTeleport = entranceTeleport;
            mainEntrance.ViewBlockerObject = GetOutsideMainEntranceViewBlocker();
        }
        else
        {
            InsideMainEntrance ??= new();
            InsideMainEntrance.Reset();
            mainEntrance = InsideMainEntrance;
            mainEntrance.EntranceTeleport = entranceTeleport;
            mainEntrance.ViewBlockerObject = GetInsideMainEntranceViewBlocker();
        }

        if (mainEntrance.HasViewBlocker)
        {
            Logger.LogInfo($"[{nameof(EntranceManager)}] Successfully found main entrance view blocker! {entranceTeleport.GetLogInfo()}");
        }
        else
        {
            Logger.LogWarning($"[{nameof(EntranceManager)}] Failed to find main entrance view blocker. {entranceTeleport.GetLogInfo()}");
        }

        Vector3 position = entranceTeleport.transform.position;
        Quaternion rotation = entranceTeleport.transform.rotation;

        GameObject gameObject = Object.Instantiate(Assets.DoorPortalPrefab, position, rotation);
        DoorPortal doorPortal = gameObject.GetComponent<DoorPortal>();

        mainEntrance.DoorPortal = doorPortal;

        doorPortal.SetMainEntranceInfo(mainEntrance);

        Logger.LogInfo($"[{nameof(EntranceManager)}] Successfully spawned main entrance door portal! {entranceTeleport.GetLogInfo()}");
    }

    public static void LinkMainEntrancePortals()
    {
        Logger.LogInfo($"[{nameof(EntranceManager)}] Attempting to link main entrance portals.");

        if (_linkedMainEntrancePortals)
        {
            Logger.LogWarning($"[{nameof(EntranceManager)}] Main entrance portals are already linked.");
            return;
        }

        _linkedMainEntrancePortals = true;

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

    public static void OnShipHasLeft()
    {
        OutsideMainEntrance.Reset();
        InsideMainEntrance.Reset();

        _linkedMainEntrancePortals = false;
    }

    private static GameObject GetOutsideMainEntranceViewBlocker()
    {
        if (OutsideMainEntrance == null)
            return null;

        EntranceTeleport entranceTeleport = OutsideMainEntrance.EntranceTeleport;

        if (entranceTeleport == null)
            return null;

        Transform parentTransform;

        if (entranceTeleport.thisEntranceAnimator != null)
        {
            parentTransform = entranceTeleport.thisEntranceAnimator.transform;
        }
        else
        {
            parentTransform = GameObject.Find("Environment/OutsideEntranceVisualDoorsContainer")?.transform ?? null;
        }

        if (parentTransform == null)
            return null;

        Transform targetTransform = parentTransform.Find("Plane");
        if (targetTransform == null) return null;

        return targetTransform.gameObject;
    }

    private static GameObject GetInsideMainEntranceViewBlocker()
    {
        if (InsideMainEntrance == null)
            return null;

        if (RoundManager.Instance == null)
            return null;

        if (RoundManager.Instance.dungeonGenerator == null)
            return null;

        try
        {
            GameObject levelGenerationRoot = RoundManager.Instance.dungeonGenerator.Root;
            
            Transform parentTransform = levelGenerationRoot.transform.Find("StartRoom(Clone)").Find("FactoryEntranceTeleVisualDoorsContainer");
            Transform targetTransform = parentTransform.Find("LightBehindDoor");

            return targetTransform.gameObject;
        }
        catch (Exception ex)
        {
            Logger.LogError($"[{nameof(EntranceManager)}] Failed to get inside main entrance view blocker. {ex}");
            return null;
        }
    }
}
