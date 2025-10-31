using UnityEngine;

public class TutorialsMenu : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button[] _buttons;
    [SerializeField] private GameObject[] _displays;
    [SerializeField] private GameObject _select;

    private void Start()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
             int j = i;
            _buttons[i].onClick.AddListener(() => ToggleDisplay(j));
        }
    }

    private void ToggleDisplay(int index)
    {
        _select.SetActive(false);
        _displays[index].SetActive(!_displays[index].activeSelf);
    }

    public void CloseAll()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            _displays[i].SetActive(false);
        }
        _select.SetActive(true);
    }
}
