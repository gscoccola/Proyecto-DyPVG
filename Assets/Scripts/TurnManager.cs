using UnityEngine;
using System.Collections.Generic;
using TMPro;

// This class handles the overall game state, switching between planning and action phases.
public class TurnManager : Singleton<TurnManager>
{
    [Header("Debug")]
    [ReadOnly] public IPathFollower ActiveDog;
    [ReadOnly] public int CurrentTurnIndex = 0;
    [ReadOnly] public int MaxReachedTurnIndex = 0;
    [ReadOnly] public GameState CurrentState;
    [ReadOnly] public List<Distraction> DistractionList = new();

    private List<IRevertable> _revertables = new();
    private List<IActionable> _actionables = new();
    private Vector3 Target;

    public TextMeshProUGUI TurnText;

    private new void Awake()
    {
        base.Awake();
        foreach (GameObject gameObject in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (gameObject.GetComponent<IRevertable>() != null) _revertables.Add(gameObject.GetComponent<IRevertable>());
            if (gameObject.GetComponent<IActionable>() != null) _actionables.Add(gameObject.GetComponent<IActionable>());
            if (gameObject.GetComponent<Distraction>() != null) DistractionList.Add(gameObject.GetComponent<Distraction>());
        }
    }

    private void Start()
    {
        CurrentState = GameState.Planning;
    }

    public void StartAction()
    {
        if (CurrentState == GameState.Action) return;
        CurrentState = GameState.Action;

        foreach (IActionable actionable in _actionables)
        {
            actionable.BeginAction();
        }
    }

    public void RevertToPlanning()
    {
        if (CurrentState == GameState.Planning) return;
        CurrentState = GameState.Planning;

        foreach (IRevertable revertable in _revertables)
        {
            revertable.RevertToHistoryPoint(CurrentTurnIndex);
        }
    }

    public void ReloadLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void TriggerNextTurn()
    {
        CurrentState = GameState.Planning;
        CurrentTurnIndex++;
        TurnText.text = $"Turn {CurrentTurnIndex + 1}";
        MaxReachedTurnIndex = CurrentTurnIndex;
        foreach (IRevertable revertable in _revertables)
        {
            revertable.SaveHistoryPoint(CurrentTurnIndex);
        }
    }

    public void MoveBack()
    {
        if (CurrentTurnIndex == 0) return;
        CurrentState = GameState.Planning;
        foreach (IRevertable revertable in _revertables)
        {
            revertable.RevertToHistoryPoint(CurrentTurnIndex - 1);
        }
        CurrentTurnIndex--;
        TurnText.text = $"Turn {CurrentTurnIndex + 1}";
    }

    public void MoveForward()
    {
        if (MaxReachedTurnIndex == CurrentTurnIndex) return;
        CurrentState = GameState.Planning;
        foreach (IRevertable revertable in _revertables)
        {
            revertable.RevertToHistoryPoint(CurrentTurnIndex + 1);
        }
        CurrentTurnIndex++;
        TurnText.text = $"Turn {CurrentTurnIndex + 1}";
    }
}

public enum GameState
{
    Planning,
    Action,
}