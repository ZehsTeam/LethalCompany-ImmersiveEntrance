using UnityEngine.Rendering.HighDefinition;

namespace com.github.zehsteam.PeekInside.Helpers;

internal static class FacilityHelper
{
    public static void RenderFacility()
    {
        if (StartOfRound.Instance == null)
            return;

        AdjacentRoomCullingModified occlusionCuller = StartOfRound.Instance.occlusionCuller;
        if (occlusionCuller == null) return;

        if (!occlusionCuller.enabled)
            return;

        occlusionCuller.SetToStartTile();
    }

    public static void SetFogEnabled(bool value)
    {
        if (RoundManager.Instance == null)
            return;

        if (PlayerUtils.LocalPlayerScript == null)
            return;

        if (PlayerUtils.LocalPlayerScript.isInsideFactory)
            return;

        LocalVolumetricFog indoorFog = RoundManager.Instance.indoorFog;

        if (indoorFog == null)
            return;

        indoorFog.enabled = value;
    }
}
