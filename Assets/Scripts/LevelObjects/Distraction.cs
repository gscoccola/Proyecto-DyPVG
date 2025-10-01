using UnityEngine;
using UnityEngine.Events;

public class Distraction : MonoBehaviour, IGridCollider
{
    [Header("Debug")]
    [ReadOnly] public bool IsOccupied;
    [ReadOnly] public Dog OccupyingDog;
    [HideInInspector] public UnityEvent OnDistractionOccupied;

    public void OnGridCollisionEnter(Transform other)
    {
        if (other.GetComponent<Dog>() == null) return;
        IsOccupied = true;
        OccupyingDog = other.GetComponent<Dog>();
        OnDistractionOccupied?.Invoke();
    }

    public void OnGridCollisionExit(Transform other)
    {
        if (other.GetComponent<Dog>() == null) return;
        IsOccupied = false;
        OccupyingDog = null;
    }
}
