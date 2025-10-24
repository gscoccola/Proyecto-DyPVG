using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : Singleton<LevelManager>
{
    [ReadOnly] public List<Distraction> DistractionList = new();
    [ReadOnly] public List<Dog> DogList = new();

    private new void Awake()
    {
        base.Awake();
        foreach (GameObject gameObject in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (gameObject.GetComponent<Distraction>() != null) DistractionList.Add(gameObject.GetComponent<Distraction>());
            if (gameObject.GetComponent<Dog>() != null) DogList.Add(gameObject.GetComponent<Dog>());
        }
        DogList = DogList.OrderBy(dog => dog.DogParameters.DogName).ToList();
    }
}
