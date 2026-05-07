using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Extensions;

internal static class TransformExtensions
{
    public static bool TryFind(this Transform transform, string name, out Transform result)
    {
        if (transform == null)
        {
            result = null;
            return false;
        }

        result = transform.Find(name);
        return result != null;
    }

    public static List<Transform> GetChildren(this Transform transform)
    {
        List<Transform> list = [];

        for (int i = 0; i < transform.childCount; i++)
        {
            list.Add(transform.GetChild(i));
        }

        return list;
    }

    public static void SetLossyScale(this Transform transform, Vector3 scale)
    {
        if (transform == null)
            return;

        if (transform.parent == null)
        {
            transform.localScale = scale;
            return;
        }

        Vector3 parentLossyScale = transform.parent.lossyScale;

        transform.localScale = new Vector3(
            parentLossyScale.x != 0 ? scale.x / parentLossyScale.x : scale.x,
            parentLossyScale.y != 0 ? scale.y / parentLossyScale.y : scale.y,
            parentLossyScale.z != 0 ? scale.z / parentLossyScale.z : scale.z
        );
    }
}
