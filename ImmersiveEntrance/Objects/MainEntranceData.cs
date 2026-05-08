using com.github.zehsteam.ImmersiveEntrance.Extensions;
using com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;
using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Objects;

public class MainEntranceData
{
    public EntranceTeleport EntranceTeleport { get; set; }
    public DoorPortal DoorPortal { get; set; }
    public EntranceObjects EntranceObjects { get; set; }

    public bool IsOutside => EntranceTeleport.IsOutside();

    public bool HasEntranceTeleport => EntranceTeleport != null;
    public bool HasDoorPortal => DoorPortal != null;

    public void Reset()
    {
        EntranceTeleport = null;

        if (DoorPortal != null)
            Object.Destroy(DoorPortal);

        EntranceObjects = null;
    }
}
