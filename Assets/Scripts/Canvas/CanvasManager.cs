using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CanvasManager : Singleton<CanvasManager>
{
    [Header("Parameters")]
    public List<GameObject> ChipPlaces = new();
    [SerializeField] private float _chipDropDistance = 120f;

    [Header("References")]
    public TextMeshProUGUI TurnText;
    public Button ActionButton;
    public GameObject ChipsHolder;
    public GameObject WinPanel;
    public GameObject ResetPanel;

    public GameObject DogChipPrefab;
    public GameObject ChipPlacePrefab;

    [HideInInspector] public List<DogChip> Chips = new();
    private List<GameObject> _numberPopups = new();


    #region SETUP

    private void Start()
    {
        int listIndex = 0;
        foreach (Dog dog in LevelManager.Instance.DogList)
        {
            /*GameObject chipPlace = Instantiate(ChipPlacePrefab, ChipsHolder.transform);
            ChipPlaces.Add(chipPlace);
            chipPlace.transform.position += _chipPlaceOffset * Vector3.down + _chipPlaceOrigin;*/

            DogChip chip = Instantiate(DogChipPrefab, ChipPlaces[listIndex].transform.position, Quaternion.identity, transform)
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

            //_chipPlaceOffset += _chipPlaceSpacing;
            listIndex++;
        }
        if (ChipPlaces.Count > listIndex) ChipPlaces.RemoveRange(listIndex, ChipPlaces.Count - (listIndex));

        SetActionButton(false);
        TurnManager.Instance.SetCurrentTurnOrder(GetCurrentTurnOrder());
        UpdateDisplayedValues(true);
    }
    #endregion

    #region ON BUTTON PRESS

    public void OnActionButtonPress()
    {
        TurnManager.Instance.OnActionButtonPress();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void RevertToPreviousTurn()
    {
        TurnManager.Instance.RevertToPreviousTurn();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void RevertToNextTurn()
    {
        TurnManager.Instance.RevertToNextTurn();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ReloadLevel()
    {
        TurnManager.Instance.ReloadLevel();
    }

    public void OpenResetPanel()
    {
        ResetPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseResetPanel()
    {
        ResetPanel.SetActive(false);
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
    #endregion

    #region OTHER PUBLIC METHODS

    public void SetActionButton(bool interactable)
    {
        ActionButton.interactable = interactable;
    }

    public void UpdateDisplayedValues(bool isPlanningPhase)
    {
        TurnText.text = $"{TurnManager.Instance.CurrentTurnIndex + 1}";
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
