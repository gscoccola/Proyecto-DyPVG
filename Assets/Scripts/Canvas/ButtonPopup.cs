using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool _popupOnDisabled;
    private GameObject _popup;
    private UnityEngine.UI.Button _button;

    private void Awake()
    {
        _button = GetComponent<UnityEngine.UI.Button>();
        _popup = transform.GetChild(0).gameObject;
        _popup.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_button.interactable && _popupOnDisabled) return;
        _popup.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_button.interactable && _popupOnDisabled) return;
        _popup.SetActive(false);
    }
}
