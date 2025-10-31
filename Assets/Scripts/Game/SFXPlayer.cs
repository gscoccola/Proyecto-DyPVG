using UnityEngine;
using System.Collections.Generic;

public class SFXPlayer : Singleton<SFXPlayer>
{
    private List<GameObject> _interruptableSounds = new();

    public void PlayClip(AudioClip[] clips)
    {
        PlayClip(clips[Random.Range(0, clips.Length)]);
    }

    public AudioSource PlayClip(AudioClip clip, float volume = 1, bool interruptable = false, bool randomizePitch = false)
    {
        GameObject tempGO = new GameObject("TempAudio"); // create the temp object
        AudioSource tempASource = tempGO.AddComponent<AudioSource>(); // add an audio source
        tempASource.clip = clip;
        tempASource.volume = volume;
        tempASource.rolloffMode = AudioRolloffMode.Linear;
        tempASource.maxDistance = 200f;
        if (randomizePitch) tempASource.pitch = Random.Range(0.95f, 1.05f);
        tempASource.Play(); // start the sound
        if (interruptable) {_interruptableSounds.Add(tempGO);}
        MonoBehaviour.Destroy(tempGO, tempASource.clip.length); // destroy object after clip duration
        return tempASource; // return the AudioSource reference
    }

    public void InterruptSounds()
    {
        foreach (var sound in _interruptableSounds) if (sound != null) Destroy(sound);
        _interruptableSounds.Clear();
    }
}
