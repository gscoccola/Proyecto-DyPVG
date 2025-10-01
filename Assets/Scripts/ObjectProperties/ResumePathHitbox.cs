using UnityEngine;

// This monobehaviour is attached to the path arrow tip, used to resume drawing a path. 

public class ResumePathHitbox : MonoBehaviour
{
    private PathDrawer _pathDrawer;

    private void Awake()
    {
        _pathDrawer = transform.parent.GetComponent<PathDrawer>();
    }

    private void OnMouseDown()
    {
        if (!_pathDrawer.IsDrawingEnabled) return;
        _pathDrawer.ResumePath();
    }

    private void OnMouseUp()
    {
        if (!_pathDrawer.IsDrawingPath) return;
        _pathDrawer.FinishPath();
    }

}
