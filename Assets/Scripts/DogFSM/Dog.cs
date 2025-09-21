using UnityEngine;
using System.Collections.Generic;

public class Dog : MonoBehaviour, IRevertable, IActionable, IGridCollider
{

    [Header("Parameters")]
    [SerializeField] private DogSO DogParameters;

    public TileType[] TraversableTiles => DogParameters.TraversableTiles;
    private bool _seesDistractions => DogParameters.SeesDistractions;
    private float _distractionDetectionDist => DogParameters.DistractionDetectionDist;

    [Header("Debug")]
    [SerializeField, ReadOnly] public DogState CurrentState;
    [SerializeField, ReadOnly] private Vector2Int _gridPosition;


    [HideInInspector] public List<Vector2Int> DrawnPath;
    [HideInInspector] public PathDrawer PathDrawerComponent;
    private FiniteStateMachine<DogState> _stateMachine;
    private GridMovement _gridMovement;
    private Vector3 _initialPos;

    private void Awake()
    {
        _gridMovement = GetComponent<GridMovement>();
        PathDrawerComponent = GetComponent<PathDrawer>();

        _stateMachine = new FiniteStateMachine<DogState>();
        _stateMachine.AddState(DogState.Stopped, new DogStopped(this, _stateMachine));
        _stateMachine.AddState(DogState.MovingToTarget, new DogMovingToTarget(this, _stateMachine));
        _stateMachine.AddState(DogState.MovingToDistraction, new DogMovingToDistraction(this, _stateMachine));
        _stateMachine.AddState(DogState.Idle, new DogIdle(this, _stateMachine));
        _gridMovement.OnTileReached.AddListener(CheckForDistractions);
    }

    private void Start()
    {
        transform.position = LevelGrid.Instance.SnapToGrid(transform.position);
        _initialPos = transform.position;
        _stateMachine.ChangeState(DogState.Stopped);
        PathDrawerComponent.IsDrawingEnabled = true;
    }

    private void OnDestroy()
    {
        _gridMovement.OnTileReached.RemoveListener(CheckForDistractions);
    }

    private void Update()
    {
        _gridPosition = LevelGrid.Instance.WorldToGridPos(transform.position);
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
        _gridMovement.StartMovement(DrawnPath);
    }

    public void CheckForDistractions()
    {
        if (!_seesDistractions || CurrentState == DogState.MovingToDistraction) return;
        foreach (var distraction in GameManager.Instance.DistractionList)
        {
            if (distraction.GetComponent<Distraction>() == null || distraction.GetComponent<Distraction>().IsOccupied) continue;
            if (Vector3.Distance(transform.position, distraction.transform.position) < _distractionDetectionDist)     
            {
                var path = LevelGrid.Instance.CalculatePath(TraversableTiles,_gridPosition,
                    LevelGrid.Instance.WorldToGridPos(distraction.transform.position));
                if (path.Count == 0 || path.Count > _distractionDetectionDist) continue;

                _stateMachine.ChangeState(DogState.MovingToDistraction, distraction);
                _gridMovement.StartMovement(path);
                return;
            }
        }
    }

    public void OnGridCollisionEnter(Transform other)
    {
        if (other.GetComponent<Distraction>() == null) return;
    }

    public void OnGridCollisionExit(Transform other)
    {
        if (other.GetComponent<Distraction>() == null) return;
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