using com.github.zehsteam.PeekInside.Extensions;
using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.MonoBehaviours;
using com.github.zehsteam.PeekInside.Objects;
using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.Managers;

internal static class EntranceDoorReplacementManager
{
    public static bool IsMansionReplaceInteriorMainEntranceDoorsEnabled => ConfigManager.Mansion_ReplaceInteriorMainEntranceDoors.Value;

    public static void ReplaceDoor(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return;

        if (!entranceTeleport.IsMainEntrance())
            return;

        if (entranceTeleport.IsOutside())
            return;

        InteriorType currentInteriorType = InteriorHelper.GetCurrentInteriorType();

        if (currentInteriorType == InteriorType.Mansion && IsMansionReplaceInteriorMainEntranceDoorsEnabled)
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

        HideAfterDelay(parent.gameObject);
    }

    private static void HideAfterDelay(GameObject gameObject, float delay = 0.1f)
    {
        IEnumerator Coroutine()
        {
            yield return new WaitForSeconds(delay);

            if (gameObject.TryGetComponent(out MeshFilter meshFilter))
            {
                meshFilter.mesh = null;
            }

            if (gameObject.TryGetComponent(out Renderer renderer))
            {
                renderer.enabled = false;
            }
        }

        CoroutineRunner.Start(Coroutine());
    }
}
