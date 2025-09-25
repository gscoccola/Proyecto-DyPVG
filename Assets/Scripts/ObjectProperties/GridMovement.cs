using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This component moves the GameObject along a given path on the grid.
public class GridMovement : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float _moveSpeed = 1f;

    [Header("Debug")]
    [SerializeField, ReadOnly] private MovementStatus _status;
    [SerializeField, ReadOnly] private Vector3 _currentTarget;

    private List<Vector2Int> _currentPath;
    private int _currentPathIndex;
    public UnityEvent OnNewTileReached;
    public UnityEvent OnLastTileReached;
    
    private void Start()
    {
        _status = MovementStatus.Stopped;
        transform.position = LevelGrid.Instance.SnapToGrid(transform.position);
    }

    private void Update()
    {
        if (_status == MovementStatus.Stopped) { return; }
        if (Vector3.Distance( transform.position, _currentTarget) < 0.05f)
        {
            OnNewTileReached?.Invoke();
            if (_currentPathIndex == _currentPath.Count - 1)
            {
                transform.position = _currentTarget;
                _status = MovementStatus.Stopped;
                OnLastTileReached?.Invoke();
                return;
            }
            _currentPathIndex++;
            _currentTarget = LevelGrid.Instance.GridToWorldPos(_currentPath[_currentPathIndex]);
        }

        transform.position = Vector3.MoveTowards(transform.position,
            _currentTarget,
            _moveSpeed * Time.deltaTime);
    }

    public void StartMovement(List<Vector2Int> path)
    {
        if (path.Count == 0) return;
        _currentPath = path;
        _currentPathIndex = 0;
        _currentTarget = LevelGrid.Instance.GridToWorldPos(_currentPath[_currentPathIndex]);
        _status = MovementStatus.Moving; 
    }

    public void Stop()
    {
        _status = MovementStatus.Stopped;
    }

}

public enum MovementStatus
{
    Moving,
    Stopped,
}
