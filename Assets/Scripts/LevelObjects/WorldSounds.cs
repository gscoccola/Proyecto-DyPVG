using UnityEngine;

public class WorldSounds : Singleton<WorldSounds>
{
    [Header("Dogs")]
    [SerializeField] public AudioClip SDogSelect;
    [SerializeField] public AudioClip SDogStart;

    [SerializeField] public AudioClip SDogTrash;
    [SerializeField] public AudioClip SDogCrawl;


    [SerializeField] public AudioClip BDogStart;
    [SerializeField] public AudioClip BDogSelect;

    [SerializeField] public AudioClip BDogTrash;
    [SerializeField] public AudioClip BDogCrawl;

    [SerializeField] public AudioClip[] DogFootSteps;

    [Header("Door")]
    [SerializeField] public AudioClip DoorOpen;
    [SerializeField] public AudioClip DoorInterrupt;



}
