using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// This class handles the flow of game turns
// It switches between planning and action phases,
// as well as reverting back to turns saved in history

public class TurnManager : Singleton<TurnManager>
{
    [Header("Debug")]
    [ReadOnly] public int CurrentTurnIndex = 0;
    [ReadOnly] public int MaxReachedTurnIndex = 0;
    [ReadOnly] public GameState CurrentState;
    [ReadOnly] public int TurnsLeft;
    

    private List<TurnOrder> _orderHistory = new();
    private int _activeActionableIndex;

    private List<IRevertable> _revertables = new();
    private List<IActionable> _actionables = new();

    [HideInInspector] public UnityEvent TurnEnd;
    //public List<IPathFollower> ActiveFollowerHistory = new();

    #region SETUP

    private new void Awake()
    {
        base.Awake();
        foreach (GameObject gameObject in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (gameObject.GetComponent<IRevertable>() != null) _revertables.Add(gameObject.GetComponent<IRevertable>());
            if (gameObject.GetComponent<IActionable>() != null) _actionables.Add(gameObject.GetComponent<IActionable>());
        }
    }

    private void Start()
    {
        CurrentState = GameState.Planning;
        TurnsLeft = PersistentInfo.Instance.LevelsInfoSO.LevelsInfo[SceneManager.GetActiveScene().buildIndex -2].MaxTurns;
        //ActiveFollowerHistory.Add(null);
    }
    #endregion

    #region CANVAS BUTTONS
    
    public void OnActionButtonPress()
    {
        if (CurrentState == GameState.Planning) StartActionPhase();
        //else InterruptActionPhase();
    }

    public void RevertToPreviousTurn()
    {
        if (CurrentTurnIndex == 0) return;
        LoadTurn(CurrentTurnIndex - 1);
    }

    public void RevertToNextTurn()
    {
        if (MaxReachedTurnIndex == CurrentTurnIndex) return;
        LoadTurn(CurrentTurnIndex + 1);
    }

    public void ReloadLevel()
    {
        SceneTransition.Instance.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void PreviousLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    #endregion

    #region TURN FLOW

    private void StartActionPhase()
    {
        
        CurrentState = GameState.Action;
        _activeActionableIndex = 0;
        _orderHistory[CurrentTurnIndex].OrderedActionables[0].BeginAction();
        CanvasManager.Instance.UpdateDisplayedValues(CurrentState == GameState.Planning);
    }

    public void TriggerNextActionable()
    {
        _activeActionableIndex++;
        if (_activeActionableIndex >= _orderHistory[CurrentTurnIndex].OrderedActionables.Length)
        {
            TriggerEndTurn();
        }
        else
        {
            _orderHistory[CurrentTurnIndex].OrderedActionables[_activeActionableIndex].BeginAction();
        }
    }

    // Called when the action phase is interrupted maually
    private void InterruptActionPhase()
    {
        SFXPlayer.Instance.InterruptSounds();
        CurrentState = GameState.Planning;
        foreach (IRevertable revertable in _revertables)
        {
            revertable.RevertToHistoryPoint(CurrentTurnIndex);
        }
        CanvasManager.Instance.UpdateDisplayedValues(CurrentState == GameState.Planning);
    }

    // Called when the action phase reaches it's natural end
    public void TriggerEndTurn()
    {
        CurrentState = GameState.Planning;
        _orderHistory.Add(_orderHistory[CurrentTurnIndex]);
        CurrentTurnIndex++;
        if (CurrentTurnIndex > MaxReachedTurnIndex) MaxReachedTurnIndex = CurrentTurnIndex;
        foreach (IRevertable revertable in _revertables)
        {
            revertable.SaveHistoryPoint(CurrentTurnIndex, false);
        }
        //ActiveFollowerHistory.Add(null);
        CanvasManager.Instance.SetActionButton(false);
        TurnEnd?.Invoke();
        TurnsLeft = PersistentInfo.Instance.LevelsInfoSO.LevelsInfo[SceneManager.GetActiveScene().buildIndex - 2].MaxTurns - CurrentTurnIndex;
        CheckForLose();
        CanvasManager.Instance.UpdateDisplayedValues(CurrentState == GameState.Planning);
    }

    private void CheckForLose()
    {
        if (TurnsLeft == 0)
            CanvasManager.Instance.SetLosePanel(true);
    }
    #endregion

    #region TURN LOADING

    private void LoadTurn(int index)
    {
        SFXPlayer.Instance.InterruptSounds();
        CanvasManager.Instance.SetActionButton(false);
        CurrentState = GameState.Planning;
        foreach (IRevertable revertable in _revertables)
        {
            revertable.RevertToHistoryPoint(index);
        }
        CollisionManager.Instance.UpdateAllCollisions();
        CurrentTurnIndex = index;
        CanvasManager.Instance.UpdateDisplayedValues(CurrentState == GameState.Planning);
        CanvasManager.Instance.SetChipOrder(_orderHistory[CurrentTurnIndex]);
        TurnsLeft = PersistentInfo.Instance.LevelsInfoSO.LevelsInfo[SceneManager.GetActiveScene().buildIndex - 2].MaxTurns - CurrentTurnIndex;
    }

    public void DeleteNextTurnData()
    {
        foreach (IRevertable revertable in _revertables)
        {
            revertable.SaveHistoryPoint(CurrentTurnIndex, true);
        }
        if (MaxReachedTurnIndex > CurrentTurnIndex)
            _orderHistory.RemoveRange(CurrentTurnIndex + 1, _orderHistory.Count - CurrentTurnIndex - 1);
        MaxReachedTurnIndex = CurrentTurnIndex;
    }
    #endregion

    #region LOW LEVEL

    // Ensures that only one IPathFollower is active per turn
    /*public void SetActiveFollower(IPathFollower newActiveFollower)
    {
        if (ActiveFollowerHistory[CurrentTurnIndex] == null)
        {
            ActiveFollowerHistory[CurrentTurnIndex] = newActiveFollower;
        }
        if (ActiveFollowerHistory[CurrentTurnIndex] != null && ActiveFollowerHistory[CurrentTurnIndex] != newActiveFollower)
        {
            ActiveFollowerHistory[CurrentTurnIndex].SetDrawnPath(new List<Vector2Int>());
            ActiveFollowerHistory[CurrentTurnIndex] = newActiveFollower;
        }
    }*/

    public void SetCurrentTurnOrder(TurnOrder turnOrder)
    {
        DeleteNextTurnData();
        if (_orderHistory.Count - 1 < CurrentTurnIndex)
            _orderHistory.Add(turnOrder);
        else
            _orderHistory[CurrentTurnIndex] = turnOrder;
    }
    #endregion
}

public enum GameState
{
    Planning,
    Action,
}

public class TurnOrder
{
    public IActionable[] OrderedActionables;

    public TurnOrder(IActionable[] actionablesInOrder)
    {
        OrderedActionables = actionablesInOrder;
    }

    public TurnOrder(List<IActionable> actionablesInOrder)
    {
        OrderedActionables = actionablesInOrder.ToArray();
    }

    public string PrintOrder()
    {
        string order = "Turn Order: ";
        foreach (IActionable actionable in OrderedActionables)
        {
            order += actionable.ToString() + " -> ";
        }
        return order;
    }
}