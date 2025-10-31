using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private Image[] _sFXButtons;
    [SerializeField] private Image[] _musicButtons;

    [SerializeField] private AudioMixerGroup _sFXGroup;
    [SerializeField] private AudioMixerGroup _musicGroup;

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
        OnButtonClick(true, PersistentInfo.Instance.SFXVolume);
        OnButtonClick(false, PersistentInfo.Instance.MusicVolume);
    }

    private void OnButtonClick(bool isSFX, int volumeLevel)
    {
        AudioMixerGroup targetGroup = isSFX ? _sFXGroup : _musicGroup;
        Image[] targetButtons = isSFX ? _sFXButtons : _musicButtons;
        if (isSFX) PersistentInfo.Instance.SFXVolume = volumeLevel;
        else PersistentInfo.Instance.MusicVolume = volumeLevel;

        UpdateImages(targetButtons, volumeLevel);
        targetGroup.audioMixer.SetFloat("Volume", volumeLevel switch
        {
            0 => -1000f,
            1 => -30f,
            2 => -20f,
            3 => -10f,
            4 => 0f,
            _ => throw new System.NotImplementedException()
        });
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
