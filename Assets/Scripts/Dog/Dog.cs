using UnityEngine;
using UnityEngine.AI;

public class Dog : MonoBehaviour, IRevertable
{

    [Header("Parameters")]
    [SerializeField] private DogType Type;



    [Header("References")]
    [SerializeField] private GameObject _pathMaker;
    [SerializeField] private GameObject _target;
    [SerializeField] private GameObject _body;

    [Header("Debug")]
    [SerializeField, ReadOnly] private DogState _currentState;
    [SerializeField, ReadOnly] private Vector2Int _gridPosition;


    private FiniteStateMachine<DogState> _stateMachine;


    private void Awake()
    {
        _stateMachine = new FiniteStateMachine<DogState>();
        _stateMachine.AddState(DogState.Stopped, new DogStopped(this, _stateMachine));
        _stateMachine.AddState(DogState.MovingToTarget, new DogMovingToTarget(this, _stateMachine));
        _stateMachine.AddState(DogState.MovingToDistraction, new DogMovingToDistraction(this, _stateMachine));
        _stateMachine.AddState(DogState.Idle, new DogIdle(this, _stateMachine));
    }

    private void Start()
    {
        _stateMachine.ChangeState(DogState.Stopped);
    }

    private void Update()
    {
        _stateMachine.Update();
    }

    public void Revert()
    {
        _stateMachine.ChangeState(DogState.Idle);
    }

    public void StartMovement()
    {
        _stateMachine.ChangeState(DogState.Stopped);
        var path = LevelGrid.Instance.CalculatePath(new TileType[] { TileType.Walkable },
            _body.transform.position, _target.transform.position);
        foreach (var tile in path)
        {
            Instantiate(_pathMaker, LevelGrid.Instance.GridToWorldPos(tile), Quaternion.identity);
        }
    }
}

public enum DogType
{
    Bully,
    Agile,
}

public enum DogState
{
    Stopped,
    MovingToTarget,
    MovingToDistraction,
    Idle,
}