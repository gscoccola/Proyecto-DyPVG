using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

// This class handles the flow of game turns
// It switches between planning and action phases,
// as well as reverting back to turns

public class TurnManager : Singleton<TurnManager>
{
    [Header("Debug")]
    [ReadOnly] public int CurrentTurnIndex = 0;
    [ReadOnly] public int MaxReachedTurnIndex = 0;
    [ReadOnly] public GameState CurrentState;
    

    private List<IRevertable> _revertables = new();
    private List<IActionable> _actionables = new();
    public List<IPathFollower> ActiveFollowerHistory = new();

    public TextMeshProUGUI TurnText;
    public TextMeshProUGUI StartOrStopText;
    public Button ActionButton;

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
        ActionButton.interactable = false;
        ActiveFollowerHistory.Add(null);
    }

    public void OnNewPathStarted(IPathFollower newActiveFollower)
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


    public void SetActionButton(bool interactable)
    {
        ActionButton.interactable = interactable;
    }

    public void OnActionButtonPress()
    {
        if (CurrentState == GameState.Planning) StartAction();
        else RevertToPlanning();
    }

    public void StartAction()
    {
        CurrentState = GameState.Action;

        foreach (IActionable actionable in _actionables)
        {
            actionable.BeginAction();
        }
        UpdateCanvasText();
    }

    public void RevertToPlanning()
    {
        CurrentState = GameState.Planning;

        foreach (IRevertable revertable in _revertables)
        {
            revertable.RevertToHistoryPoint(CurrentTurnIndex);
        }
        UpdateCanvasText();
    }

    public void ReloadLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void TriggerEndTurn()
    {
        CurrentState = GameState.Planning;
        CurrentTurnIndex++;
        if (CurrentTurnIndex > MaxReachedTurnIndex)  MaxReachedTurnIndex = CurrentTurnIndex;
        foreach (IRevertable revertable in _revertables)
        {
            revertable.SaveHistoryPoint(CurrentTurnIndex, false);
        }
        ActiveFollowerHistory.Add(null);
        UpdateCanvasText();
        SetActionButton(false);
    }

    public void DeleteNextTurns()
    {
        if (MaxReachedTurnIndex > CurrentTurnIndex)
            ActiveFollowerHistory.RemoveRange(CurrentTurnIndex + 1, ActiveFollowerHistory.Count - CurrentTurnIndex -1);
        MaxReachedTurnIndex = CurrentTurnIndex;
    }

    public void RevertToPreviousTurn()
    {
        if (CurrentTurnIndex == 0) return;
        SetActionButton(false);
        CurrentState = GameState.Planning;
        foreach (IRevertable revertable in _revertables)
        {
            revertable.RevertToHistoryPoint(CurrentTurnIndex - 1);
        }
        CurrentTurnIndex--;
        UpdateCanvasText();
    }

    public void RevertToNextTurn()
    {
        if (MaxReachedTurnIndex == CurrentTurnIndex) return;
        SetActionButton(false);
        CurrentState = GameState.Planning;
        foreach (IRevertable revertable in _revertables)
        {
            revertable.RevertToHistoryPoint(CurrentTurnIndex + 1);
        }
        CurrentTurnIndex++;
        UpdateCanvasText();
    }

    private void UpdateCanvasText()
    {
        StartOrStopText.text = CurrentState == GameState.Planning ? "Start" : "Stop";
        TurnText.text = $"Turn {CurrentTurnIndex + 1}";
    }
}

public enum GameState
{
    Planning,
    Action,
}