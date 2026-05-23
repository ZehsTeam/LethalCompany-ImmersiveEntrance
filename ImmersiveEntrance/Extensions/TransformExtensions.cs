using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Extensions;

internal static class TransformExtensions
{
    public static bool TryFind(this Transform transform, string name, out Transform result)
    {
        result = transform.Find(name);
        return result != null;
    }

    public static bool TryFind(this Transform transform, string name, out Transform result, TransformFindOptions options)
    {
        result = transform.Find(name);
        return options.SatisfiesConditions(result);
    }

    public static bool TryFindFirst(this Transform transform, IEnumerable<string> names, out Transform result)
    {
        return TryFindFirst(transform, names, out result, new TransformFindOptions());
    }

    public static bool TryFindFirst(this Transform transform, IEnumerable<string> names, out Transform result, TransformFindOptions options)
    {
        result = transform.Find(names, options).FirstOrDefault();
        return result != null;
    }

    public static List<Transform> Find(this Transform transform, IEnumerable<string> names)
    {
        return Find(transform, names, new TransformFindOptions());
    }

    public static List<Transform> Find(this Transform transform, IEnumerable<string> names, TransformFindOptions options)
    {
        List<Transform> result = [];

        foreach (var child in transform.GetChildren(options))
        {
            if (names.Contains(child.gameObject.name, StringComparer.OrdinalIgnoreCase))
            {
                result.Add(child);
            } 
        }

        return result;
    }

    public static List<Transform> GetChildren(this Transform transform)
    {
        return GetChildren(transform, new TransformFindOptions());
    }

    public static List<Transform> GetChildren(this Transform transform, TransformFindOptions options)
    {
        List<Transform> result = [];

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (options.SatisfiesConditions(child))
            {
                result.Add(child);
            }
        }

        return result;
    }

    public static void SetLossyScale(this Transform transform, Vector3 scale)
    {
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

    public static string GetHierarchyPath(this Transform transform)
    {
        if (transform == null)
            return $"Transform is null";

        string path = transform.name;

        Transform currentTransform = transform;

        while (currentTransform.parent != null)
        {
            currentTransform = currentTransform.parent;
            path = $"{currentTransform.name}/{path}";
        }

        return path;
    }
}

internal struct TransformFindOptions
{
    public bool OnlyEnabled { get; set; }
    public int[] IncludeLayers { get; set; }
    public int[] ExcludeLayers { get; set; }

    public TransformFindOptions()
    {
        IncludeLayers = [];
        ExcludeLayers = [];
    }

    public TransformFindOptions(bool onlyEnabled, int[] includeLayers = null, int[] excludeLayers = null)
    {
        OnlyEnabled = onlyEnabled;
        IncludeLayers = includeLayers ?? [];
        ExcludeLayers = excludeLayers ?? [];
    }

    public readonly bool SatisfiesConditions(Transform transform)
    {
        if (transform == null)
            return false;

        if (OnlyEnabled && !transform.gameObject.activeSelf)
            return false;

        int objectLayer = transform.gameObject.layer;

        if (IncludeLayers.Length > 0 && !IncludeLayers.Contains(objectLayer))
            return false;

        if (ExcludeLayers.Contains(objectLayer))
            return false;

        return true;
    }
}
