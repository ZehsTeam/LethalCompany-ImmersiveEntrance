using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Objects;

[CreateAssetMenu(menuName = "ImmersiveEntrance/EntranceDoorReplacement", order = 0, fileName = "EntranceDoorReplacement")]
public class EntranceDoorReplacement : ScriptableObject
{
    [field: SerializeField]
    public GameObject DoorLeft { get; private set; }

    [field: SerializeField]
    public GameObject DoorRight { get; private set; }
}
