using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CanvasManager : Singleton<CanvasManager>
{
    [Header("Parameters")]
    public List<GameObject> ChipPlaces = new();
    [SerializeField] private float _chipDropDistance = 120f;

    [Header("References")]
    public TextMeshProUGUI TurnText;
    public UnityEngine.UI.Button ActionButton;
    public GameObject ChipsHolder;
    public GameObject WinPanel;
    public GameObject ResetPanel;
    public GameObject TutorialPanel;

    public GameObject DogChipPrefab;
    public GameObject ChipPlacePrefab;

    [HideInInspector] public List<DogChip> Chips = new();
    private List<GameObject> _numberPopups = new();
    private CanvasSounds _sounds;


    #region SETUP

    private void Start()
    {
        _sounds = GetComponent<CanvasSounds>();
        int listIndex = 0;
        foreach (Dog dog in LevelManager.Instance.DogList)
        {
            DogChip chip = Instantiate(DogChipPrefab, ChipPlaces[listIndex].transform.position, Quaternion.identity, transform.GetChild(0))
                .GetComponent<DogChip>();
            chip.GetComponent<Image>().sprite = dog.DogParameters.ChipSprites[0];
            chip.Sprites = dog.DogParameters.ChipSprites;
            chip.Order = listIndex;
            chip.Actionable = dog;
            _numberPopups.Add(ChipPlaces[listIndex].transform.GetChild(0).gameObject);
            chip.BeginDrag.AddListener(() => TogglePopups(true));
            chip.EndDrag.AddListener(() => TogglePopups(false));
            Chips.Add(chip);
            chip.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dog.DogParameters.DogName[0].ToString();

            listIndex++;
        }
        if (ChipPlaces.Count > listIndex) ChipPlaces.RemoveRange(listIndex, ChipPlaces.Count - (listIndex));

        SetActionButton(false);
        TurnManager.Instance.SetCurrentTurnOrder(GetCurrentTurnOrder());
        UpdateDisplayedValues(true);
        TutorialPanel.SetActive(true);
    }
    #endregion

    #region CANVAS BUTTONS

    public void OnActionButtonPress()
    {
        if (TurnManager.Instance.CurrentState == GameState.Planning)  SFXPlayer.Instance.PlayClip(_sounds.Go);
        TurnManager.Instance.OnActionButtonPress();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void RevertToPreviousTurn()
    {
        SFXPlayer.Instance.PlayClip(_sounds.PreviousTurn);
        TurnManager.Instance.RevertToPreviousTurn();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void RevertToNextTurn()
    {
        SFXPlayer.Instance.PlayClip(_sounds.NextTurn);
        TurnManager.Instance.RevertToNextTurn();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ReloadLevel()
    {
        SFXPlayer.Instance.PlayClip(_sounds.Restart);
        TurnManager.Instance.ReloadLevel();
    }

    public void OpenResetPanel()
    {
        SFXPlayer.Instance.PlayClip(_sounds.AcceptOrCancel);
        ResetPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseResetPanel()
    {
        SFXPlayer.Instance.PlayClip(_sounds.AcceptOrCancel);
        ResetPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnButtonHover()
    {
        SFXPlayer.Instance.PlayClip(_sounds.ButtonHover);
    }

    public void ToggleTutorial()
    {
        TutorialPanel.SetActive(!TutorialPanel.activeSelf);
    }

    public void NextLevel()
    {
        TurnManager.Instance.NextLevel();
    }

    public void PreviousLevel()
    {
        TurnManager.Instance.PreviousLevel();
    }
    #endregion

    #region DOG CHIP MANAGEMENT

    public void OnChipDrop(DogChip dogChip, int currentIndex)
    {
        bool hasSwitched = false;
        for (int i = 0; i < ChipPlaces.Count; i++)
        {
            if (i == currentIndex) continue;
            if (Vector2.Distance(dogChip.transform.position, ChipPlaces[i].transform.position) < _chipDropDistance)
            {
                dogChip.transform.position = ChipPlaces[i].transform.position;
                dogChip.Order = i;

                Chips[i].Order = currentIndex;
                Chips[i].transform.position = ChipPlaces[currentIndex].transform.position;
                Chips[currentIndex] = Chips[i];
                Chips[i] = dogChip;
                hasSwitched = true;
                break;
            }
        }
        if (!hasSwitched) dogChip.transform.position = ChipPlaces[currentIndex].transform.position;
        TurnManager.Instance.SetCurrentTurnOrder(GetCurrentTurnOrder());
    }

    public void SetChipOrder(TurnOrder turnOrder)
    {
        for (int i = 0; i < turnOrder.OrderedActionables.Length; i++)
        {
            IActionable actionable = turnOrder.OrderedActionables[i];
            for (int j = 0; j < Chips.Count; j++)
            {
                if (Chips[j].Actionable == actionable)
                {
                    Chips[j].Order = i;
                    Chips[j].transform.position = ChipPlaces[i].transform.position;
                    (Chips[j], Chips[i]) = (Chips[i], Chips[j]);
                    break;
                }
            }
        }
    }

    public void PlayChipSound()
    {
        SFXPlayer.Instance.PlayRandomClip(_sounds.ChipHover);
    }
    #endregion

    #region OTHER PUBLIC METHODS

    public void SetActionButton(bool interactable)
    {
        ActionButton.interactable = interactable;
    }

    public void UpdateDisplayedValues(bool isPlanningPhase)
    {
        TurnText.text = $"TL : " +
            $"{PersistentInfo.Instance.LevelsInfoSO.LevelsInfo[SceneManager.GetActiveScene().buildIndex - 1].MaxTurns - TurnManager.Instance.CurrentTurnIndex}";
    }

    public void ShowWinPanel()
    {
        WinPanel.SetActive(true);
    }

    

    public TurnOrder GetCurrentTurnOrder()
    {
        List<IActionable> result = new();
        foreach (DogChip chip in Chips)
        {
            result.Add(chip.Actionable);
        }
        return new TurnOrder(result);
    }

    private void TogglePopups(bool areActive)
    {
        foreach (GameObject popup in _numberPopups)
        {
            popup.SetActive(areActive);
        }
    }

    #endregion
}
