using UnityEngine;

namespace com.github.zehsteam.PeekInside.Objects;

[CreateAssetMenu(menuName = "PeekInside/EntranceDoorReplacement", order = 0, fileName = "EntranceDoorReplacement")]
public class EntranceDoorReplacement : ScriptableObject
{
    [field: SerializeField]
    public GameObject DoorLeft { get; private set; }

    [field: SerializeField]
    public GameObject DoorRight { get; private set; }
}
