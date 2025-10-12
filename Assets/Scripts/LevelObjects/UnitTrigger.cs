using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class UnitTrigger : MonoBehaviour, IGridCollider, IRevertable
{
    [Header("Parameters")]
    //[SerializeField] private DogType _requiredType;
    [SerializeField] private bool _mustBeHeld;

    [Header("Debug")]
    [ReadOnly] public bool IsTriggered;
    [HideInInspector] public UnityEvent Triggered;
    [HideInInspector] public UnityEvent Untriggered;
    [HideInInspector] public List<bool> TriggeredHistory = new();

    private void Start()
    {
        TriggeredHistory.Add(false);
    }

    #region GRID COLLIDER INTERFACE

    public void OnGridCollisionEnter(Transform other)
    {
        if (other.GetComponent<Dog>() == null) return;
        ToggleTriggered(true);
        //Triggered?.Invoke();
    }

    public void OnGridCollisionExit(Transform other)
    {
        if (other.GetComponent<Dog>() == null) return;
        if (!_mustBeHeld) return;
        ToggleTriggered(false);
    }
    #endregion

    #region REVERTABLE INTERFACE

    public void RevertToHistoryPoint(int turnIndex)
    {
        ToggleTriggered(TriggeredHistory[turnIndex]);
    }

    public void SaveHistoryPoint(int turnIndex, bool deleteFuturePoints = true)
    {
        if (deleteFuturePoints)
        {
            while (TriggeredHistory.Count > turnIndex)
                TriggeredHistory.RemoveAt(TriggeredHistory.Count - 1);
        }
        if (TriggeredHistory.Count == turnIndex)
            TriggeredHistory.Add(IsTriggered);
        else
            TriggeredHistory[turnIndex] = IsTriggered;
    }
    #endregion

    public void ToggleTriggered(bool triggered)
    {
        IsTriggered = triggered;
        if (triggered) Triggered?.Invoke();
        else Untriggered?.Invoke();
    }
}
