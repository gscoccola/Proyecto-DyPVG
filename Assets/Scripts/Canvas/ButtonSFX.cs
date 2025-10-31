using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSFX : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CanvasManager.Instance != null)
            CanvasManager.Instance.OnButtonHover();
        else if (MainMenu.Instance != null)
            MainMenu.Instance.OnButtonHover();
    }


}
