using UnityEngine;

public class SplashVFXManager : MonoBehaviour
{
    [SerializeField] private GameObject _frontSplash;
    [SerializeField] private GameObject _backSplash;
    public bool IsBackSplash;

    public void ToggleVFX(bool enabled, Vector2Int direction)
    {
        if (!enabled)
        {
            _frontSplash.SetActive(false);
            _backSplash.SetActive(false);
            return;
        }


        if (direction.x == -1f)
        {
            IsBackSplash = false;
            if (_frontSplash.transform.parent.localScale.x == -1f) _backSplash.SetActive(true);
            else _frontSplash.SetActive(true);
        }
        else
        {
            IsBackSplash = true;
            _frontSplash.SetActive(true);
        }

    }
        
    public void Switch()
    {
        if (IsBackSplash) return;
        bool frontActive = _frontSplash.activeSelf;
        _frontSplash.SetActive(_backSplash.activeSelf);
        _backSplash.SetActive(frontActive);
    }

}
