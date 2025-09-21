using UnityEngine;
using System.Collections.Generic;

// This component allows the player to draw a path for the Dog on the grid by clicking and dragging the mouse.
public class PathDrawer : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private DogSO DogParameters;

    private int _maxDistance => DogParameters.MaxDistance;
    private Vector3 _offset => DogParameters.PathOffset;
    private Color _startColor => DogParameters.PathStartColor;
    private Color _endColor => DogParameters.PathEndColor;

    [Header("References")]
    [SerializeField] private GameObject _straightMarkerPrefab;
    [SerializeField] private GameObject _curvedMarkerPrefab;
    [SerializeField] private GameObject _arrowMarkerPrefab;
    [SerializeField] private GameObject _resumePathHitbox;

    [Header("Debug")]
    [SerializeField, ReadOnly] private List<Vector2Int> _path = new();
    [SerializeField, ReadOnly] private Vector2Int _excludedPos = new Vector2Int(800, 800);

    private List<GameObject> _markers = new();

    [HideInInspector] public bool IsDrawingEnabled;
    [HideInInspector] public bool IsDrawingPath;
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
        if (!IsDrawingPath) return;
        _currentMousePos = LevelGrid.Instance.WorldToGridPos(GetWorldPositionOnPlane(Input.mousePosition));
        /*_currentMousePos = LevelGrid.Instance.WorldToGridPos(
            _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10))
            );*/

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
        if (!IsDrawingEnabled) return;
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
        var path = LevelGrid.Instance.CalculatePath(_dog.TraversableTiles,
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
        _resumePathHitbox.transform.position = LevelGrid.Instance.GridToWorldPos(_lastMousePos);
    }

    private void StartPath()
    {
        _path.Clear();
        _markerColorOffset = 0f;
        ClearAllMarkers();
        _lastPathDir = Vector2Int.zero;
        _lastMousePos = LevelGrid.Instance.WorldToGridPos(transform.position);
        _currentMousePos = _lastMousePos;
        ResumePath();
    }

    public void ResumePath()
    {
        IsDrawingPath = true;
    }

    public void FinishPath()
    {
        IsDrawingPath = false;
        _dog.DrawnPath = new List<Vector2Int>(_path);
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

    public void ClearAllMarkers()
    {
        foreach (GameObject marker in _markers)
        {
            Destroy(marker);
        }
        _markers.Clear();
    }
}
