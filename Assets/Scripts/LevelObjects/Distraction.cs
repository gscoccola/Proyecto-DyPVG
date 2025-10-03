using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Distraction : MonoBehaviour, IGridCollider, IRevertable
{
    [Header("Debug")]
    [ReadOnly] public bool IsOccupied;
    [ReadOnly] public Dog OccupyingDog;
    [HideInInspector] public UnityEvent OnDistractionOccupied;
    [HideInInspector] public List<bool> OccupiedHistory = new();

    private void Start()
    {
        OccupiedHistory.Add(false);
    }

    #region GRID COLLIDER INTERFACE

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
    #endregion

    #region REVERTABLE INTERFACE

    public void RevertToHistoryPoint(int turnIndex)
    {
        IsOccupied = OccupiedHistory[turnIndex];
    }

    public void SaveHistoryPoint(int turnIndex, bool deleteFuturePoints = true)
    {
        if (deleteFuturePoints)
        {
            while (OccupiedHistory.Count > turnIndex)
                OccupiedHistory.RemoveAt(OccupiedHistory.Count - 1);
        }
        if (OccupiedHistory.Count == turnIndex)
            OccupiedHistory.Add(IsOccupied);
        else
            OccupiedHistory[turnIndex] = IsOccupied;
    }

    #endregion
}
