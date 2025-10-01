using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CanvasManager : Singleton<CanvasManager>
{
    [Header("References")]
    public TextMeshProUGUI TurnText;
    public TextMeshProUGUI ActionButtonText;
    public Button ActionButton;
    public GameObject DogIconPrefab;

    #region SETUP

    private void Start()
    {
        foreach (Dog dog in LevelManager.Instance.DogList)
        {
            
        }
    }
    #endregion

    #region ON BUTTON PRESS

    public void OnActionButtonPress()
    {
        TurnManager.Instance.OnActionButtonPress();
    }

    public void RevertToPreviousTurn()
    {
        TurnManager.Instance.RevertToPreviousTurn();
    }

    public void RevertToNextTurn()
    {
        TurnManager.Instance.RevertToNextTurn();
    }

    public void ReloadLevel()
    {
        TurnManager.Instance.ReloadLevel();
    }
    #endregion

    #region OTHER PUBLIC METHODS

    public void SetActionButton(bool interactable)
    {
        ActionButton.interactable = interactable;
    }

    public void UpdateDisplayedValues(bool isPlanningPhase)
    {
        ActionButtonText.text = isPlanningPhase ? "Start" : "Stop";
        TurnText.text = $"Turn {TurnManager.Instance.CurrentTurnIndex + 1}";
    }
    #endregion
}
