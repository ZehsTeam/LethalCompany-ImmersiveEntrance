using UnityEngine;

namespace com.github.zehsteam.PeekInside.MonoBehaviours;

public class FacilitySunBlocker : MonoBehaviour
{
    public static FacilitySunBlocker Instance { get; private set; }

    public static void Spawn()
    {
        if (Instance != null)
            return;

        GameObject parentObject = GameObject.Find("Environment");
        Transform parentTransform = parentObject?.transform ?? null;

        Object.Instantiate(Assets.FacilitySunBlockerPrefab, parentTransform);
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
