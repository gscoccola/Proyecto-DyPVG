using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CanvasManager : Singleton<CanvasManager>
{
    [Header("Parameters")]
    [SerializeField] private float _chipPlaceSpacing = 200f;
    [SerializeField] private float _chipDropDistance = 120f;

    [Header("References")]
    public TextMeshProUGUI TurnText;
    public TextMeshProUGUI ActionButtonText;
    public Button ActionButton;
    public GameObject ChipsHolder;

    public GameObject DogChipPrefab;
    public GameObject ChipPlacePrefab;

    [HideInInspector] public List<GameObject> ChipPlaces = new();
    [HideInInspector] public List<DogChip> Chips = new();

    private float _chipPlaceOffset;


    #region SETUP

    private void Start()
    {
        int listIndex = 0;
        foreach (Dog dog in LevelManager.Instance.DogList)
        {
            GameObject chipPlace = Instantiate(ChipPlacePrefab, ChipsHolder.transform);
            ChipPlaces.Add(chipPlace);
            chipPlace.transform.position += _chipPlaceOffset * Vector3.right;

            DogChip chip = Instantiate(DogChipPrefab, chipPlace.transform.position, Quaternion.identity, transform)
                .GetComponent<DogChip>();
            chip.Order = listIndex;
            chip.Actionable = dog;
            chip.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dog.Initial;
            Chips.Add(chip);

            _chipPlaceOffset += _chipPlaceSpacing;
            listIndex++;
        }

        SetActionButton(false);
        TurnManager.Instance.SetCurrentTurnOrder(CurrentTurnOrder());
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
        TurnText.text = $"{TurnManager.Instance.CurrentTurnIndex + 1}";
    }

    public void OnChipDrop(DogChip dogChip, int currentIndex)
    {
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
                break;
            }
            else dogChip.transform.position = ChipPlaces[currentIndex].transform.position;
        }
        TurnManager.Instance.SetCurrentTurnOrder(CurrentTurnOrder());
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

    public TurnOrder CurrentTurnOrder()
    {
        List<IActionable> result = new();
        foreach (DogChip chip in Chips)
        {
            result.Add(chip.Actionable);
        }
        return new TurnOrder(result);
    }

    #endregion
}
