using UnityEngine;
using System.Collections.Generic;

public class PathDrawer : MonoBehaviour
{
    [HideInInspector] public bool IsDrawingEnabled;
    private bool _isDrawingPath;
    private Dog _dog;
    private Vector2Int _currentMouseGridPos;
    private Vector2Int _lastMouseGridPos;
    private List<Vector2Int> _path = new();
    private Camera _camera;
    private MarkerManager _markerManager;
    private float _markerColorOffset = 0f;

    private void Awake()
    {
        _camera = FindAnyObjectByType<Camera>();
        _markerManager = GetComponent<MarkerManager>();
        _dog = GetComponent<Dog>();
    }

    private void Update()
    {
        if (!_isDrawingPath) return;
        
        _currentMouseGridPos = LevelGrid.Instance.WorldToGridPos(
            _camera.ScreenToWorldPoint(Input.mousePosition)
            );

        if (_currentMouseGridPos != _lastMouseGridPos) GenerateSubPathToMouse();
    }

    private void OnMouseDown()
    {
        if (!IsDrawingEnabled) return;
        StartPath();
    }

    private void OnMouseUp()
    {
        if (!_isDrawingPath) return;
        FinishPath();
    }

    private void GenerateSubPathToMouse()
    {
        var path = LevelGrid.Instance.CalculatePath(new TileType[] { TileType.Walkable },
            _lastMouseGridPos, _currentMouseGridPos);
        path.Reverse();
        if (path.Count == 0) return;
        foreach (var tile in path)
        {
            _markerColorOffset += 0.08f;
            _markerManager.AddMarker(tile, Color.yellow + new Color(_markerColorOffset, _markerColorOffset, _markerColorOffset));
            _path.Add(tile);
        }
        _lastMouseGridPos = _currentMouseGridPos;
    }

    private void StartPath()
    {
        _markerManager.ClearAllMarkers();
        _isDrawingPath = true;
        _lastMouseGridPos = LevelGrid.Instance.WorldToGridPos(transform.position);
        _currentMouseGridPos = _lastMouseGridPos;
    }

    private void FinishPath()
    {
        _isDrawingPath = false;
        _path.Reverse();
        _dog.CurrentPath = new List<Vector2Int>(_path);
        _path.Clear();
        _markerColorOffset = 0f;
    }

}
