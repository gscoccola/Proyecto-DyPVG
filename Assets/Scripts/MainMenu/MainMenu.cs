using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button[] _levelButtons;
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _levelsMenu;

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
                _levelButtons[i].onClick.AddListener(() => SceneManager.LoadScene( j +1 ));
            }
        }
    }

    public void ActivateLevelMenu()
    {
        _mainMenu.SetActive(false);
        _levelsMenu.SetActive(true);
    }

}
