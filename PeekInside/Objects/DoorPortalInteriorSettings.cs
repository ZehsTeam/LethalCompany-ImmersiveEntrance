using System;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.Objects;

[Serializable]
public class DoorPortalInteriorSettings
{
    public InteriorType InteriorType = InteriorType.Unknown;

    [Space(5f)]
    [Header("Pivot")]

    public bool UseDynamicPivot = true;
    public Vector3 PivotPositionOffset;

    [Space(5f)]
    [Header("Screen")]

    [Range(0f, 0.5f)]
    public float ScreenCropLeft;

    [Range(0f, 0.5f)]
    public float ScreenCropRight;

    [Range(0f, 0.5f)]
    public float ScreenCropTop;

    [Range(0f, 0.5f)]
    public float ScreenCropBottom;
}
