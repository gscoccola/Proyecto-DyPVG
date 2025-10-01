using UnityEngine;
using System.Collections.Generic;

// This component allows the player to draw a path on the grid by clicking and dragging the mouse.

public class PathDrawer : MonoBehaviour
{
    [Header("Parameters")]
    public IPathFollower PathFollower;
    public IPathParametersSO PathParameters;

    [Header("Debug")]
    [SerializeField, ReadOnly] private List<Vector2Int> _path = new();

    [Header("References")]
    [SerializeField] private GameObject _straightMarkerPrefab;
    [SerializeField] private GameObject _curvedMarkerPrefab;
    [SerializeField] private GameObject _arrowMarkerPrefab;
    public GameObject ResumePathHitbox;

    private int _maxDistance => PathParameters.MaxDistance;
    private Vector3 _offset => PathParameters.PathOffset;
    private Color _startColor => PathParameters.PathStartColor;
    private Color _endColor => PathParameters.PathEndColor;

    private Vector2Int _excludedPos = new Vector2Int(800, 800);
    private List<GameObject> _markers = new();

    [HideInInspector] public bool IsDrawingEnabled;
    [HideInInspector] public bool IsDrawingPath;

    private Vector2Int _currentMousePos;
    private Vector2Int _lastMousePos;

    private Vector2Int _pathDir;
    private Vector2Int _lastPathDir;
    private GameObject _lastMarker;
    private Transform _container;
    private float _markerColorOffset = 0f;

    #region SETUP
    private void Awake()
    {
        _container = GameObject.Find("Markers").transform;
        GetComponent<Collider2D>().isTrigger = false;
    }
    #endregion

    private void Update()
    {
        if (!IsDrawingPath) return;
        _currentMousePos = LevelGrid.Instance.WorldToGridPos(GetWorldPositionOnPlane(Input.mousePosition));
        if (_currentMousePos != _lastMousePos) GenerateSubPathToMouse();
    }

    public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, 0));
        float distance;
        xy.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    private void OnMouseDown()
    {
        if (!IsDrawingEnabled) { Debug.Log("DISABLED"); return; }
        TurnManager.Instance.SetActiveFollower(PathFollower);
        StartPath();
    }

    private void OnMouseUp()
    {
        if (!IsDrawingPath) return;
        FinishPath();
    }

    private void GenerateSubPathToMouse()
    {
        if (_path.Count >= _maxDistance) return;
        bool excludePoint = _path.Count > 0;
        var path = LevelGrid.Instance.CalculatePath(PathFollower.TraversableTiles,
            _lastMousePos, _currentMousePos, excludePoint, _excludedPos);
        if (path.Count == 0 || path.Count > 2 || path[0] == new Vector2(1, 0)) return;
        foreach (Vector2Int tile in path)
        {
            _path.Add(tile);
            _pathDir = _path.Count > 1 ? (tile - _path[_path.Count - 2]) : tile - _lastMousePos;
            _markerColorOffset += 1f/ _maxDistance;
            AddMarker(tile, _pathDir, _lastPathDir, 
                _startColor * (1f - _markerColorOffset) + _markerColorOffset * _endColor);
            _lastPathDir = _pathDir;
        }
        _excludedPos = _lastMousePos;
        _lastMousePos = _currentMousePos;
        ResumePathHitbox.transform.position = LevelGrid.Instance.GridToWorldPos(_lastMousePos);
    }

    private void StartPath()
    {
        ClearPath();
        _lastPathDir = Vector2Int.zero;
        _lastMousePos = LevelGrid.Instance.WorldToGridPos(transform.position);
        _currentMousePos = _lastMousePos;
        ResumePath();
    }


    public void ResumePath()
    {
        IsDrawingPath = true;
        //TurnManager.Instance.OnNewPathStarted(PathFollower);
    }

    public void FinishPath()
    {
        IsDrawingPath = false;
        PathFollower.SetDrawnPath(new List<Vector2Int>(_path));
    }

    public void RedrawFinishedPath(List<Vector2Int> path)
    {
        ClearPath();
        _lastPathDir = Vector2Int.zero;
        _lastMousePos = LevelGrid.Instance.WorldToGridPos(transform.position);
        if (path.Count == 0) return;
        foreach (Vector2Int tile in path)
        {
            _path.Add(tile);
            _pathDir = _path.Count > 1 ? (tile - _path[_path.Count - 2]) : tile - _lastMousePos;
            _markerColorOffset += 1f / _maxDistance;
            AddMarker(tile, _pathDir, _lastPathDir,
                _startColor * (1f - _markerColorOffset) + _markerColorOffset * _endColor);
            _lastPathDir = _pathDir;
        }
        _excludedPos = _path.Count > 1 ? path[path.Count - 2] : _lastMousePos;
        _lastMousePos = path[path.Count - 1];
        ResumePathHitbox.transform.position = LevelGrid.Instance.GridToWorldPos(path[path.Count -1]);
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

        
        _lastMarker = Instantiate(_arrowMarkerPrefab, LevelGrid.Instance.GridToWorldPos(position) + _offset,
             Quaternion.LookRotation(Vector3.forward, new Vector3(-pathDir.y, pathDir.x, 0f)), _container);
        _lastMarker.GetComponent<SpriteRenderer>().color = color;
        _lastMarker.GetComponent<SpriteRenderer>().sortingOrder = _path.Count;
        _markers.Add(_lastMarker);
    }

    public void ClearPath()
    {
        _path.Clear();
        _lastMarker = null;
        _markerColorOffset = 0f;
        ClearAllMarkers();
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