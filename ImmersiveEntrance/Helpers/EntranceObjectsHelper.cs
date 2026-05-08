using com.github.zehsteam.ImmersiveEntrance.Extensions;
using com.github.zehsteam.ImmersiveEntrance.Objects;
using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Helpers;

internal static class EntranceObjectsHelper
{
    private static TransformFindOptions _transformFindOptions;

    static EntranceObjectsHelper()
    {
        _transformFindOptions = new TransformFindOptions(onlyEnabled: true, excludeLayers: [LayerMask.NameToLayer("MapRadar"), LayerMask.NameToLayer("ScanNode")]);
    }

    public static EntranceObjects GetEntranceObjects(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return new EntranceObjects();

        Transform visualDoorsContainer = GetVisualDoorsContainer(entranceTeleport);

        if (visualDoorsContainer == null)
        {
            return new EntranceObjects { IsOutside = entranceTeleport.IsOutside() };
        }

        return new EntranceObjects
        {
            IsOutside = entranceTeleport.IsOutside(),
            ViewBlocker = GetViewBlocker(entranceTeleport, visualDoorsContainer),
            DoorFrame = GetDoorFrame(entranceTeleport, visualDoorsContainer),
            DoorLeft = GetDoorLeft(entranceTeleport, visualDoorsContainer),
            DoorRight = GetDoorRight(entranceTeleport, visualDoorsContainer)
        };
    }

    private static GameObject GetViewBlocker(EntranceTeleport entranceTeleport, Transform visualDoorsContainer)
    {
        if (TryGetViewBlockerFromParent(entranceTeleport.transform, out Transform result))
        {
            return result.gameObject;
        }

        if (TryGetViewBlockerFromParent(visualDoorsContainer, out result))
        {
            return result.gameObject;
        }

        // Adamance
        if (entranceTeleport.IsOutside())
        {
            if (TryGetViewBlockerFromParent(entranceTeleport.transform.parent, out result))
            {
                return result.gameObject;
            }
        }

        if (!entranceTeleport.IsOutside())
            return null;

        // Outside

        if (!LevelHelper.TryGetEnvironment(out GameObject environment))
            return null;

        if (TryGetViewBlockerFromParent(environment.transform, out result))
        {
            return result.gameObject;
        }

        // Artifice
        if (environment.transform.TryFind("MainFactory", out Transform parent))
        {
            if (TryGetViewBlockerFromParent(parent, out result))
            {
                return result.gameObject;
            }
        }

        return null;
    }

    private static GameObject GetDoorFrame(EntranceTeleport entranceTeleport, Transform visualDoorsContainer)
    {
        if (TryGetDoorFrameFromParent(entranceTeleport.transform, out Transform result))
        {
            return result.gameObject;
        }

        if (TryGetDoorFrameFromParent(visualDoorsContainer, out result))
        {
            return result.gameObject;
        }

        if (!entranceTeleport.IsOutside())
            return null;

        // Outside

        if (!LevelHelper.TryGetEnvironment(out GameObject environment))
            return null;

        if (TryGetDoorFrameFromParent(environment.transform, out result))
        {
            return result.gameObject;
        }

        // Artifice
        if (environment.transform.TryFind("MainFactory", out Transform parent))
        {
            if (TryGetDoorFrameFromParent(parent, out result))
            {
                return result.gameObject;
            }
        }

        return null;
    }

    private static GameObject GetDoorLeft(EntranceTeleport entranceTeleport, Transform visualDoorsContainer)
    {
        if (TryGetDoorLeftFromParent(visualDoorsContainer, out Transform result))
        {
            return result.gameObject;
        }

        return null;
    }

    private static GameObject GetDoorRight(EntranceTeleport entranceTeleport, Transform visualDoorsContainer)
    {
        if (TryGetDoorRightFromParent(visualDoorsContainer, out Transform result))
        {
            return result.gameObject;
        }

        return null;
    }

    private static bool TryGetViewBlockerFromParent(Transform parent, out Transform result)
    {
        if (parent == null)
        {
            result = null;
            return false;
        }

        return parent.TryFindFirst([
            // Facility outside door
            "Plane",

            // Inside doors
            "LightBehindDoor"
        ], out result, _transformFindOptions);
    }

    private static bool TryGetDoorFrameFromParent(Transform parent, out Transform result)
    {
        if (parent == null)
        {
            result = null;
            return false;
        }

        return parent.TryFindFirst([
            // Facility/Mineshaft outside door
            "DoorFrame (1)",
            
            // Facility/Mineshaft inside door
            "DoorFrame",
            
            // Mansion door
            "WideDoorFrame (1)"
        ], out result, _transformFindOptions);
    }

    private static bool TryGetDoorLeftFromParent(Transform parent, out Transform result)
    {
        if (parent == null)
        {
            result = null;
            return false;
        }

        return parent.TryFindFirst([
            // Facility/Mineshaft door
            "SteelDoorFake",
            
            // Mansion door
            "DoorMesh (1)"
        ], out result, _transformFindOptions);
    }

    private static bool TryGetDoorRightFromParent(Transform parent, out Transform result)
    {
        if (parent == null)
        {
            result = null;
            return false;
        }

        return parent.TryFindFirst([
            // Facility/Mineshaft door
            "SteelDoorFake (1)",
            
            // Mansion door
            "DoorMesh"
        ], out result, _transformFindOptions);
    }

    public static Transform GetVisualDoorsContainer(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport.thisEntranceAnimator != null)
        {
            return entranceTeleport.thisEntranceAnimator.transform;
        }

        if (entranceTeleport.IsOutside())
        {
            if (LevelHelper.TryGetEnvironment(out GameObject environment))
            {
                return environment.transform.Find("OutsideEntranceVisualDoorsContainer")?.transform;
            }

            return null;
        }
        else
        {
            return GameObject.FindWithTag("InsideEntranceDoor")?.transform;
        }
    }
}
