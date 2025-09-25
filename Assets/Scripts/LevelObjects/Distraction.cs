using UnityEngine;
using UnityEngine.Events;

public class Distraction : MonoBehaviour, IGridCollider
{
    public bool IsOccupied;
    public Dog OccupyingDog;
    public UnityEvent OnDistractionOccupied;

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
