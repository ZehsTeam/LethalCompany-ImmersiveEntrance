using System.Text;
using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Objects;

public class EntranceObjects
{
    public bool IsOutside;

    public GameObject ViewBlocker;
    public GameObject DoorFrame;
    public GameObject DoorLeft;
    public GameObject DoorRight;

    public bool IsValid()
    {
        if (ViewBlocker == null)
            return false;

        if (!IsOutside && DoorFrame == null)
            return false;

        if (DoorLeft == null)
            return false;

        if (DoorRight == null)
            return false;

        return true;
    }

    public void SetViewBlockerEnabled(bool value)
    {
        ViewBlocker?.SetActive(value);
    }

    public void SetObjectsEnabled(bool value)
    {
        //ViewBlocker?.SetActive(value);
        DoorFrame?.SetActive(value);
        DoorLeft?.SetActive(value);
        DoorRight?.SetActive(value);
    }

    public void LogMissingObjects()
    {
        var builder = new StringBuilder();

        builder.AppendLine($"[{nameof(EntranceObjects)}] (IsOutside: {IsOutside}) is missing these objects:");

        if (ViewBlocker == null) builder.AppendLine($"- {nameof(ViewBlocker)}");
        if (DoorFrame == null) builder.AppendLine($"- {nameof(DoorFrame)}");
        if (DoorLeft == null) builder.AppendLine($"- {nameof(DoorLeft)}");
        if (DoorRight == null) builder.AppendLine($"- {nameof(DoorRight)}");

        Logger.LogWarning(builder.ToString());
    }
}
