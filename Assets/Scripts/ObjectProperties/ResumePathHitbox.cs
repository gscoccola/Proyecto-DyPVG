using UnityEngine;
using UnityEngine.EventSystems;


// This monobehaviour is attached to the path arrow tip, used to resume drawing a path. 

public class ResumePathHitbox : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private PathDrawer _pathDrawer;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_pathDrawer.IsDrawingEnabled) return;
        _pathDrawer.ResumePath();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_pathDrawer.IsDrawingPath) return;
        _pathDrawer.FinishPath();
    }

    private void Awake()
    {
        _pathDrawer = transform.parent.GetComponent<PathDrawer>();
    }
}
