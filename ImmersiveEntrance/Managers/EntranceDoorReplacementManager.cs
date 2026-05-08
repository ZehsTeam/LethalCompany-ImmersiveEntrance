using com.github.zehsteam.ImmersiveEntrance.Extensions;
using com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;
using com.github.zehsteam.ImmersiveEntrance.Objects;
using com.github.zehsteam.ImmersiveEntrance.Objects.PortalSettingTypes;
using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Managers;

internal static class EntranceDoorReplacementManager
{
    public static void ReplaceDoor(MainEntranceData mainEntrance)
    {
        if (mainEntrance == null)
            return;

        if (mainEntrance.IsOutside)
            return;

        InteriorPortalSettings interiorSettings = PortalSettingsManager.GetCurrentInteriorSettings();

        if (interiorSettings.HasDoorReplacement && interiorSettings.ReplaceDoor.Value)
        {
            ReplaceDoor(mainEntrance, interiorSettings.GetDoorReplacement());
        }
    }

    private static void ReplaceDoor(MainEntranceData mainEntrance, EntranceDoorReplacement replacement)
    {
        if (mainEntrance == null || replacement == null)
            return;

        Transform originalDoorLeft = mainEntrance.EntranceObjects.DoorLeft?.transform;
        Transform originalDoorRight = mainEntrance.EntranceObjects.DoorRight?.transform;

        if (originalDoorLeft == null || originalDoorRight == null)
        {
            Logger.LogError($"[{nameof(EntranceDoorReplacementManager)}] Failed to replace main entrance door {mainEntrance.EntranceTeleport.GetLogInfo()} with {replacement.name}. Could not find all of the original door objects.");
            return;
        }

        SpawnReplacementDoor(originalDoorLeft, replacement.DoorLeft);
        SpawnReplacementDoor(originalDoorRight, replacement.DoorRight);

        Logger.LogInfo($"[{nameof(EntranceDoorReplacementManager)}] Replaced main entrance door {mainEntrance.EntranceTeleport.GetLogInfo()} with {replacement.name}", extended: true);
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
