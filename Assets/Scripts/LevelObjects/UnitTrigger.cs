using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class UnitTrigger : MonoBehaviour, IGridCollider, IRevertable
{
    [Header("Parameters")]
    //[SerializeField] private DogType _requiredType;
    [SerializeField] private bool _mustBeHeld;
    [SerializeField] private RequiredType _requiredType;

    [Header("Debug")]
    [ReadOnly] public bool IsTriggered;
    [HideInInspector] public UnityEvent<bool> Triggered;
    [HideInInspector] public UnityEvent<bool> Untriggered;
    [HideInInspector] public List<bool> TriggeredHistory = new();

    [SerializeField] private int _occupyingDogs = 0; 

    private void Start()
    {
        TriggeredHistory.Add(false);
    }

    #region GRID COLLIDER INTERFACE

    public void OnGridCollisionEnter(Transform other)
    {
        Dog dog = other.GetComponent<Dog>();
        if (dog == null) return;
        if (_requiredType == RequiredType.Bully && dog.DogParameters.Type != DogType.Bully) return;
        if (_requiredType == RequiredType.Water && dog.DogParameters.Type != DogType.Water) return;
        _occupyingDogs++;
        ToggleTriggered(true);
    }

    public void OnGridCollisionExit(Transform other)
    {
        Dog dog = other.GetComponent<Dog>();
        if (dog == null) return;
        if(_requiredType == RequiredType.Bully && dog.DogParameters.Type != DogType.Bully) return;
        if (_requiredType == RequiredType.Water && dog.DogParameters.Type != DogType.Water) return;
        if (!_mustBeHeld) return;
        _occupyingDogs--;
        if (_occupyingDogs == 0) ToggleTriggered(false);
    }
    #endregion

    #region REVERTABLE INTERFACE

    public void RevertToHistoryPoint(int turnIndex)
    {
        _occupyingDogs = TriggeredHistory[turnIndex] ? 1 : 0;
        ToggleTriggered(TriggeredHistory[turnIndex], false);
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

    public void ToggleTriggered(bool triggered, bool triggerSFX = true)
    {
        if (IsTriggered == triggered) return; 
        IsTriggered = triggered;
        if (triggered) Triggered?.Invoke(triggerSFX);
        else Untriggered?.Invoke(triggerSFX);
    }

}

public enum RequiredType
{
    Any,
    Bully,
    Water,
}
