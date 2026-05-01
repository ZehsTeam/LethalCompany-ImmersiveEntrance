using com.github.zehsteam.PeekInside.MonoBehaviours;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.Objects;

public class MainEntranceInfo
{
    public EntranceTeleport EntranceTeleport { get; set; }
    public DoorPortal DoorPortal { get; set; }
    public GameObject ViewBlockerObject { get; set; }

    public bool HasEntranceTeleport => EntranceTeleport != null;
    public bool HasDoorPortal => DoorPortal != null;
    public bool HasViewBlocker => ViewBlockerObject != null;

    public void Reset()
    {
        EntranceTeleport = null;
        DoorPortal = null;
        ViewBlockerObject = null;
    }
}
