using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject _popup;

    private void Awake()
    {
        _popup = transform.GetChild(0).gameObject;
        _popup.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _popup.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _popup.SetActive(false);
    }
}
