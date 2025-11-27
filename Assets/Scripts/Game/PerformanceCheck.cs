using UnityEngine;

public class PerformanceCheck : MonoBehaviour
{
    public int GameObjectsCount;
    public int TotalObjectsCount;

    public int maxGameObjects;
    public int maxTotalObjects;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        GameObjectsCount = FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
        TotalObjectsCount = GameObjectsCount + FindObjectsByType<Component>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;

        if (GameObjectsCount > maxGameObjects)
        {
            maxGameObjects = GameObjectsCount;
        }

        if (TotalObjectsCount > maxTotalObjects)
        {
            maxTotalObjects = TotalObjectsCount;
        }
    }

    [ContextMenu("PrintActiveGO")]
    private void PrintActiveObjects()
    {
        var gameObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var go in gameObjects)
        {
            Debug.Log(go.name);
        }
    }
}
