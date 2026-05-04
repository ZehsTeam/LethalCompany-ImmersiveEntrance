using System;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.Objects;

[Serializable]
public class DoorPortalMoonSettings
{
    public string PlanetName;

    [Space(5f)]
    public bool UseViewRange;
    public float ViewRange;
}
