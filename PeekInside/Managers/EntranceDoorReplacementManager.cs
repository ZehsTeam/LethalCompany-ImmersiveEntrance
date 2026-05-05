using com.github.zehsteam.PeekInside.Extensions;
using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.Objects;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.Managers;

internal static class EntranceDoorReplacementManager
{
    public static bool IsMansionEntranceDoorReplacementEnabled => true;

    public static void ReplaceDoor(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return;

        if (!entranceTeleport.IsMainEntrance())
            return;

        if (entranceTeleport.IsOutside())
            return;

        InteriorType currentInteriorType = InteriorHelper.GetCurrentInteriorType();

        if (currentInteriorType == InteriorType.Mansion && IsMansionEntranceDoorReplacementEnabled)
        {
            ReplaceDoor(entranceTeleport, Assets.MansionEntranceDoorReplacement);
        }
    }

    private static void ReplaceDoor(EntranceTeleport entranceTeleport, EntranceDoorReplacement replacement)
    {
        if (entranceTeleport == null || replacement == null)
            return;

        if (!InteriorHelper.TryGetVisualDoorsContainer(entranceTeleport, out Transform visualDoorsContainer))
        {
            Logger.LogError($"[{nameof(EntranceDoorReplacementManager)}] Failed to replace main entrance door {entranceTeleport.GetLogInfo()} with {replacement.name}. Could not find visual doors container.");
            return;
        }

        Transform originalDoorLeft = visualDoorsContainer.Find("DoorMesh (1)");
        Transform originalDoorRight = visualDoorsContainer.Find("DoorMesh");

        if (originalDoorLeft == null || originalDoorRight == null)
        {
            Logger.LogError($"[{nameof(EntranceDoorReplacementManager)}] Failed to replace main entrance door {entranceTeleport.GetLogInfo()} with {replacement.name}. Could not find all of the original door objects.");
            return;
        }

        SpawnReplacementDoor(originalDoorLeft, replacement.DoorLeft);
        SpawnReplacementDoor(originalDoorRight, replacement.DoorRight);

        Logger.LogInfo($"[{nameof(EntranceDoorReplacementManager)}] Replaced main entrance door {entranceTeleport.GetLogInfo()} with {replacement.name}", extended: true);
    }

    private static void SpawnReplacementDoor(Transform parent, GameObject prefab)
    {
        GameObject gameObject = Object.Instantiate(prefab, parent);
        gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        if (parent.TryGetComponent(out MeshRenderer meshRenderer))
        {
            meshRenderer.enabled = false;
        }
    }
}
