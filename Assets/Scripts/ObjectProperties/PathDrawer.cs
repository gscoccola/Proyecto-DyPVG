using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// This component allows the player to draw a path on the grid by clicking and dragging the mouse.

public class PathDrawer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Parameters")]
    public IPathFollower PathFollower;
    public IPathParametersSO PathParameters;
    private float _magnetism = 0.2f;

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
    public List<GameObject> _markers = new();

    [HideInInspector] public bool IsDrawingEnabled;
    [HideInInspector] public bool IsDrawingPath;

    private Vector2Int _correctedPointerPos;
    private Vector2Int _lastMousePos;

    private Vector2Int _pathDir;
    private Vector2Int _lastPathDir;
    private GameObject _lastMarker;
    private Transform _container;
    private float _markerColorOffset = 0f;

    private bool _unpreciseMode = false;

    private PlayerInput _playerInput;
    //private InputAction TouchPressAction;
    private InputAction TouchPositionAction;

    #region SETUP
    private void Awake()
    {
        _container = GameObject.Find("Markers").transform;
        GetComponent<Collider2D>().isTrigger = false;
        _playerInput = FindAnyObjectByType<PlayerInput>();

        //TouchPressAction = _playerInput.actions["TouchPress"];
        TouchPositionAction = _playerInput.actions["TouchPosition"];

        _unpreciseMode = Application.isMobilePlatform;
    }
    #endregion

    private void Update()
    {
        Vector2 pointerPos;
        if (Application.isMobilePlatform) pointerPos = TouchPositionAction.ReadValue<Vector2>();
        else pointerPos = Mouse.current.position.ReadValue();

        if (!IsDrawingPath) return;
        Vector3 pointerPosOnPlane = GetWorldPositionOnPlane(pointerPos);

        _correctedPointerPos = _unpreciseMode
            ? LevelGrid.Instance.PredictiveWorldToGridPos(pointerPosOnPlane, _lastMousePos, _magnetism * 2f, _pathDir)
            : LevelGrid.Instance.PredictiveWorldToGridPos(pointerPosOnPlane, _lastMousePos, _magnetism * 2f, _pathDir);
            //: LevelGrid.Instance.CorrectedWorldToGridPos(pointerPosOnPlane, _lastMousePos, _magnetism);
        if (_correctedPointerPos != _lastMousePos) GenerateSubPathToMouse();
    }

    #region HANDLE POINTER
    public Vector3 GetWorldPositionOnPlane(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, 0));
        float distance;
        xy.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if ( (TurnManager.Instance.TurnsLeft == 0))
        {
            Debug.Log("NO TURNS");
        }
        if (!IsDrawingEnabled || TurnManager.Instance.CurrentState == GameState.Action|| TurnManager.Instance.TurnsLeft == 0)  
        { Debug.Log("DISABLED"); VibrationHandler.Instance.LightVibrate(); return; }
        VibrationHandler.Instance.MediumVibrate();
        StartPath();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsDrawingPath) return;
        FinishPath();
    }
    #endregion

    private void GenerateSubPathToMouse()
    {
        
        if (_correctedPointerPos == _excludedPos)
        {
            ReducePath();
        }
        else
        {
            if (_path.Count < _maxDistance) ExtendPath();
            //else SFXPlayer.Instance.PlayClip(WorldSounds.Instance.PathEnd);
        }
        
        
            
    }

    private void ExtendPath()
    {
        bool excludePoint = _path.Count > 1;
        var path = LevelGrid.Instance.CalculatePath(PathFollower.TraversableTiles,
                _lastMousePos, _correctedPointerPos, excludePoint, _excludedPos);
        if (path.Count == 0 || path.Count > 2/* || path[0] == new Vector2(1, 0)*/) return;
        if (_path.Count == _maxDistance -1) SFXPlayer.Instance.PlayClip(WorldSounds.Instance.PathEnd);
        foreach (Vector2Int tile in path)
        {
            _path.Add(tile);
            _pathDir = _path.Count > 1 ? (tile - _path[_path.Count - 2]) : tile - _lastMousePos;
            _markerColorOffset += 1f / _maxDistance;
            AddMarker(tile, _pathDir, _lastPathDir,
                _startColor * (1f - _markerColorOffset) + _markerColorOffset * _endColor);
            _lastPathDir = _pathDir;
        }
        _excludedPos = path.Count > 1 ? path[path.Count - 2] : _lastMousePos;
        _lastMousePos = _correctedPointerPos;
        ResumePathHitbox.transform.position = LevelGrid.Instance.GridToWorldPos(_lastMousePos);
    }

    private void ReducePath()
    {
        if (_path.Count < 2) return; 
        _path.RemoveAt(_path.Count - 1);
        _pathDir = _path.Count > 1 ? (_path[_path.Count - 1] - _path[_path.Count - 2]) : Vector2Int.zero;
        _markerColorOffset -= 1f / _maxDistance;
        RemoveMaker(_excludedPos, _pathDir,
            _startColor * (1f - _markerColorOffset) + _markerColorOffset * _endColor);

        //_lastPathDir = _path.Count > 2 ? (_path[_path.Count - 2] - _path[_path.Count - 3]) : Vector2Int.zero; ;
        //_lastPathDir = _path.Count > 1 ? (_path[_path.Count - 1] - _path[_path.Count - 2]) : Vector2Int.zero; ;
        _lastPathDir = _pathDir;
        _excludedPos = _path.Count > 1 ? _path[_path.Count - 2] : LevelGrid.Instance.WorldToGridPos(transform.position);
        _lastMousePos = _correctedPointerPos;
        ResumePathHitbox.transform.position = LevelGrid.Instance.GridToWorldPos(_correctedPointerPos);
        return;
    }

    private void StartPath()
    {
        SFXPlayer.Instance.PlayClip(PathParameters.SelectedSFX);
        ClearPath();
        _path.Add(LevelGrid.Instance.WorldToGridPos(transform.position));
        _lastPathDir = Vector2Int.zero;
        _lastMousePos = LevelGrid.Instance.WorldToGridPos(transform.position);
        _correctedPointerPos = _lastMousePos;
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
        if (path.Count == 0 || path.Count == 1) return;
        for (int i = 0; i < path.Count; i++)
        {
            _path.Add(path[i]);
            _pathDir = _path.Count > 1 ? (path[i] - _path[_path.Count - 2]) : path[i] - _lastMousePos;
            _markerColorOffset += 1f / _maxDistance;
            if (i != 0) AddMarker(path[i], _pathDir, _lastPathDir,
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
            // add straight or curved marker where arrow was
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

            // remove previous arrow
            _markers.RemoveAt(_markers.Count - 2);
            Destroy(_lastMarker);
        }

        // add arrow on the end
        _lastMarker = Instantiate(_arrowMarkerPrefab, LevelGrid.Instance.GridToWorldPos(position) + _offset,
             Quaternion.LookRotation(Vector3.forward, new Vector3(-pathDir.y, pathDir.x, 0f)), _container);
        _lastMarker.GetComponent<SpriteRenderer>().color = color;
        _lastMarker.GetComponent<SpriteRenderer>().sortingOrder = _path.Count;
        _markers.Add(_lastMarker);
    }

    private void RemoveMaker(Vector2Int position, Vector2Int pathDir, Color color)
    {
        // remove previous arrow
        _markers.RemoveAt(_markers.Count -1);
        Destroy(_lastMarker);

        // return if done
        if (_markers.Count == 0) return;

        // else remove straight or curved marker
        Destroy(_markers[_markers.Count - 1]);
        _markers.RemoveAt(_markers.Count - 1);
        //_lastMarker = _markers[_markers.Count - 1];

        // and replace with arrow on the end
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
        _excludedPos = new Vector2Int(800, 800);
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