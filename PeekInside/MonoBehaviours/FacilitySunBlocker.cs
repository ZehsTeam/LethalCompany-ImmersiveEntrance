using UnityEngine;

namespace com.github.zehsteam.PeekInside.MonoBehaviours;

// TODO: Rename this to InteriorSunBlocker
// NOTE: THIS MIGHT NOT BE NEEDED
public class FacilitySunBlocker : MonoBehaviour
{
    public static FacilitySunBlocker Instance { get; private set; }

    public static void Spawn()
    {
        if (Instance != null)
            return;

        GameObject parentObject = GameObject.Find("Environment");
        Transform parentTransform = parentObject?.transform ?? null;

        Instantiate(Assets.FacilitySunBlockerPrefab, parentTransform);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transform.SetPositionAndRotation(new Vector3(0f, -75f, 0f), Quaternion.identity);
    }
}
