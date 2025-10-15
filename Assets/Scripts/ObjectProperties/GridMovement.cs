using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

//This component moves the GameObject along a given path on the grid.
// Only handles the movement, not the pathfinding or path drawing.

public class GridMovement : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _pauseDelay = 0.7f;
    [SerializeField] private float _slowRatio = 0.1f;

    [Header("Debug")]
    [SerializeField, ReadOnly] private MovementStatus _status;
    [ReadOnly] public Vector3 CurrentTarget;
    [ReadOnly] public Vector2Int LastDirection;
    [ReadOnly] public int CurrentPathIndex;

    private List<Vector2Int> _currentPath;
    [HideInInspector] public UnityEvent NewTileReached;
    [HideInInspector] public UnityEvent LastTileReached;
    [HideInInspector] public UnityEvent TargetAcquired;
    [HideInInspector] public UnityEvent BeginMovement;
    private float _pauseTimer;

    public float CurrentMoveSpeed;
    private bool _playedActionSound;
    
    private void Start()
    {
        _status = MovementStatus.Stopped;
        transform.position = LevelGrid.Instance.SnapToGrid(transform.position);
        CurrentMoveSpeed = _moveSpeed;
    }

    private void Update()
    {
        if (_status == MovementStatus.Stopped) { return; }
        if (_status == MovementStatus.Paused)
        {
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer < 0f) Resume();
            return;
        }
        if (Vector3.Distance( transform.position, CurrentTarget) < 0.05f)
        {
            CurrentMoveSpeed = _moveSpeed;
            NewTileReached?.Invoke();
            if (CurrentPathIndex == _currentPath.Count - 1)
            {
                transform.position = CurrentTarget;
                _status = MovementStatus.Stopped;
                LastTileReached?.Invoke();
                return;
            }
            CurrentPathIndex++;
            LastDirection = _currentPath[CurrentPathIndex] - _currentPath[CurrentPathIndex - 1];
            CurrentTarget = LevelGrid.Instance.GridToWorldPos(_currentPath[CurrentPathIndex]);
            TargetAcquired?.Invoke();
        }

        transform.position = Vector3.MoveTowards(transform.position,
            CurrentTarget,
            CurrentMoveSpeed * Time.deltaTime);
    }

    public void StartMovement(List<Vector2Int> path)
    {
        if (path.Count == 0) return;
        _currentPath = path;
        CurrentPathIndex = 0;
        CurrentTarget = LevelGrid.Instance.GridToWorldPos(_currentPath[CurrentPathIndex]);
        _status = MovementStatus.Moving;
        //StartCoroutine(IFootsteps());
        NewTileReached?.Invoke();
        _playedActionSound = false;
        Pause();
    }

    public void Stop()
    {
        _status = MovementStatus.Stopped;
        _currentPath = new();
        //CurrentPathIndex = 0;
    }

    public void EndPath()
    {
        Stop();
        LastTileReached?.Invoke();
    }

    public void Pause()
    {
        _status = MovementStatus.Paused;
        _pauseTimer = _pauseDelay;
    }

    public void Slow()
    {
        CurrentMoveSpeed = _moveSpeed * _slowRatio;
    }

    public void Resume()
    {
        _status = MovementStatus.Moving;
        if (!_playedActionSound)
        {
            BeginMovement?.Invoke();
            _playedActionSound = true;
        }
        StartCoroutine(IFootsteps());
    }

    private IEnumerator IFootsteps()
    {
        while (true)
        {
            if (_status != MovementStatus.Moving) yield break;
            SFXPlayer.Instance.PlayRandomClip(WorldSounds.Instance.DogFootSteps);
            yield return new WaitForSeconds(1.5f / CurrentMoveSpeed);
        }
    }

}

public enum MovementStatus
{
    Moving,
    Stopped,
    Paused,
}
