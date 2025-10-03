using UnityEngine;
using UnityEngine.EventSystems;

public class DogChip : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private RectTransform _rectTransform;
    private Canvas _canvas;

    [Header("Debug")]
    [ReadOnly] public int Order;
    [ReadOnly] public IActionable Actionable;


    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = FindAnyObjectByType<Canvas>();
    }


    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        CanvasManager.Instance.OnChipDrop(this, Order);
    }

}