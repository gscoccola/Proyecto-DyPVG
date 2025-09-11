using UnityEngine;

public class PersistentSingleton<T> : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = GetComponent<T>();
            DontDestroyOnLoad(gameObject);
        }
    }
}

