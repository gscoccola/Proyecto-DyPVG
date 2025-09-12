using UnityEngine;

public class DogMovingToTarget : IState<DogState>
{

    private FiniteStateMachine<DogState> _stateMachine;
    private Dog _dog;

    public DogMovingToTarget(params object[] parameters)
    {
        _dog = (Dog)parameters[0];
        _stateMachine = (FiniteStateMachine<DogState>)parameters[1];
    }


    public void OnEnter(params object[] parameters)
    {
        _dog.CurrentState = DogState.MovingToTarget;
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {

    }

}