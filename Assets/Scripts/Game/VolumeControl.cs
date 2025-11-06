using UnityEngine;
using UnityEngine.Audio;

public class VolumeControl : PersistentSingleton<VolumeControl>
{
    [SerializeField] private AudioMixerGroup _sFXGroup;
    [SerializeField] private AudioMixerGroup _musicGroup;

    public void SetVolume(bool isSFX, int volumeLevel)
    {
        AudioMixerGroup targetGroup = isSFX ? _sFXGroup : _musicGroup;
        if (isSFX) PersistentInfo.Instance.SFXVolume = volumeLevel;
        else PersistentInfo.Instance.MusicVolume = volumeLevel;

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


}
