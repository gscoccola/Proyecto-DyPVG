using UnityEngine;

public class DogIdle : IState<DogState>
{

    private FiniteStateMachine<DogState> _stateMachine;
    private Dog _dog;

    public DogIdle(params object[] parameters)
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