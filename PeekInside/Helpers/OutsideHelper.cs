using com.github.zehsteam.PeekInside.Extensions;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace com.github.zehsteam.PeekInside.Helpers;

internal static class OutsideHelper
{
    public static bool IsOverridingSun { get; private set; }

    public static void Reset()
    {
        IsOverridingSun = false;
    }

    public static void SetSunEnabled(bool value)
    {
        if (!value)
        {
            IsOverridingSun = value;
        }

        TimeOfDay timeOfDay = TimeOfDay.Instance;

        if (timeOfDay == null)
            return;

        if (!PlayerUtils.TryGetLocalPlayerScript(out PlayerControllerB playerScript))
            return;

        if (!playerScript.isInsideFactory)
            return;

        IsOverridingSun = value;

        timeOfDay.sunDirect.enabled = value;
        timeOfDay.sunIndirect.enabled = value;

        //HDAdditionalLightData additionalLightData = timeOfDay.indirectLightData;
        //additionalLightData.lightDimmer = Mathf.Lerp(additionalLightData.lightDimmer, 1f, 5f * Time.deltaTime);
    }



    public static GameObject GetDoorViewBlocker(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return null;

        if (TryGetVisualDoorsContainer(entranceTeleport, out Transform container))
        {
            if (container.TryFind("Plane", out Transform viewBlocker))
            {
                return viewBlocker.gameObject;
            }
        }

        if (GameObjectHelper.TryFind("Environment/Plane", out GameObject result))
        {
            return result;
        }

        return null;
    }

    public static List<GameObject> GetDoorObjects(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return [];

        if (!TryGetVisualDoorsContainer(entranceTeleport, out Transform container))
            return [];

        string[] names = ["SteelDoorFake", "SteelDoorFake (1)", "DoorFrame"];

        List<GameObject> doorObjects = [];

        foreach (var child in container.GetChildren())
        {
            if (names.Contains(child.name, StringComparer.OrdinalIgnoreCase))
            {
                doorObjects.Add(child.gameObject);
            }
        }

        if (GameObjectHelper.TryFind("Environment/DoorFrame (1)", out GameObject result))
        {
            doorObjects.Add(result);
        }

        return doorObjects;
    }

    private static Transform GetVisualDoorsContainer(EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport.thisEntranceAnimator != null)
        {
            return entranceTeleport.thisEntranceAnimator.transform;
        }

        try
        {
            return GameObject.Find("Environment/OutsideEntranceVisualDoorsContainer").transform;
        }
        catch { }

        return null;
    }

    private static bool TryGetVisualDoorsContainer(EntranceTeleport entranceTeleport, out Transform transform)
    {
        transform = GetVisualDoorsContainer(entranceTeleport);
        return transform != null;
    }
}
