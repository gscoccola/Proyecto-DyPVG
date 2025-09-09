using UnityEngine;
using UnityEngine.AI;

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; }
    [SerializeField] private Dog[] Dogs;
    [SerializeField] private Vector3 Target;

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