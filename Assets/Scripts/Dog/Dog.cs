using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

// This class represents a dog character that can move on a grid, follow paths, and interact with distractions.

public class Dog : MonoBehaviour, IRevertable, IActionable, IGridCollider, IPathFollower
{

    [Header("Parameters")]
    [SerializeField] public DogSO DogParameters;

    [Header("Reference")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Animator _distractionVFX;

    [Header("Debug")]
    [SerializeField, ReadOnly] public DogState CurrentState;
    [SerializeField, ReadOnly] private Vector2Int _gridPosition;
    [SerializeField, ReadOnly] private List<DogHistoryPoint> StatusHistory = new();
    [ReadOnly] public List<Vector2Int> Path = new();
    [ReadOnly] public List<Transform> collidingList = new();

    [HideInInspector] public PathDrawer Drawer;
    private FiniteStateMachine<DogState> _stateMachine;
    private GridMovement _gridMovement;
    private int _tilesSinceValidPos;

    public TileType[] TraversableTiles { get; set; }

    private bool _seesDistractions;
    private float _distractionDetectionDist;
    private bool _isPathInterrupted;

    private bool _vfxActive;

    private AudioClip[] _actionSFX;

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

        _gridMovement.NewTileReached.AddListener(OnTileReached);
        _gridMovement.LastTileReached.AddListener(() => OnPathFinished());
        _gridMovement.TargetAcquired.AddListener(() => OnTargetAcquired());
        _gridMovement.BeginMovement.AddListener(() => PlayActionSFX());

        _gridMovement.BeginMovement.AddListener(() => SetAnimatorBool(true));
        _gridMovement.LastTileReached.AddListener(() => SetAnimatorBool(false));
        _gridMovement.Interrupted.AddListener(() => SetAnimatorBool(false));
        _gridMovement.Paused.AddListener(() => SetAnimatorBool(false));
        _gridMovement.Resumed.AddListener(() => SetAnimatorBool(true));
    }

    private void LoadSO()
    {
        TraversableTiles = DogParameters.TraversableTiles;
        _seesDistractions = DogParameters.SeesDistractions;
        _distractionDetectionDist = DogParameters.DistractionDetectionDist;
        Drawer.PathParameters = DogParameters;
        _actionSFX = DogParameters.ActionSFX;
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
        if (path.Count == Path.Count || (Path.Count == 1 && path.Count == 0) || (Path.Count == 0 && path.Count == 1)) { }
        else if (path.Count > 1) CanvasManager.Instance.ChangeActivePaths(1);
        else if (path.Count == 1 || path.Count == 0)
        {
            Drawer.ClearAllMarkers();
            CanvasManager.Instance.ChangeActivePaths(-1);
        }
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
        if (deleteFuturePoints && StatusHistory.Count >= turnIndex)
            StatusHistory.RemoveRange(turnIndex, StatusHistory.Count - turnIndex);
        StatusHistory.Add(new DogHistoryPoint(LevelGrid.Instance.WorldToGridPos(transform.position), Path, _vfxActive));
        Drawer.IsDrawingEnabled = true;
    }

    public void RevertToHistoryPoint(int turnIndex)
    {
        SetAnimatorBool(false);
        collidingList.Clear();
        _tilesSinceValidPos = 0;
        _stateMachine.ChangeState(DogState.Stopped);
        _gridMovement.Stop(true);
        transform.position = LevelGrid.Instance.GridToWorldPos(StatusHistory[turnIndex].GridPosition);
        Path = new List<Vector2Int>(StatusHistory[turnIndex].DrawnPath);
        Drawer.ResumePathHitbox.SetActive(Path.Count > 1);
        Drawer.IsDrawingEnabled = true;
        if (Path.Count > 1) CanvasManager.Instance.ChangeActivePaths(1);
        Drawer.RedrawFinishedPath(Path);
        _vfxActive = StatusHistory[turnIndex].VFXActive;
        if (_distractionVFX != null)  _distractionVFX.SetBool("Active", _vfxActive);
    }
    #endregion

    #region ACTIONABLE INTERFACE

    public void BeginAction()
    {
        //Path = Drawer.
        Drawer.ResumePathHitbox.SetActive(false);
        Drawer.IsDrawingEnabled = false;
        _isPathInterrupted = false;
        if (_isPathInterrupted) return;
        if (Path.Count > 1)
        {
            _gridMovement.StartMovement(Path);
            _stateMachine.ChangeState(DogState.MovingToTarget);
            
        }
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

        if (other.GetComponent<Distraction>() != null)
        {
            if (!other.GetComponent<Distraction>().IsDisabled && DogParameters.Type == DogType.Bully) _gridMovement.Pause(1f);
            return;
        }
        if (other.GetComponent<UnitTrigger>() != null) return;
        if (other.GetComponent<Door>() != null && !other.GetComponent<Door>().DisableLingering) return;
        collidingList.Add(other);
    }

    public void OnGridCollisionExit(Transform other)
    {
        if (other.GetComponent<Distraction>() != null) return;
        if (other.GetComponent<UnitTrigger>() != null) return;
        if (other.GetComponent<Door>() != null && !other.GetComponent<Door>().DisableLingering) return;
        collidingList.Remove(other);
    }
    #endregion

    #region GRID MOVEMENT EVENTS

