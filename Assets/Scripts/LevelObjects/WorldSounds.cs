using UnityEngine;

public class WorldSounds : Singleton<WorldSounds>
{
    [Header("Dogs")]

    [SerializeField] public AudioClip SDogTrash;
    [SerializeField] public AudioClip SDogCrawl;

    [SerializeField] public AudioClip BDogTrash;

    [SerializeField] public AudioClip[] DogFootSteps;

    [Header("Door")]
    [SerializeField] public AudioClip MetalDoorOpen;
    [SerializeField] public AudioClip MetalDoorInterrupt;
    [SerializeField] public AudioClip WoodDoorOpen;
    [SerializeField] public AudioClip WoodDoorInterrupt;

    [Header("Water")]
    [SerializeField] public AudioClip[] WaterCross;
    [SerializeField] public AudioClip WaterMaintain;

    [Header("Misc")]
    [SerializeField] public AudioClip PathEnd;

    [Header("Win")]
    [SerializeField] public AudioClip WinWithChallenge;
    [SerializeField] public AudioClip WinNoChallenge;
    [SerializeField] public AudioClip Stamp;
}
