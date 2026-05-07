using com.github.zehsteam.ImmersiveEntrance.Extensions;
using com.github.zehsteam.ImmersiveEntrance.Helpers;
using com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;
using com.github.zehsteam.ImmersiveEntrance.Objects;
using com.github.zehsteam.ImmersiveEntrance.Objects.PortalSettingTypes;
using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Managers;

internal static class EntranceDoorReplacementManager
{
    public static void ReplaceDoor(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return;

        if (!entranceTeleport.IsMainEntrance())
            return;

        if (entranceTeleport.IsOutside())
            return;

        InteriorPortalSettings interiorSettings = PortalSettingsManager.GetCurrentInteriorSettings();

        if (interiorSettings.HasDoorReplacement && interiorSettings.ReplaceDoor.Value)
        {
            ReplaceDoor(entranceTeleport, interiorSettings.GetDoorReplacement());
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
