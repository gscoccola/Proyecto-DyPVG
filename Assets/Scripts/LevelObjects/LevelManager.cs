using UnityEngine;
using System.Collections.Generic;

public class LevelManager : Singleton<LevelManager>
{
    [ReadOnly] public List<Distraction> DistractionList = new();

    private new void Awake()
    {
        base.Awake();
        foreach (GameObject gameObject in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (gameObject.GetComponent<Distraction>() != null) DistractionList.Add(gameObject.GetComponent<Distraction>());
        }
    }
}
