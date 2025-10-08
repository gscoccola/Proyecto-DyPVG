using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This component moves the GameObject along a given path on the grid.
// Only handles the movement, not the pathfinding or path drawing.

public class GridMovement : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float _moveSpeed = 1f;

    [Header("Debug")]
    [SerializeField, ReadOnly] private MovementStatus _status;
    [ReadOnly] public Vector3 CurrentTarget;
    [ReadOnly] public Vector2Int LastDirection;

    private List<Vector2Int> _currentPath;
    private int _currentPathIndex;
    [HideInInspector] public UnityEvent OnNewTileReached;
    [HideInInspector] public UnityEvent OnLastTileReached;
    [HideInInspector] public UnityEvent OnTargetAcquired;
    
    private void Start()
    {
        _status = MovementStatus.Stopped;
        transform.position = LevelGrid.Instance.SnapToGrid(transform.position);
    }

    private void Update()
    {
        if (_status == MovementStatus.Stopped) { return; }
        if (Vector3.Distance( transform.position, CurrentTarget) < 0.05f)
        {
            OnNewTileReached?.Invoke();
            if (_currentPathIndex == _currentPath.Count - 1)
            {
                transform.position = CurrentTarget;
                _status = MovementStatus.Stopped;
                OnLastTileReached?.Invoke();
                return;
            }
            _currentPathIndex++;
            LastDirection = _currentPath[_currentPathIndex] - _currentPath[_currentPathIndex - 1];
            CurrentTarget = LevelGrid.Instance.GridToWorldPos(_currentPath[_currentPathIndex]);
            OnTargetAcquired?.Invoke();
        }

        transform.position = Vector3.MoveTowards(transform.position,
            CurrentTarget,
            _moveSpeed * Time.deltaTime);
    }

    public void StartMovement(List<Vector2Int> path)
    {
        if (path.Count == 0) return;
        _currentPath = path;
        _currentPathIndex = 0;
        CurrentTarget = LevelGrid.Instance.GridToWorldPos(_currentPath[_currentPathIndex]);
        _status = MovementStatus.Moving;
        OnNewTileReached?.Invoke();
    }

    public void Stop()
    {
        _status = MovementStatus.Stopped;
        _currentPath = new();
        _currentPathIndex = 0;
    }

    public void EndPath()
    {
        Stop();
        OnLastTileReached?.Invoke();
    }

}

public enum MovementStatus
{
    Moving,
    Stopped,
}
