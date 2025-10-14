using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Door : MonoBehaviour, IGridCollider, IRevertable
{
    [Header("References")]
    [SerializeField] private Blocking _blocking;
    [SerializeField] private Sprite _openSprite;
    [SerializeField] private Sprite _closedSprite;

    [Header("Debug")]
    [ReadOnly] public bool IsDisabled;
    [HideInInspector] public UnityEvent OnDoorOpen;
    [HideInInspector] public List<bool> OpenHistory = new();

    //private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        //_spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        OpenHistory.Add(false);
    }

    #region GRID COLLIDER INTERFACE

    public void OnGridCollisionEnter(Transform other)
    {
        if (other.GetComponent<Dog>() == null) return;
        Toggle(true);
        OnDoorOpen?.Invoke();
    }

    public void OnGridCollisionExit(Transform other)
    {

    }
    #endregion

    #region REVERTABLE INTERFACE

    public void RevertToHistoryPoint(int turnIndex)
    {
        Toggle(OpenHistory[turnIndex]);
    }

    public void SaveHistoryPoint(int turnIndex, bool deleteFuturePoints = true)
    {
        if (deleteFuturePoints)
        {
            while (OpenHistory.Count > turnIndex)
                OpenHistory.RemoveAt(OpenHistory.Count - 1);
        }
        if (OpenHistory.Count == turnIndex)
            OpenHistory.Add(IsDisabled);
        else
            OpenHistory[turnIndex] = IsDisabled;
    }
    #endregion

    public void Toggle(bool disabled)
    {
        _blocking.IsEnabled = !disabled;
        _blocking.Renderer.sprite = !disabled ? _closedSprite: _openSprite;
        Vector2Int blockedPos = LevelGrid.Instance.WorldToGridPos(_blocking.transform.position);
        //_spriteRenderer.enabled = !disabled;

        IsDisabled = disabled;
    }
}