    public void OnTileReached()
    {
        CollisionManager.Instance.UpdateColliderCollisions(transform, true, true);
        if (collidingList.Count == 0 && LevelGrid.Instance.GetTileTypeAtPos(_gridPosition) == TileType.Walkable)
        {
            _tilesSinceValidPos = 0;
        }
        else
            _tilesSinceValidPos++;
        if (LevelGrid.Instance.GetTileTypeAtPos(_gridPosition) == TileType.Jumpable)
        {
            _gridMovement.Slow();
        }
        CheckForDistractions();
    }

    private void CheckForDistractions()
    {
        if (!_seesDistractions || CurrentState == DogState.MovingToDistraction) return;
        foreach (var distraction in LevelManager.Instance.DistractionList)
        {
            // Check if distraction is enabled and in euclidian distance range
            if (distraction.IsDisabled ||
                Vector3.Distance(transform.position, distraction.transform.position) >= _distractionDetectionDist + 3f) continue;
            
            _isPathInterrupted = true;
            var path = LevelGrid.Instance.CalculatePath(TraversableTiles, _gridPosition,
                LevelGrid.Instance.WorldToGridPos(distraction.transform.position), false, default, true);
            // Check if path to distraction exists and is short enough
            if (path.Count == 0 || path.Count > _distractionDetectionDist + 1) continue;
            //Trigger distraction
            if (_distractionVFX != null) SetDistractedVFX(true);
            distraction.DistractedDog = this;
            SFXPlayer.Instance.PlayClip(WorldSounds.Instance.SDogTrash, 1f, true);
            _stateMachine.ChangeState(DogState.MovingToDistraction, distraction);
            path.RemoveAt(path.Count - 1);
            if (path.Count == 0)
            {
                _gridMovement.EndPath();
                return;
            }
            _gridMovement.StartMovement(path);
            Path = path;
            return; 
        }
    }

    public void SetDistractedVFX(bool enabled)
    {
        _vfxActive = enabled;
        if (enabled) StartCoroutine(IEnableVFX());
        else _distractionVFX.SetBool("Active", false);
    }

    private IEnumerator IEnableVFX()
    {
        yield return new WaitForSeconds(0.5f);
        _distractionVFX.SetBool("Active", true);
    }


    private void OnPathFinished(bool checkValidTile = true)
    {
        Drawer.ClearPath();
        switch (IsValidFinishTile())
        {
            case FinishTileType.Valid:
                Drawer.ResumePathHitbox.SetActive(false);
                _stateMachine.ChangeState(DogState.Stopped);
                Path = new();
                //PathDrawerComponent.IsDrawingEnabled = true;
                TurnManager.Instance.TriggerNextActionable();
                break;

            case FinishTileType.GoBack:
                List<Vector2Int> returnPath = new List<Vector2Int>(Path);
                if (_gridMovement.CurrentPathIndex < returnPath.Count - 1)
                    returnPath.RemoveRange(_gridMovement.CurrentPathIndex, returnPath.Count - _gridMovement.CurrentPathIndex);
                //Debug.Log(returnPath.Count);
                //Debug.Log(_tilesSinceValidPos);
                returnPath.RemoveRange(0, returnPath.Count - _tilesSinceValidPos - 1);
                returnPath.Reverse();
                _gridMovement.StartMovement(returnPath, false);
                break;

            case FinishTileType.PushForward:
                
                List<Vector2Int> forwardPath = new List<Vector2Int>() { _gridPosition, _gridPosition + _gridMovement.LastDirection };
                _gridMovement.StartMovement(forwardPath);
                break;
        }
    }

    private FinishTileType IsValidFinishTile()
    {
        if (LevelGrid.Instance.GetTileTypeAtPos(_gridPosition) == TileType.Jumpable)
            return FinishTileType.GoBack;
        else if (Path.Count > 0 && collidingList.Count != 0) return FinishTileType.GoBack;
        else return FinishTileType.Valid;
    }

    private void OnTargetAcquired()
    {
        if (LevelGrid.Instance.GetTileTypeAtPos(LevelGrid.Instance.WorldToGridPos(_gridMovement.CurrentTarget)) == TileType.Jumpable)
        {
            _gridMovement.Slow();
            SFXPlayer.Instance.PlayClip(WorldSounds.Instance.SDogCrawl, 1f, true);
        }
    }

    private void PlayActionSFX()
    {
        SFXPlayer.Instance.PlayClip(_actionSFX);
    }

    private void SetAnimatorBool(bool value)
    {
        
            if (_animator == null) return;
        _animator.SetBool("IsRunning", value);
    }
    #endregion

    public bool HasDrawnPath()
    {
        return Path.Count > 1;
    }
}


public enum DogState
{
    Stopped,
    MovingToTarget,
    MovingToDistraction,
    Idle,
}

public enum FinishTileType
{
    Valid,
    GoBack,
    PushForward,
}


[Serializable]
public class DogHistoryPoint
{
    public Vector2Int GridPosition;
    public List<Vector2Int> DrawnPath;
    public bool VFXActive;

    public DogHistoryPoint(Vector2Int gridPosition, List<Vector2Int> drawnPath, bool vfxActive) 
    {
        GridPosition = gridPosition;
        DrawnPath = drawnPath;
        VFXActive = vfxActive;
    }
}