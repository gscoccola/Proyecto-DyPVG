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
    public GameObject TutorialInnerPanel;
    public GameObject TutorialOKButton;

    public GameObject PausePanel;
    public Image GOLosePopup;
    public Sprite[] GOLosePopupSprites;

    public GameObject DogChipPrefab;
    public GameObject ChipPlacePrefab;

    public TextMeshProUGUI turnParText;
    public Image turnParPanel;
    public Sprite[] turnParSprites;

    [HideInInspector] public List<DogChip> Chips = new();
    private List<GameObject> _numberPopups = new();
    private CanvasSounds _sounds;
    private int currentOpenTutorial;
    [HideInInspector] public int DrawnPaths;

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
        ToggleTutorial();
    }

    private void LoadTutorials(int levelIndex)
    {
        TutorialPanel.GetComponent<Image>().sprite =
            PersistentInfo.Instance.LevelsInfoSO.LevelsInfo[levelIndex].TutorialImage;
        TutorialInnerPanel.GetComponent<Image>().sprite =
            PersistentInfo.Instance.LevelsInfoSO.LevelsInfo[levelIndex].TutorialImageInner1;
        currentOpenTutorial = levelIndex;
    }

    public void CycleTutorial()
    {
        TutorialOKButton.SetActive(true);
         if (TutorialInnerPanel.GetComponent<Image>().sprite ==
            PersistentInfo.Instance.LevelsInfoSO.LevelsInfo[currentOpenTutorial].TutorialImageInner1)
        {
            TutorialInnerPanel.GetComponent<Image>().sprite =
                PersistentInfo.Instance.LevelsInfoSO.LevelsInfo[currentOpenTutorial].TutorialImageInner2;
        }
        else
        {
            TutorialInnerPanel.GetComponent<Image>().sprite =
                PersistentInfo.Instance.LevelsInfoSO.LevelsInfo[currentOpenTutorial].TutorialImageInner1;
        }
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
        SetActionButton(false);
        SFXPlayer.Instance.PlayClip(_sounds.PreviousTurn);
        TurnManager.Instance.RevertToPreviousTurn();
        EventSystem.current.SetSelectedGameObject(null);
        SetLosePanel(false);
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
        /*if (GOLosePopup.enabled)
        {
            SetLosePanel(false);
            TurnManager.Instance.ReloadLevel();
            return;
        }*/
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
        if (TutorialPanel == null) return;
        PausePanel.SetActive(false);
        if (PersistentInfo.Instance.LevelsInfoSO.LevelsInfo[currentOpenTutorial].SkipTutorial) return;
        TutorialPanel.SetActive(!TutorialPanel.activeSelf);
        currentOpenTutorial = SceneManager.GetActiveScene().buildIndex - 1;
        LoadTutorials(currentOpenTutorial);
    }

    public void NextTutorial()
    {
        if (currentOpenTutorial == PersistentInfo.Instance.LevelsInfoSO.LevelsInfo.Count - 1) return;
        LoadTutorials(currentOpenTutorial + 1);
    }

    public void PreviousTutorial()
    {
        if (currentOpenTutorial == 0) return;
        LoadTutorials(currentOpenTutorial - 1);
    }

    public void NextLevel()
    {
        TurnManager.Instance.NextLevel();
    }

    public void PreviousLevel()
    {
        TurnManager.Instance.PreviousLevel();
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void TogglePausePanel()
    {
        bool isActive = !PausePanel.activeSelf;
        PausePanel.SetActive(isActive);
        if (isActive)
        {
            Time.timeScale = 0f;
            SFXPlayer.Instance.PlayClip(_sounds.AcceptOrCancel);
        }
        else
        {
            Time.timeScale = 1f;
            SFXPlayer.Instance.PlayClip(_sounds.AcceptOrCancel);
        }
        EventSystem.current.SetSelectedGameObject(null);
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
        if (interactable) Debug.LogError("Setting action button interactable to true from CanvasManager");
        DrawnPaths = 0;
        ActionButton.interactable = false;
    }
    public void ChangeActivePaths(int amount)
    {
        DrawnPaths += amount;
        ActionButton.interactable = DrawnPaths > 0;
    }

    public void UpdateDisplayedValues(bool isPlanningPhase)
    {
        int turnsLeft = 
        PersistentInfo.Instance.LevelsInfoSO.LevelsInfo[SceneManager.GetActiveScene().buildIndex - 1].MaxTurns - TurnManager.Instance.CurrentTurnIndex;
        TurnText.text = turnsLeft.ToString();
        if (turnsLeft == 0) TurnText.color = Color.red;
        else TurnText.color = Color.white;
    }

    public void ShowWinPanel()
    {
        TurnManager.Instance.CurrentTurnIndex++;
        UpdateDisplayedValues(false);
        WinPanel.SetActive(true);
        bool challengeAchieved = 
            PersistentInfo.
            Instance.LevelsInfoSO.LevelsInfo[SceneManager.GetActiveScene().buildIndex - 1].ParTurns >=
            TurnManager.Instance.CurrentTurnIndex;

        turnParPanel.sprite = challengeAchieved ? turnParSprites[0] : turnParSprites[1];
        turnParText.color = challengeAchieved ? Color.black : Color.white;
        turnParText.text = "Ganá en " + PersistentInfo.Instance.LevelsInfoSO.LevelsInfo[SceneManager.GetActiveScene().buildIndex - 1].ParTurns + " turnos";
    }

    public void SetLosePanel(bool isActive)
    {
        GOLosePopup.sprite = isActive ? GOLosePopupSprites[1] : GOLosePopupSprites[0];
        //GOLosePopup.enabled = isActive ;
        //LosePanel.SetActive(isActive);
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
