using UnityEngine;
using System.Collections.Generic;

// This class handles the flow of game turns
// It switches between planning and action phases,
// as well as reverting back to turns saved in history

public class TurnManager : Singleton<TurnManager>
{
    [Header("Debug")]
    [ReadOnly] public int CurrentTurnIndex = 0;
    [ReadOnly] public int MaxReachedTurnIndex = 0;
    [ReadOnly] public GameState CurrentState;

    private List<IRevertable> _revertables = new();
    private List<IActionable> _actionables = new();
    public List<IPathFollower> ActiveFollowerHistory = new();

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
        ActiveFollowerHistory.Add(null);
    }

    #endregion

    #region CANVAS BUTTONS
    
    public void OnActionButtonPress()
    {
        if (CurrentState == GameState.Planning) StartActionPhase();
        else InterruptActionPhase();
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
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    #endregion

    #region TURN FLOW

    private void StartActionPhase()
    {
        CurrentState = GameState.Action;
        foreach (IActionable actionable in _actionables)
        {
            actionable.BeginAction();
        }
        CanvasManager.Instance.UpdateDisplayedValues(CurrentState == GameState.Planning);
    }

    // Called when the action phase is interrupted maually
    private void InterruptActionPhase()
    {
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
        CurrentTurnIndex++;
        if (CurrentTurnIndex > MaxReachedTurnIndex) MaxReachedTurnIndex = CurrentTurnIndex;
        foreach (IRevertable revertable in _revertables)
        {
            revertable.SaveHistoryPoint(CurrentTurnIndex, false);
        }
        ActiveFollowerHistory.Add(null);
        CanvasManager.Instance.UpdateDisplayedValues(CurrentState == GameState.Planning);
        CanvasManager.Instance.SetActionButton(false);
    }
    #endregion

    #region TURN LOADING

    private void LoadTurn(int index)
    {
        CanvasManager.Instance.SetActionButton(false);
        CurrentState = GameState.Planning;
        foreach (IRevertable revertable in _revertables)
        {
            revertable.RevertToHistoryPoint(index);
        }
        CollisionManager.Instance.UpdateAllCollisions();
        CurrentTurnIndex = index;
        CanvasManager.Instance.UpdateDisplayedValues(CurrentState == GameState.Planning);
    }

    public void DeleteNextTurnData()
    {
        if (MaxReachedTurnIndex > CurrentTurnIndex)
            ActiveFollowerHistory.RemoveRange(CurrentTurnIndex + 1, ActiveFollowerHistory.Count - CurrentTurnIndex - 1);
        MaxReachedTurnIndex = CurrentTurnIndex;
    }
    #endregion

    #region LOW LEVEL

    // Ensures that only one IPathFollower is active per turn
    public void SetActiveFollower(IPathFollower newActiveFollower)
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
    }
    #endregion
}

public enum GameState
{
    Planning,
    Action,
}