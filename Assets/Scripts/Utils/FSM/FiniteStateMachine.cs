using System;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine<T> where T : Enum
{
    private IState<T> _currentState;
    private readonly Dictionary<T, IState<T>> _allStates = new();

    
    #region FSM
    public void Update()
    {
        _currentState.OnUpdate();
    }

    public void ChangeState(T state, params object[] parameters)
    {
        if (!_allStates.ContainsKey(state))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"FSM: El estado {state} no existe.");
#endif
            return;
        }
        _currentState?.OnExit();
        _currentState = _allStates[state];
        _currentState.OnEnter(parameters);
    }

    public void AddState(T state, IState<T> value)
    {
        if (_allStates.ContainsKey(state))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"FSM: El estado {state} ya existe, será reemplazado.");
#endif
        }
        _allStates[state] = value;
    }

    public void RemoveState(T state)
    {
        if (!_allStates.Remove(state))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"FSM: No se encontró el estado {state} para eliminar.");
#endif
        }
    }

    public void ClearStates()
    {
        _allStates.Clear();
        _currentState = null;
    }
    
    public IState<T> GetState(T state)
    {
        if (!_allStates.ContainsKey(state))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"FSM: No se encontró el estado {state}.");
            return null;
#endif
        }
        return _allStates[state];
    }
    #endregion
}