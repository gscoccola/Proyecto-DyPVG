using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Distraction : MonoBehaviour, IGridCollider, IRevertable
{
    [Header("Debug")]
    [ReadOnly] public bool IsDisabled;
    [HideInInspector] public UnityEvent OnDistractionDisabled;
    [HideInInspector] public List<DistractionData> History = new();
    [SerializeField] private GameObject _eatSFX;
    public Dog DistractedDog;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        History.Add(new DistractionData(null, false));
    }

    #region GRID COLLIDER INTERFACE

    public void OnGridCollisionEnter(Transform other)
    {
        if (IsDisabled) return;
        if (other.GetComponent<Dog>() == null || other.GetComponent<Dog>().DogParameters.Type != DogType.Bully) return;
        Toggle(true);
        if (DistractedDog != null) DistractedDog.SetDistractedVFX(false);
        DistractedDog = null;
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
        Toggle(History[turnIndex].IsDisabled);
        DistractedDog = History[turnIndex].DistractedDog;
    }

    public void SaveHistoryPoint(int turnIndex, bool deleteFuturePoints = true)
    {
        if (deleteFuturePoints)
        {
            while (History.Count > turnIndex)
                History.RemoveAt(History.Count - 1);
        }
        if (History.Count == turnIndex)
            History.Add(new DistractionData(DistractedDog, IsDisabled));
        else
            History[turnIndex] = new DistractionData(DistractedDog, IsDisabled);
    }
    #endregion

    public void Toggle(bool disabled)
    {
        _spriteRenderer.enabled = !disabled;
        IsDisabled = disabled;
    }


}

public class DistractionData
{
    public bool IsDisabled;
    public Dog DistractedDog;

    public DistractionData(Dog distractedDog, bool isDisabled)
    {
        DistractedDog = distractedDog;
        IsDisabled = isDisabled;
    }
}