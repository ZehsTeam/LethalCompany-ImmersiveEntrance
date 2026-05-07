using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.Helpers;

internal static class GameObjectHelper
{
    public static bool TryFind(string name, out GameObject gameObject)
    {
        gameObject = GameObject.Find(name);
        return gameObject != null;
    }
}
