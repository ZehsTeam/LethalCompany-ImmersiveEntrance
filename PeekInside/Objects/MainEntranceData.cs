using com.github.zehsteam.PeekInside.MonoBehaviours;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.Objects;

public class MainEntranceData
{
    public EntranceTeleport EntranceTeleport { get; set; }
    public DoorPortal DoorPortal { get; set; }
    public GameObject ViewBlockerObject { get; set; }
    public List<GameObject> DoorObjects { get; set; } = [];

    public bool HasEntranceTeleport => EntranceTeleport != null;
    public bool HasDoorPortal => DoorPortal != null;
    public bool HasViewBlocker => ViewBlockerObject != null;

    public void Reset()
    {
        EntranceTeleport = null;
        DoorPortal = null;
        ViewBlockerObject = null;
    }

    public void SetDoorObjectsEnabled(bool value)
    {
        if (DoorObjects == null || DoorObjects.Count == 0)
            return;

        foreach (var item in DoorObjects)
        {
            item.SetActive(value);
        }
    }
}
