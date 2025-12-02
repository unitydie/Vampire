using UnityEngine;

public class CameraTargetSingleton : MonoBehaviour
{
    public static CameraTargetSingleton Instance;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Multiple CameraTargetSingletons detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
