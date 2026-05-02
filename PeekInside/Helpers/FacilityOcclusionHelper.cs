namespace com.github.zehsteam.PeekInside.Helpers;

internal static class FacilityOcclusionHelper
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
}
