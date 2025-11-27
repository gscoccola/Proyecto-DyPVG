using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DogChip : MonoBehaviour, IDragHandler, IBeginDragHandler,
    IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private Image _image;
    private bool _isDragging;

    [Header("Debug")]
    [ReadOnly] public int Order;
    [ReadOnly] public IActionable Actionable;

    [HideInInspector] public Sprite[] Sprites;
    [HideInInspector] public UnityEvent BeginDrag;
    [HideInInspector] public UnityEvent EndDrag;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _image = GetComponent<Image>();
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (TurnManager.Instance.CurrentState == GameState.Action) return;
        _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        CanvasManager.Instance.OnChipDrag(this, Order);
        //_selfCanvas.sortingOrder = 10;
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        CanvasManager.Instance.OnChipDrop(this, Order);
        _image.sprite = Sprites[0];
        _isDragging = false;
        EndDrag.Invoke();
        VibrationHandler.Instance.LightVibrate();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetAsLastSibling();
        _image.sprite = Sprites[2];
        _isDragging = true;
        BeginDrag.Invoke();
        VibrationHandler.Instance.LightVibrate();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isDragging) return;
        _image.sprite = Sprites[1];
        CanvasManager.Instance.PlayChipSound();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isDragging) return;
        _image.sprite = Sprites[0];
    }
}