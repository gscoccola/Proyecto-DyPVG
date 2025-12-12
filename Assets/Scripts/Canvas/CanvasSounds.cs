using UnityEngine;

public class CanvasSounds : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] public AudioClip NextTurn;
    [SerializeField] public AudioClip PreviousTurn;
    [SerializeField] public AudioClip[] Go;
    [SerializeField] public AudioClip ButtonHover;
    [SerializeField] public AudioClip Restart;
    [SerializeField] public AudioClip AcceptOrCancel;
    [SerializeField] public AudioClip[] ChipHover;

    [Header("Victory")]
    [SerializeField] public AudioClip VictoryBanner;
    [SerializeField] public AudioClip VictoryBannerSlide;
    [SerializeField] public AudioClip ChallengePanel;
    [SerializeField] public AudioClip GoodJob;
    [SerializeField] public AudioClip FinalButtons;
}
