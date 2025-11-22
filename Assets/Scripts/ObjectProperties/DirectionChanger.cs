using UnityEngine;

public class DirectionChanger : MonoBehaviour
{

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        transform.parent.GetComponent<GridMovement>().ChangedDirection.AddListener(ChangeDirection);
    }

    private void ChangeDirection(Vector2Int direction)
    {
        _spriteRenderer.flipX = direction == Vector2Int.left || direction == Vector2Int.up;
    }
}
