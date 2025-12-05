using UnityEngine;
public class DirectionChanger : MonoBehaviour
{

    private SpriteRenderer _spriteRenderer;
    private SplashVFXManager _splashVFX;

    private bool _isFlipped;

    private void Awake()
    {
        //_spriteRenderer = GetComponent<SpriteRenderer>();
        transform.parent.GetComponent<GridMovement>().ChangedDirection.AddListener(SetDirection);
        _splashVFX = transform.parent.GetComponent<SplashVFXManager>();
    }

    private void SetDirection(Vector2Int direction)
    {
        bool flipped = direction == Vector2Int.left || direction == Vector2Int.up;
        if (_isFlipped == flipped) return;

        _isFlipped = flipped;
        if (flipped) transform.localScale = new Vector3(-1f, 1f, 1f);
        else transform.localScale = Vector3.one;

        _splashVFX.Switch();
    }

}
