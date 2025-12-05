using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class MainMenu : Singleton<MainMenu>
{
    [SerializeField] private UnityEngine.UI.Button[] _levelButtons;
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _levelsMenu;
    [SerializeField] private GameObject _tutorialMenu;
    [SerializeField] private AudioClip _buttonSFX;
    [SerializeField] private Sprite[] _stamps;

    [SerializeField] private UnityEngine.UI.Button _argButton;
    [SerializeField] private UnityEngine.UI.Button _USButton;

    [SerializeField] private Locale _argLocale;
    [SerializeField] private Locale _engLocale;

    private void Start()
    {
        // Setup level buttons
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
        // Setup Locale buttons
        _argButton.onClick.AddListener(SetLocaleEsp);
        _USButton.onClick.AddListener(SetLocaleEng);

    }

    public void ActivateLevelMenu()
    {
        _mainMenu.SetActive(false);
        _levelsMenu.SetActive(true);
    }

    public void BackToMainMenu()
    {
        _mainMenu.SetActive(true);
        _levelsMenu.SetActive(false);
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

    public void ResetData()
    {
        PersistentInfo.Instance.ResetData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void UnlockAll()
    {
        PersistentInfo.Instance.UnlockAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SetLocaleEsp()
    {
        LocalizationSettings.SelectedLocale = _argLocale;
        _argButton.interactable = false;
        _USButton.interactable = true;
    }

    private void SetLocaleEng()
    {
        LocalizationSettings.SelectedLocale = _engLocale;
        _USButton.interactable = false;
        _argButton.interactable = true;
    }

}
