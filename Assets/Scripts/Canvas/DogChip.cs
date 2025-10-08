using UnityEngine;
using UnityEngine.EventSystems;

public class DogChip : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private Canvas _selfCanvas;

    [Header("Debug")]
    [ReadOnly] public int Order;
    [ReadOnly] public IActionable Actionable;


    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        //_selfCanvas = GetComponent<Canvas>();
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (TurnManager.Instance.CurrentState == GameState.Action) return;
        _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        //_selfCanvas.sortingOrder = 10;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        CanvasManager.Instance.OnChipDrop(this, Order);
        //_selfCanvas.sortingOrder = 0;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }
}