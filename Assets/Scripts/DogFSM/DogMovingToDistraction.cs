using UnityEngine;

public class DogMovingToDistraction : IState<DogState>
{

    private FiniteStateMachine<DogState> _stateMachine;
    private Dog _dog;

    public DogMovingToDistraction(params object[] parameters)
    {
        _dog = (Dog)parameters[0];
        _stateMachine = (FiniteStateMachine<DogState>)parameters[1];
    }


    public void OnEnter(params object[] parameters)
    {
        _dog.CurrentState = DogState.MovingToDistraction;
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {

    }

}