using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class DogPen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnitTrigger[] _triggers;

    private int _dogLeftOutside;
    private Dictionary<Dog, bool> _dogsInPen = new();

    private void Awake()
    {
        foreach (UnitTrigger trigger in _triggers)
        {
            trigger.Triggered.AddListener(OnDogEntered);
            trigger.Untriggered.AddListener(OnDogExit);
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
        yield return new WaitForEndOfFrame();
        if (_dogLeftOutside > 0) yield break;
        CanvasManager.Instance.ShowWinPanel();
        // Implement additional win condition logic here, such as notifying a GameManager.
    }
}
