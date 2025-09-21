using UnityEngine;

public interface IGridCollider
{
    public void OnGridCollisionEnter(Transform other);

    public void OnGridCollisionExit(Transform other);
}
