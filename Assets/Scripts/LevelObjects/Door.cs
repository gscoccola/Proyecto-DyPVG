using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Door : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Sprite _openSprite;
    [SerializeField] private Sprite _closedSprite;

    [SerializeField] private UnitTrigger _trigger;

    [Header("Debug")]
    [ReadOnly] public bool IsDisabled;
    [HideInInspector] public UnityEvent OnDoorOpen;
    [HideInInspector] public List<bool> OpenHistory = new();

    private SpriteRenderer _spriteRenderer;
    private Blocking _blocking;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _blocking = GetComponent<Blocking>();
    }

    private void Start()
    {
        _trigger.Triggered.AddListener(playSFX => Toggle(true, playSFX));
        _trigger.Untriggered.AddListener(playSFX => Toggle(false, playSFX));
    }


    public void Toggle(bool disabled, bool playSound = true)
    {
        _blocking.IsEnabled = !disabled;
        _spriteRenderer.sprite = !disabled ? _closedSprite: _openSprite;
        //Vector2Int blockedPos = LevelGrid.Instance.WorldToGridPos(_blocking.transform.position);
        IsDisabled = disabled;
        if (!playSound) return;
        if (disabled) SFXPlayer.Instance.PlayClip(WorldSounds.Instance.DoorOpen);
        else SFXPlayer.Instance.PlayClip(WorldSounds.Instance.DoorClose);
    }
}
