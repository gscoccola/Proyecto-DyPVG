using UnityEngine;

// This interface is implemented by objects that need to respond to grid-based collision events.

public interface IGridCollider
{
    public void OnGridCollisionEnter(Transform other);

    public void OnGridCollisionExit(Transform other);
}
