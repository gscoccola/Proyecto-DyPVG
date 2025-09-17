using UnityEngine;
using UnityEngine.Events;

// This component allows a GameObject to be dragged with the mouse and snapped to the grid.
public class Draggable : MonoBehaviour
{
    public bool IsSelectable = true;
    public bool IsSelected { get; set; }

    //[SerializeField] private bool _centerOnMouse;

    private Camera _camera;
    private Vector2 _cameraBounds;
    private Vector3 _initialRelativePos;
    private Vector3 _initialPos;

    public UnityEvent OnSelected;
    public UnityEvent OnDeSelected;

    private void Awake()
    {
        _camera = FindAnyObjectByType<Camera>();
        _cameraBounds = GetBounds();
        _initialPos = transform.position;
        IsSelectable = true;
    }

    private void Start()
    {
        transform.position = LevelGrid.Instance.SnapToGrid(transform.position);
    }

    private void OnMouseDrag()
    {
        if (!IsSelected || !IsSelectable) return;
        Vector3 newPos = _camera.ScreenToWorldPoint(Input.mousePosition);

        transform.position = new Vector3(
            newPos.x,
            newPos.y,
            transform.position.z
        );
    }

    private void OnMouseUp()
    {
        OnMouseAction(false);
        OnDeSelected?.Invoke();
        transform.position = LevelGrid.Instance.SnapToGrid(transform.position);
    }

    private void OnMouseDown()
    {
        if (!IsSelectable) return;
        OnSelected?.Invoke();
        OnMouseAction(true);
        _initialRelativePos = transform.position - _camera.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseAction(bool mouseDown)
    {
        IsSelected = mouseDown;
    }

    public Vector2 GetBounds()
    {
        return new Vector2(
            _camera.orthographicSize * _camera.aspect,
            _camera.orthographicSize
        );
    }
}

public enum LimitType
{
    None,
    Min,
    Max,
}
