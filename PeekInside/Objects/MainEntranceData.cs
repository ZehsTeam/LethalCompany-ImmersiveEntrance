using com.github.zehsteam.PeekInside.Extensions;
using com.github.zehsteam.PeekInside.MonoBehaviours;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.Objects;

public class MainEntranceData
{
    public EntranceTeleport EntranceTeleport { get; set; }
    public DoorPortal DoorPortal { get; set; }
    public GameObject DoorViewBlocker { get; set; }
    public List<GameObject> DoorObjects { get; set; } = [];

    public bool IsOutside => EntranceTeleport.IsOutside();

    public bool HasEntranceTeleport => EntranceTeleport != null;
    public bool HasDoorPortal => DoorPortal != null;
    public bool HasDoorViewBlocker => DoorViewBlocker != null;
    public bool HasDoorObjects => DoorObjects != null && DoorObjects.Count > 0;

    public void Reset()
    {
        EntranceTeleport = null;
        DoorPortal = null;
        DoorViewBlocker = null;
    }

    public void SetDoorObjectsEnabled(bool value)
    {
        if (DoorObjects == null || DoorObjects.Count == 0)
            return;

        foreach (var gameObject in DoorObjects)
        {
            if (gameObject == null)
                continue;

            gameObject.SetActive(value);
        }
    }
}
