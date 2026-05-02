using com.github.zehsteam.PeekInside.Extensions;
using com.github.zehsteam.PeekInside.MonoBehaviours;
using com.github.zehsteam.PeekInside.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.github.zehsteam.PeekInside.Managers;

internal static class EntranceManager
{
    public static MainEntranceData OutsideMainEntrance { get; private set; } = new();
    public static MainEntranceData InsideMainEntrance { get; private set; } = new();

    private static bool _linkedMainEntrancePortals;

    public static void SpawnDoorPortal(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return;

        if (!entranceTeleport.IsMainEntrance())
            return;

        Logger.LogInfo($"[{nameof(EntranceManager)}] Attempting to spawn main entrance door portal! {entranceTeleport.GetLogInfo()}");

        MainEntranceData mainEntrance;

        if (entranceTeleport.isEntranceToBuilding)
        {
            OutsideMainEntrance ??= new();
            OutsideMainEntrance.Reset();
            mainEntrance = OutsideMainEntrance;
            mainEntrance.EntranceTeleport = entranceTeleport;
            AssignOutsideMainEntranceObjects();
        }
        else
        {
            InsideMainEntrance ??= new();
            InsideMainEntrance.Reset();
            mainEntrance = InsideMainEntrance;
            mainEntrance.EntranceTeleport = entranceTeleport;
            AssignInsideMainEntranceObjects();
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

        doorPortal.SetMainEntranceData(mainEntrance);

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

    public static void OnEndOfGame()
    {
        OutsideMainEntrance.Reset();
        InsideMainEntrance.Reset();

        _linkedMainEntrancePortals = false;
    }

    private static void AssignOutsideMainEntranceObjects()
    {
        if (OutsideMainEntrance == null)
            return;

        EntranceTeleport entranceTeleport = OutsideMainEntrance.EntranceTeleport;

        if (entranceTeleport == null)
            return;

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
            return;

        Transform viewBlockerTransform = parentTransform.Find("Plane");

        OutsideMainEntrance.ViewBlockerObject = viewBlockerTransform?.gameObject;

        AssignDoorObjects(OutsideMainEntrance, parentTransform);
    }

    private static void AssignInsideMainEntranceObjects()
    {
        if (InsideMainEntrance == null)
            return;

        if (RoundManager.Instance == null)
            return;

        if (RoundManager.Instance.dungeonGenerator == null)
            return;

        try
        {
            GameObject levelGenerationRoot = RoundManager.Instance.dungeonGenerator.Root;
            
            Transform parentTransform = levelGenerationRoot.transform.Find("StartRoom(Clone)").Find("FactoryEntranceTeleVisualDoorsContainer");
            Transform viewBlockerTransform = parentTransform.Find("LightBehindDoor");

            InsideMainEntrance.ViewBlockerObject = viewBlockerTransform?.gameObject;

            AssignDoorObjects(InsideMainEntrance, parentTransform);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[{nameof(EntranceManager)}] Failed to get inside main entrance view blocker. {ex}");
        }
    }

    private static void AssignDoorObjects(MainEntranceData mainEntrance, Transform parent)
    {
        string[] doorObjectNames = ["SteelDoorFake", "SteelDoorFake (1)", "DoorFrame"];

        List<GameObject> doorObjects = [];

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (doorObjectNames.Contains(child.name))
            {
                doorObjects.Add(child.gameObject);
            }
        }

        mainEntrance.DoorObjects = doorObjects;
    }
}
