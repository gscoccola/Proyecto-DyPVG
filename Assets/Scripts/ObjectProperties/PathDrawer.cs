using UnityEngine;
using System.Collections.Generic;

// This component allows the player to draw a path for the Dog on the grid by clicking and dragging the mouse.
public class PathDrawer : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int _maxDistance;

    [Header("References")]
    [SerializeField] private GameObject _straightMarkerPrefab;
    [SerializeField] private GameObject _curvedMarkerPrefab;
    [SerializeField] private GameObject _arrowMarkerPrefab;

    [Header("Debug")]
    [SerializeField, ReadOnly] private List<Vector2Int> _path = new();
    [SerializeField, ReadOnly] private Vector2Int _excludedPos = new Vector2Int(800, 800);

    private List<GameObject> _markers = new();

    [HideInInspector] public bool IsDrawingEnabled;
    private bool _isDrawingPath;
    private Dog _dog;
    private Vector2Int _currentMousePos;
    private Vector2Int _lastMousePos;

    private Vector2Int _pathDir;
    private Vector2Int _lastPathDir;
    private GameObject _lastMarker;
    private Transform _container;

    private Camera _camera;
    private float _markerColorOffset = 0f;

    private void Awake()
    {
        _camera = FindAnyObjectByType<Camera>();
        _dog = GetComponent<Dog>();
        _container = GameObject.Find("Markers").transform;
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
        if (_path.Count >= _maxDistance) return;
        var path = LevelGrid.Instance.CalculatePath(new TileType[] { TileType.Walkable },
            _lastMousePos, _currentMousePos, _excludedPos);
        if (path.Count == 0 || path.Count > 2 || path[0] == new Vector2(1, 0)) return;
        foreach (Vector2Int tile in path)
        {
            _path.Add(tile);
            _pathDir = _path.Count > 1 ? (tile - _path[_path.Count - 2]) : tile - _lastMousePos;
            _markerColorOffset += 1f/ _maxDistance;
            AddMarker(tile, _pathDir, _lastPathDir, Color.yellow + new Color(_markerColorOffset, _markerColorOffset, _markerColorOffset));
            _lastPathDir = _pathDir;
        }
        _excludedPos = _lastMousePos;
        _lastMousePos = _currentMousePos;
    }

    private void StartPath()
    {
        ClearAllMarkers();
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

    private void AddMarker(Vector2Int position, Vector2Int pathDir, Vector2Int lastPathDir, Color color)
    {
        
        if (_lastMarker != null)
        {
            if (lastPathDir == Vector2Int.zero || lastPathDir == pathDir)
            {
                GameObject marker =
                Instantiate(_straightMarkerPrefab, _lastMarker.transform.position,
                     Quaternion.LookRotation(Vector3.forward, new Vector3(-pathDir.y, pathDir.x, 0f)), _container);
                marker.GetComponent<SpriteRenderer>().color = color;
                marker.GetComponent<SpriteRenderer>().sortingOrder = _path.Count;
                _markers.Add(marker);
            }
            else
            {
                GameObject marker =
                Instantiate(_curvedMarkerPrefab, _lastMarker.transform.position,
                     Quaternion.LookRotation(Vector3.forward, new Vector3(-lastPathDir.y, lastPathDir.x, 0f)), _container);

                marker.GetComponent<SpriteRenderer>().flipY = (pathDir.x * lastPathDir.y - pathDir.y * lastPathDir.x) == 1;
                marker.GetComponent<SpriteRenderer>().color = color;
                marker.GetComponent<SpriteRenderer>().sortingOrder = _path.Count;
                _markers.Add(marker);
            }

            _markers.Remove(_lastMarker);
            Destroy(_lastMarker);
        }

        
        _lastMarker = Instantiate(_arrowMarkerPrefab, LevelGrid.Instance.GridToWorldPos(position),
             Quaternion.LookRotation(Vector3.forward, new Vector3(-pathDir.y, pathDir.x, 0f)), _container);
        _lastMarker.GetComponent<SpriteRenderer>().color = color;
        _lastMarker.GetComponent<SpriteRenderer>().sortingOrder = _path.Count;
        _markers.Add(_lastMarker);
    }

    public void ClearAllMarkers()
    {
        foreach (GameObject marker in _markers)
        {
            Destroy(marker);
        }
        _markers.Clear();
    }
}
