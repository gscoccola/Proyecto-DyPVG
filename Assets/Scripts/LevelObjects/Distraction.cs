using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Distraction : MonoBehaviour, IGridCollider, IRevertable
{
    [Header("Debug")]
    [ReadOnly] public bool IsDisabled;
    [HideInInspector] public UnityEvent OnDistractionDisabled;
    [HideInInspector] public List<bool> OccupiedHistory = new();
    [SerializeField] private GameObject _eatSFX;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        OccupiedHistory.Add(false);
    }

    #region GRID COLLIDER INTERFACE

    public void OnGridCollisionEnter(Transform other)
    {
        if (IsDisabled) return;
        if (other.GetComponent<Dog>() == null || other.GetComponent<Dog>().DogParameters.Type != DogType.Bully) return;
        Toggle(true);
        OnDistractionDisabled?.Invoke();
        Instantiate(_eatSFX, transform.position, Quaternion.identity);
        SFXPlayer.Instance.PlayClip(WorldSounds.Instance.BDogTrash, 1f, true);
    }

    public void OnGridCollisionExit(Transform other)
    {

    }
    #endregion

    #region REVERTABLE INTERFACE

    public void RevertToHistoryPoint(int turnIndex)
    {
        Toggle(OccupiedHistory[turnIndex]);
    }

    public void SaveHistoryPoint(int turnIndex, bool deleteFuturePoints = true)
    {
        if (deleteFuturePoints)
        {
            while (OccupiedHistory.Count > turnIndex)
                OccupiedHistory.RemoveAt(OccupiedHistory.Count - 1);
        }
        if (OccupiedHistory.Count == turnIndex)
            OccupiedHistory.Add(IsDisabled);
        else
            OccupiedHistory[turnIndex] = IsDisabled;
    }
    #endregion

    public void Toggle(bool disabled)
    {
        _spriteRenderer.enabled = !disabled;
        IsDisabled = disabled;
    }
}
