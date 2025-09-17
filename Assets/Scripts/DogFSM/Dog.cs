using UnityEngine;
using System.Collections.Generic;

public class Dog : MonoBehaviour, IRevertable, IActionable
{

    [Header("Parameters")]
    [SerializeField] private DogType Type;
    [SerializeField] public TileType[] TraversableTiles = new TileType[] { TileType.Walkable };

    [Header("Debug")]
    [SerializeField, ReadOnly] public DogState CurrentState;
    [SerializeField, ReadOnly] private Vector2Int _gridPosition;


    [HideInInspector] public List<Vector2Int> CurrentPath;
    [HideInInspector] public PathDrawer PathDrawerComponent;
    private FiniteStateMachine<DogState> _stateMachine;
    private MarkerManager _markerManager;
    private GridMovement _gridMovement;
    private Vector3 _initialPos;

    private void Awake()
    {
        _gridMovement = GetComponent<GridMovement>();
        _markerManager = GetComponent<MarkerManager>();
        PathDrawerComponent = GetComponent<PathDrawer>();

        _stateMachine = new FiniteStateMachine<DogState>();
        _stateMachine.AddState(DogState.Stopped, new DogStopped(this, _stateMachine));
        _stateMachine.AddState(DogState.MovingToTarget, new DogMovingToTarget(this, _stateMachine));
        _stateMachine.AddState(DogState.MovingToDistraction, new DogMovingToDistraction(this, _stateMachine));
        _stateMachine.AddState(DogState.Idle, new DogIdle(this, _stateMachine));
    }

    private void Start()
    {
        transform.position = LevelGrid.Instance.SnapToGrid(transform.position);
        _initialPos = transform.position;
        _stateMachine.ChangeState(DogState.Stopped);
        PathDrawerComponent.IsDrawingEnabled = true;
    }

    private void Update()
    {
        _stateMachine.Update();
    }

    public void Revert()
    {
        _stateMachine.ChangeState(DogState.Stopped);
        _gridMovement.Stop();
        transform.position = _initialPos;
        PathDrawerComponent.IsDrawingEnabled = true;
    }

    public void BeginAction()
    {
        PathDrawerComponent.IsDrawingEnabled = false;
        _gridMovement.MoveAlongPath(CurrentPath);
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