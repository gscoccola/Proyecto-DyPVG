using UnityEngine;

public class DogStopped : IState<DogState>
{

    private FiniteStateMachine<DogState> _stateMachine;
    private Dog _dog;

    public DogStopped(params object[] parameters)
    {
        _dog = (Dog)parameters[0];
        _stateMachine = (FiniteStateMachine<DogState>)parameters[1];
    }


    public void OnEnter(params object[] parameters)
    {
        
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {

    }

}
