namespace com.github.zehsteam.ImmersiveEntrance.Extensions;

internal static class EntranceTeleportExtensions
{
    public static bool IsMainEntrance(this EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return false;

        return entranceTeleport.entranceId == 0;
    }

    public static bool IsOutside(this EntranceTeleport entranceTeleport)
    {
        return entranceTeleport.isEntranceToBuilding;
    }

    public static string GetLogInfo(this EntranceTeleport entranceTeleport)
    {
        if (entranceTeleport == null)
            return "(EntranceTeleport is null)";

        return $"(entranceId: {entranceTeleport.entranceId}, isEntranceToBuilding: {entranceTeleport.isEntranceToBuilding})";
    }
}
