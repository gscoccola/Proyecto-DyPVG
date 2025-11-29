using UnityEngine;

public class Button : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Door _door;
    [SerializeField] private Sprite _buttonUnpressedSprite;
    [SerializeField] private Sprite _buttonPressedSprite;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private UnitTrigger _trigger;


    private void Awake()
    {
        _trigger = GetComponent<UnitTrigger>();
    }

    private void Start()
    {
        _trigger.Triggered.AddListener(playSFX => Toggle(true, playSFX));
        _trigger.Untriggered.AddListener(playSFX => Toggle(false, playSFX));
    }

    private void Toggle(bool pressed, bool playSound = true)
    {
        if (_spriteRenderer == null) return;
        if (_buttonUnpressedSprite == null || _buttonPressedSprite == null)
        {
            _spriteRenderer.enabled = !pressed;
            return;
        }
        _spriteRenderer.sprite = pressed ? _buttonPressedSprite : _buttonUnpressedSprite;
    }
}
