using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;

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
        PersistentInfo.Instance.LevelScores[UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex - 1] = TurnManager.Instance.CurrentTurnIndex;
        PersistentInfo.Instance.Save();
        TurnManager.Instance.CurrentTurnIndex--;
        yield return new WaitForSecondsRealtime(1f);
        CanvasManager.Instance.ShowWinPanel();
        // Implement additional win condition logic here, such as notifying a GameManager.
    }

}
