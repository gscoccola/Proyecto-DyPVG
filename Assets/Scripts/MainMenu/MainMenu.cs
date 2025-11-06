using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenu : Singleton<MainMenu>
{
    [SerializeField] private UnityEngine.UI.Button[] _levelButtons;
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _levelsMenu;
    [SerializeField] private GameObject _tutorialMenu;
    [SerializeField] private AudioClip _buttonSFX;
    [SerializeField] private Sprite[] _stamps;

    private void Start()
    {
        
        for (int i = 0; i < _levelButtons.Length; i++)
        {
            if (i > PersistentInfo.Instance.HighestAvailableLevel())
            {
                _levelButtons[i].interactable = false;
            }
            else
            {
                int j = i;
                _levelButtons[i].onClick.AddListener(() => SceneTransition.Instance.LoadScene(j+1));
            }
            if (PersistentInfo.Instance.CompletionInfo[i].Score > 0 )
            {
                _levelButtons[i].transform.GetChild(0).gameObject.SetActive(true);
                if (PersistentInfo.Instance.CompletionInfo[i].Score <= PersistentInfo.Instance.LevelsInfoSO.LevelsInfo[i].ParTurns)
                    _levelButtons[i].transform.GetChild(0).GetComponent<Image>().sprite = _stamps[1];
                else _levelButtons[i].transform.GetChild(0).GetComponent<Image>().sprite = _stamps[0];
            }
        }
    }

    public void ActivateLevelMenu()
    {
        _mainMenu.SetActive(false);
        _levelsMenu.SetActive(true);
    }

    public void ToggleTutorialMenu()
    {
        _levelsMenu.SetActive(!_levelsMenu.activeSelf);
        _tutorialMenu.SetActive(!_tutorialMenu.activeSelf);
    }

    public void OnButtonHover()
    {
        SFXPlayer.Instance.PlayClip(_buttonSFX, 1, false, true);
    }

}
