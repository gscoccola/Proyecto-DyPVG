using UnityEngine;
using System.Collections.Generic;

// This class handles the overall game state, switching between planning and action phases.
public class GameManager : Singleton<GameManager>
{
    [Header("Debug")]
    [SerializeField, ReadOnly] public GameState CurrentState { get; private set; }
    [SerializeField, ReadOnly] public List<Distraction> DistractionList = new();

    private List<IRevertable> _revertables = new();
    private List<IActionable> _actionables = new();
    private Vector3 Target;

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
            revertable.Revert();
        }
    }

    public void ReloadLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}

public enum GameState
{
    Planning,
    Action,
}