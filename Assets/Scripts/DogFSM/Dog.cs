using UnityEngine;
using System.Collections.Generic;

public class Dog : MonoBehaviour, IRevertable, IActionable, IGridCollider, IPathFollower
{

    [Header("Parameters")]
    [SerializeField] private DogSO DogParameters;

    public TileType[] TraversableTiles { get; set; }
    private bool _seesDistractions;
    private float _distractionDetectionDist;

    [Header("Debug")]
    [SerializeField, ReadOnly] public DogState CurrentState;
    [SerializeField, ReadOnly] private Vector2Int _gridPosition;
    [SerializeField, ReadOnly] private List<DogHistoryPoint> StatusHistory { get; set; } = new();
    [ReadOnly] public List<Vector2Int> DrawnPath { get; set; } = new();
    [ReadOnly] public List<Vector2Int> DebugDrawnPath;


    [HideInInspector] public PathDrawer PathDrawerComponent;
    private FiniteStateMachine<DogState> _stateMachine;
    private GridMovement _gridMovement;

    private void Awake()
    {
        _gridMovement = GetComponent<GridMovement>();
        PathDrawerComponent = GetComponent<PathDrawer>();
        PathDrawerComponent.PathFollower = this;
        LoadSO();

        _stateMachine = new FiniteStateMachine<DogState>();
        _stateMachine.AddState(DogState.Stopped, new DogStopped(this, _stateMachine));
        _stateMachine.AddState(DogState.MovingToTarget, new DogMovingToTarget(this, _stateMachine));
        _stateMachine.AddState(DogState.MovingToDistraction, new DogMovingToDistraction(this, _stateMachine));
        _stateMachine.AddState(DogState.Idle, new DogIdle(this, _stateMachine));
        _gridMovement.OnTileReached.AddListener(CheckForDistractions);
        _gridMovement.OnPathFinished.AddListener(OnTargetReached);
    }

    private void LoadSO()
    {
        TraversableTiles = DogParameters.TraversableTiles;
        _seesDistractions = DogParameters.SeesDistractions;
        _distractionDetectionDist = DogParameters.DistractionDetectionDist;
    }

    private void Start()
    {
        transform.position = LevelGrid.Instance.SnapToGrid(transform.position);
        _stateMachine.ChangeState(DogState.Stopped);
        PathDrawerComponent.IsDrawingEnabled = true;
        SaveHistoryPoint(0);
    }


    private void OnDestroy()
    {
        _gridMovement.OnTileReached.RemoveListener(CheckForDistractions);
    }

    private void Update()
    {
        _gridPosition = LevelGrid.Instance.WorldToGridPos(transform.position);
        _stateMachine.Update();
        DebugDrawnPath = new List<Vector2Int>(DrawnPath);
    }

    public void SaveHistoryPoint(int turnIndex)
    {
        if (StatusHistory.Count < turnIndex) Debug.LogError("Trying to skip a turn in history");
        if (StatusHistory.Count > turnIndex)
            StatusHistory.RemoveRange(turnIndex, StatusHistory.Count - turnIndex);
        StatusHistory.Add(new DogHistoryPoint(_gridPosition, DrawnPath));
    }

    public void SetDrawnPath(List<Vector2Int> path)
    {
        DrawnPath = new List<Vector2Int>(path);
        SaveHistoryPoint(TurnManager.Instance.CurrentTurnIndex);
    }

    public void RevertToHistoryPoint(int turnIndex)
    {
        _stateMachine.ChangeState(DogState.Stopped);
        _gridMovement.Stop();
        transform.position = LevelGrid.Instance.GridToWorldPos(StatusHistory[turnIndex].GridPosition);
        DrawnPath = new List<Vector2Int>(StatusHistory[turnIndex].DrawnPath);
        PathDrawerComponent.IsDrawingEnabled = true;
        if (turnIndex == TurnManager.Instance.CurrentTurnIndex) return;

        PathDrawerComponent.RedrawFinishedPath(DrawnPath);
    }

    public void BeginAction()
    {
        if (DrawnPath.Count == 0) return;
        PathDrawerComponent.IsDrawingEnabled = false;
        _gridMovement.StartMovement(DrawnPath);
        if (DrawnPath.Count > 0)
            _stateMachine.ChangeState(DogState.MovingToTarget);
        else _stateMachine.ChangeState(DogState.Idle);
    }

    public void CheckForDistractions()
    {
        if (!_seesDistractions || CurrentState == DogState.MovingToDistraction) return;
        foreach (var distraction in TurnManager.Instance.DistractionList)
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

    private void OnTargetReached()
    {
        _stateMachine.ChangeState(DogState.Stopped);
        DrawnPath = new();
        PathDrawerComponent.ClearPath();
        PathDrawerComponent.IsDrawingEnabled = true;
        TurnManager.Instance.TriggerNextTurn();
    }


    public void OnGridCollisionEnter(Transform other)
    {
        //if (other.GetComponent<Distraction>() == null) return;
    }

    public void OnGridCollisionExit(Transform other)
    {
        //if (other.GetComponent<Distraction>() == null) return;
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

public class DogHistoryPoint
{
    public Vector2Int GridPosition;
    public List<Vector2Int> DrawnPath;

    public DogHistoryPoint(Vector2Int gridPosition, List<Vector2Int> drawnPath)
    {
        GridPosition = gridPosition;
        DrawnPath = drawnPath;
    }
}