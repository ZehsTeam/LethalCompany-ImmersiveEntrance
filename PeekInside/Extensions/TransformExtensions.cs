using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.Extensions;

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
}
