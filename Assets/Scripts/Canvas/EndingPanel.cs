using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndingPanel : MonoBehaviour
{
    [SerializeField] private GameObject _incompleteEndingBackground;
    [SerializeField] private GameObject _trueEndingBackground;
    [SerializeField] private TextMeshProUGUI _endingText;
    private void OnEnable()
    {
        int totalChallenges = PersistentInfo.Instance.TotalChallengesCompleted();
        if (totalChallenges == 10)
        {
            _trueEndingBackground.SetActive(true);
        }
        else
        {
            _endingText.text = totalChallenges.ToString();
            _incompleteEndingBackground.SetActive(true);
        }
    }
}
