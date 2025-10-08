using UnityEngine;
using System.Collections.Generic;
using System;

// This class represents a dog character that can move on a grid, follow paths, and interact with distractions.

public class Dog : MonoBehaviour, IRevertable, IActionable, IGridCollider, IPathFollower
{

    [Header("Parameters")]
    [SerializeField] private DogSO DogParameters;

    

    [Header("Debug")]
    [SerializeField, ReadOnly] public DogState CurrentState;
    [SerializeField, ReadOnly] private Vector2Int _gridPosition;
    [SerializeField, ReadOnly] private List<DogHistoryPoint> StatusHistory = new();
    [ReadOnly] public List<Vector2Int> Path { get; set; } = new();
    [ReadOnly] public List<Vector2Int> DebugDrawnPath;
    [ReadOnly] public List<Transform> collidingList = new();

    [HideInInspector] public PathDrawer Drawer;
    [HideInInspector] public string Initial;
    private FiniteStateMachine<DogState> _stateMachine;
    private GridMovement _gridMovement;
    private int _tilesSinceValidPos;

    public TileType[] TraversableTiles { get; set; }

    private bool _seesDistractions;
    private float _distractionDetectionDist;
    private bool _isPathInterrupted;

    #region SETUP

    private void Awake()
    {
        _gridMovement = GetComponent<GridMovement>();
        Drawer = GetComponent<PathDrawer>();
        Drawer.PathFollower = this;
        LoadSO();

        _stateMachine = new FiniteStateMachine<DogState>();
        _stateMachine.AddState(DogState.Stopped, new DogStopped(this, _stateMachine));
        _stateMachine.AddState(DogState.MovingToTarget, new DogMovingToTarget(this, _stateMachine));
        _stateMachine.AddState(DogState.MovingToDistraction, new DogMovingToDistraction(this, _stateMachine));
        _stateMachine.AddState(DogState.Idle, new DogIdle(this, _stateMachine));

        _gridMovement.OnNewTileReached.AddListener(OnTileReached);
        _gridMovement.OnLastTileReached.AddListener(() => OnPathFinished());
    }

    private void LoadSO()
    {
        Initial = DogParameters.DogName[0].ToString();
        TraversableTiles = DogParameters.TraversableTiles;
        _seesDistractions = DogParameters.SeesDistractions;
        _distractionDetectionDist = DogParameters.DistractionDetectionDist;
        Drawer.PathParameters = DogParameters;
    }

    private void Start()
    {
        transform.position = LevelGrid.Instance.SnapToGrid(transform.position);
        _stateMachine.ChangeState(DogState.Stopped);
        Drawer.IsDrawingEnabled = true;
        SaveHistoryPoint(0);
        Drawer.ResumePathHitbox.SetActive(false);
    }
    #endregion


    /*private void OnDestroy()
    {
        _gridMovement.OnNewTileReached.RemoveListener(CheckForDistractions);
    }*/

    private void Update()
    {
        _gridPosition = LevelGrid.Instance.WorldToGridPos(transform.position);
        /*_stateMachine.Update();
        DebugDrawnPath = new List<Vector2Int>(DrawnPath);*/
    }

    #region PATH FOLLOWER INTERFACE

    public void SetDrawnPath(List<Vector2Int> path)
    {
        if (path.Count > 1) CanvasManager.Instance.SetActionButton(true);
        if (path.Count == 1) Drawer.ClearAllMarkers();
        TurnManager.Instance.DeleteNextTurnData();
        Path = new List<Vector2Int>(path);
        SaveHistoryPoint(TurnManager.Instance.CurrentTurnIndex);
        Drawer.ResumePathHitbox.SetActive(true);
    }
    #endregion

    #region REVERTABLE INTERFACE

    public void SaveHistoryPoint(int turnIndex, bool deleteFuturePoints = true)
    {
        if (StatusHistory.Count < turnIndex) Debug.LogError("Trying to skip a turn in history");
        if (deleteFuturePoints && StatusHistory.Count > turnIndex)
            StatusHistory.RemoveRange(turnIndex, StatusHistory.Count - turnIndex);
        StatusHistory.Add(new DogHistoryPoint(LevelGrid.Instance.WorldToGridPos(transform.position), Path));
        Drawer.IsDrawingEnabled = true;
    }

