using UnityEngine;

public class Dog : MonoBehaviour, IRevertable, IActionable
{

    [Header("Parameters")]
    [SerializeField] private DogType Type;

    [Header("References")]
    [SerializeField] private GameObject _pathMaker;
    [SerializeField] private GameObject _target;

    [Header("Debug")]
    [SerializeField, ReadOnly] public DogState CurrentState;
    [SerializeField, ReadOnly] private Vector2Int _gridPosition;


    private FiniteStateMachine<DogState> _stateMachine;
    private MarkerManager _markerManager;
    private GridMovement _gridMovement;
    private Vector3 _initialPos;

    private void Awake()
    {
        _gridMovement = GetComponent<GridMovement>();
        _markerManager = GetComponent<MarkerManager>();
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
        _markerManager.ClearAllMarkers();
    }

    public void BeginAction()
    {
        
        var path = LevelGrid.Instance.CalculatePath(new TileType[] { TileType.Walkable },
            transform.position, _target.transform.position);
        if (path.Count == 0)
        {
            _stateMachine.ChangeState(DogState.Idle);
            return;
        }
        _stateMachine.ChangeState(DogState.MovingToTarget);
        foreach (var tilePos in path)
        {
            _markerManager.AddMarker(tilePos, Color.yellow);
        }
        _gridMovement.SetPath(path);
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