using System;

public interface IState<T> where T : Enum
{
    void OnEnter(params object[] parameters);
    void OnUpdate();
    void OnExit();
}