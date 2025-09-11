using UnityEngine;
using UnityEngine.AI;

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; }
    private Dog[] Dogs;
    private Vector3 Target;

    private new void Awake()
    {
        base.Awake();
        Dogs = FindObjectsByType<Dog>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    private void Start()
    {
        CurrentState = GameState.Planning;
    }

    public void StartAction()
    {
        if (CurrentState == GameState.Action) return;
        CurrentState = GameState.Action;
        foreach (Dog dog in Dogs)
        {
            dog.StartMovement();
        }
    }

    public void RevertToPlanning()
    {
        if (CurrentState == GameState.Planning) return;
        CurrentState = GameState.Planning;
        foreach (Dog dog in Dogs)
        {
            dog.Revert();
        }
    }
}

public enum GameState
{
    Planning,
    Action,
}