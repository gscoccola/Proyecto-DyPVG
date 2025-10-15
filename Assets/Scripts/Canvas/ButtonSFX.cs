using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSFX : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        CanvasManager.Instance.OnButtonHover();
    }


}
