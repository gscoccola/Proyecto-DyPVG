using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeMenu : MonoBehaviour
{
    [SerializeField] private Image[] _sFXButtons;
    [SerializeField] private Image[] _musicButtons;

    [SerializeField] private Sprite _disabledSprite;
    [SerializeField] private Sprite _enabledSprite;
    [SerializeField] private Sprite _selectedSprite;

    private void Start()
    {
        for (int i = 0; i < _sFXButtons.Length; i++)
        {
            int j = i;
            _sFXButtons[j].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnButtonClick(true, j));
            _musicButtons[j].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnButtonClick(false, j));
        }
        UpdateImages(_sFXButtons, PersistentInfo.Instance.SFXVolume);
        UpdateImages(_musicButtons, PersistentInfo.Instance.MusicVolume);
    }

    private void OnButtonClick(bool isSFX, int volumeLevel)
    {
        Image[] targetButtons = isSFX ? _sFXButtons : _musicButtons;
        if (isSFX) PersistentInfo.Instance.SFXVolume = volumeLevel;
        else PersistentInfo.Instance.MusicVolume = volumeLevel;

        UpdateImages(targetButtons, volumeLevel);
        VolumeControl.Instance.SetVolume(isSFX, volumeLevel);
    }

    private void UpdateImages(Image[] images, int volumeLevel)
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (i < volumeLevel)
            {
                images[i].sprite = _enabledSprite;
            }
            else if (i == volumeLevel)
            {
                images[i].sprite = _selectedSprite;
            }
            else
            {
                images[i].sprite = _disabledSprite;
            }
        }
    }
}