    public void RevertToHistoryPoint(int turnIndex)
    {
        _stateMachine.ChangeState(DogState.Stopped);
        _gridMovement.Stop();
        transform.position = LevelGrid.Instance.GridToWorldPos(StatusHistory[turnIndex].GridPosition);
        Path = new List<Vector2Int>(StatusHistory[turnIndex].DrawnPath);
        Drawer.ResumePathHitbox.SetActive(Path.Count > 1);
        Drawer.IsDrawingEnabled = true;
        if (Path.Count > 1) CanvasManager.Instance.SetActionButton(true);
        Drawer.RedrawFinishedPath(Path);
    }
    #endregion

    #region ACTIONABLE INTERFACE

    public void BeginAction()
    {
        Drawer.ResumePathHitbox.SetActive(false);
        Drawer.IsDrawingEnabled = false;
        _isPathInterrupted = false;
        _gridMovement.StartMovement(Path);
        if (_isPathInterrupted) return;
        if (Path.Count > 1)
            _stateMachine.ChangeState(DogState.MovingToTarget);
        else
        {
            _stateMachine.ChangeState(DogState.Idle);
            OnPathFinished();
        } 
    }
    #endregion

    #region GRID COLLIDER INTERFACE

    public void OnGridCollisionEnter(Transform other)
    {
        if (other.GetComponent<Distraction>() != null) return;
        collidingList.Add(other);
    }

    public void OnGridCollisionExit(Transform other)
    {
        if (other.GetComponent<Distraction>() != null) return;
        collidingList.Remove(other);
    }
    #endregion

    public void OnTileReached()
    {
        CollisionManager.Instance.UpdateColliderCollisions(transform, true, true);
        if (collidingList.Count == 0)
        {
            _tilesSinceValidPos = 0;
        }
        else
            _tilesSinceValidPos++;
        CheckForDistractions();
    }

    private void CheckForDistractions()
    {
        if (!_seesDistractions || CurrentState == DogState.MovingToDistraction) return;
        foreach (var distraction in LevelManager.Instance.DistractionList)
        {
            if (distraction.IsDisabled) continue;
            if (Vector3.Distance(transform.position, distraction.transform.position) < _distractionDetectionDist)
            {
                _isPathInterrupted = true;
                var path = LevelGrid.Instance.CalculatePath(TraversableTiles, _gridPosition,
                    LevelGrid.Instance.WorldToGridPos(distraction.transform.position));
                if (path.Count == 0 || path.Count > _distractionDetectionDist + 1) continue;
                _stateMachine.ChangeState(DogState.MovingToDistraction, distraction);
                path.RemoveAt(path.Count - 1);
                if (path.Count == 0)
                {
                    //OnPathFinished(false);
                    _gridMovement.EndPath();
                    return;
                }
                _gridMovement.StartMovement(path);
                Path = path;
                return;
            }
        }
    }

    private void OnPathFinished(bool checkValidTile = true)
    {
        Drawer.ClearPath();
        if (Path.Count > 0 && !IsValidFinishTile())
        {
            List<Vector2Int> returnPath = new List<Vector2Int>(Path);
            returnPath.RemoveRange(0, Path.Count - _tilesSinceValidPos - 1);
            returnPath.Reverse();
            _gridMovement.StartMovement(returnPath);
            return;
        }
        Drawer.ResumePathHitbox.SetActive(false);
        _stateMachine.ChangeState(DogState.Stopped);
        Path = new();
        //PathDrawerComponent.IsDrawingEnabled = true;
        TurnManager.Instance.TriggerNextActionable();
    }

    private bool IsValidFinishTile()
    {
        return collidingList.Count == 0;
    }

    public bool HasDrawnPath()
    {
        return Path.Count > 1;
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


[Serializable]
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