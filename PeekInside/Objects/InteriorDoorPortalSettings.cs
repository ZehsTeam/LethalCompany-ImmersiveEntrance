using System;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.Objects;

[Serializable]
public class InteriorDoorPortalSettings
{
    public InteriorType InteriorType = InteriorType.Unknown;
    public bool UseDynamicPivot = true;
    public Vector3 PivotPositionOffset;
    public float ScreenWidthOffset;
    public float ScreenHeightOffset;
}
