using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class DogPen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnitTrigger[] _triggers;

    [Header("Debug")]

    [SerializeField, ReadOnly] private int _dogLeftOutside;

    private void Awake()
    {
        foreach (UnitTrigger trigger in _triggers)
        {
            trigger.Triggered.AddListener(value => OnDogEntered());
            trigger.Untriggered.AddListener(value => OnDogExit());
        }
        
    }

    private void Start()
    {
        TurnManager.Instance.TurnEnd.AddListener(CheckForWin);
        foreach (Dog dog in LevelManager.Instance.DogList)
        {
            _dogLeftOutside++;
        }
    }

    private void OnDogEntered()
    {
        _dogLeftOutside--;
        //Debug.Log(_dogLeftOutside);
    }

    private void OnDogExit()
    {
        _dogLeftOutside++;
        //Debug.Log(_dogLeftOutside);
    }

    private void CheckForWin()
    {
        if (_dogLeftOutside <= 0)
        {
            StartCoroutine(TriggerWinCondition());
        }
    }

    private IEnumerator TriggerWinCondition()
    {
        // if current turn index is less than the stored score for this level, or score is zero, update it
        int previousScore = PersistentInfo.Instance.CompletionInfo[SceneManager.GetActiveScene().buildIndex - 1].Score;
        if (TurnManager.Instance.CurrentTurnIndex < previousScore ||
            previousScore == 0)
            PersistentInfo.Instance.CompletionInfo[SceneManager.GetActiveScene().buildIndex - 1].Score = TurnManager.Instance.CurrentTurnIndex;
        PersistentInfo.Instance.Save();
        TurnManager.Instance.CurrentTurnIndex--;
        yield return new WaitForSecondsRealtime(1f);
        CanvasManager.Instance.ShowWinPanel();
        // Implement additional win condition logic here, such as notifying a GameManager.
    }

}
