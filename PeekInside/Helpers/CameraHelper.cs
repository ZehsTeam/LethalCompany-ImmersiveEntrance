using System.Drawing;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.Helpers;

internal static class CameraHelper
{
    public static bool IsVisibleFromCamera(Renderer renderer, Camera camera)
    {
        if (renderer == null || camera == null)
            return false;

        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
    }

    public static Size GetCameraScreenSize()
    {
        IngamePlayerSettings playerSettings = IngamePlayerSettings.Instance;

        if (playerSettings != null && playerSettings.playerGameplayScreenTex != null)
        {
            return new Size(playerSettings.playerGameplayScreenTex.width, playerSettings.playerGameplayScreenTex.height);
        }

        return new Size(860, 520);
    }
}
