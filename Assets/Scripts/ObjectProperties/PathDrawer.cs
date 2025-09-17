using UnityEngine;
using System.Collections.Generic;

// This component allows the player to draw a path for the Dog on the grid by clicking and dragging the mouse.
public class PathDrawer : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private bool _allow180Turn;

    [Header("References")]
    [SerializeField] private GameObject _straightMarkerPrefab;
    [SerializeField] private GameObject _curvedMarkerPrefab;
    [SerializeField] private GameObject _arrowMarkerPrefab;

    [Header("Debug")]
    [SerializeField, ReadOnly] private List<Vector2Int> _path = new();
    [SerializeField, ReadOnly] private List<Vector2Int> _pathDirection = new();

    [HideInInspector] public bool IsDrawingEnabled;
    private bool _isDrawingPath;
    private Dog _dog;
    private Vector2Int _currentMousePos;
    private Vector2Int _lastMousePos;

    private Vector2Int _pathDir;
    private Vector2Int _lastPathDir;
    private GameObject _lastMarker;

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
        
        _currentMousePos = LevelGrid.Instance.WorldToGridPos(
            _camera.ScreenToWorldPoint(Input.mousePosition)
            );

        if (_currentMousePos != _lastMousePos) GenerateSubPathToMouse();
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
            _lastMousePos, _currentMousePos);
        if (path.Count == 0 || path.Count > 2 || path[0] == new Vector2(1, 0)) return;
        foreach (Vector2Int tile in path)
        {
            _markerColorOffset += 0.08f;
            _path.Add(tile);
            _pathDir = _path.Count > 1 ? (tile - _path[_path.Count - 2]) : tile - _lastMousePos;
            _pathDirection.Add(_pathDir);
            AddMarker(tile, _pathDir, _lastPathDir);
            //_markerManager.AddMarker(0, tile, Color.yellow + new Color(_markerColorOffset, _markerColorOffset, _markerColorOffset));
            _lastPathDir = _pathDir;
        }
        _lastMousePos = _currentMousePos;
    }

    private void StartPath()
    {
        _markerManager.ClearAllMarkers();
        _isDrawingPath = true;
        _lastMousePos = LevelGrid.Instance.WorldToGridPos(transform.position);
        _currentMousePos = _lastMousePos;
        _lastPathDir = Vector2Int.zero;
    }

    private void FinishPath()
    {
        _isDrawingPath = false;
        _dog.CurrentPath = new List<Vector2Int>(_path);
        _path.Clear();
        _markerColorOffset = 0f;
    }

    private void AddMarker(Vector2Int position, Vector2Int pathDir, Vector2Int lastPathDir)
    {
        
        if (_lastMarker != null)
        {
            if (lastPathDir == Vector2Int.zero || lastPathDir == pathDir)
            {
                Instantiate(_straightMarkerPrefab, _lastMarker.transform.position,
                     Quaternion.LookRotation(Vector3.forward, new Vector3(-pathDir.y, pathDir.x, 0f)));
            }
            else
            {
                Instantiate(_curvedMarkerPrefab, _lastMarker.transform.position,
                     Quaternion.LookRotation(Vector3.forward, new Vector3(-lastPathDir.y, lastPathDir.x, 0f)))
                    .GetComponent<SpriteRenderer>().flipY = (pathDir.x * lastPathDir.y - pathDir.y * lastPathDir.x) == 1;
            }

                Destroy(_lastMarker);
        }

        _lastMarker = Instantiate(_arrowMarkerPrefab, LevelGrid.Instance.GridToWorldPos(position),
             Quaternion.LookRotation(Vector3.forward, new Vector3(-pathDir.y, pathDir.x, 0f)));

    }
}
