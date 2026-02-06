using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

public class PersistentInfo : MonoBehaviour
{
    public List<LevelCompletionInfo> CompletionInfo;

    public LevelsInfoSO LevelsInfoSO;

    public int MusicVolume = 4;
    public int SFXVolume = 4;
    public string CurrentLocale;

    [SerializeField] private Locale _argLocale;
    [SerializeField] private Locale _engLocale;

    [HideInInspector] public static PersistentInfo Instance;
    private SaveAndLoader _saveAndLoader;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        _saveAndLoader = GetComponent<SaveAndLoader>();
        Load();
        
    }

    private void Start()
    {
        GetComponent<VolumeControl>().SetVolume(false, MusicVolume);
        GetComponent<VolumeControl>().SetVolume(true, SFXVolume);
        
    }

    public int HighestAvailableLevel()
    {
        for (int i = 0; i < CompletionInfo.Count; i++)
        {
            if (CompletionInfo[i].Score == 0)
            {
                return i;
            }
        }
        return CompletionInfo.Count;
    }

    public void Save()
    {
        _saveAndLoader.SaveData(CompletionInfo, MusicVolume, SFXVolume, CurrentLocale);
    }

    public void Load()
    {
        CompletionInfo = _saveAndLoader.LoadLevelData();
        while (CompletionInfo.Count < LevelsInfoSO.LevelsInfo.Count)
        {
            CompletionInfo.Add(new LevelCompletionInfo(false, 0));
        }
        MusicVolume = _saveAndLoader.LoadMusicVolume();
        SFXVolume = _saveAndLoader.LoadSFXVolume();
        GetComponent<VolumeControl>().SetVolume(false, MusicVolume);
        GetComponent<VolumeControl>().SetVolume(true, SFXVolume);
        /*CurrentLocale = _saveAndLoader.LoadLocale();*/
        //LocalizationSettings.SelectedLocale = _engLocale;
        CurrentLocale = "eng";
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public void ResetData()
    {
        for (int i = 0; i < LevelsInfoSO.LevelsInfo.Count; i++)
        {
            CompletionInfo[i] = new LevelCompletionInfo(false, 0);
        }
        Save();
    }

    public void UnlockAll()
    {
        for (int i = 0; i < LevelsInfoSO.LevelsInfo.Count; i++)
        {
            CompletionInfo[i] = new LevelCompletionInfo(false, 1);
        }
        Save();
    }

    public void SetLocaleEsp()
    {
        CurrentLocale = "esp";
        //LocalizationSettings.SelectedLocale = _argLocale;
        StartCoroutine(ISetLocale(_argLocale));
    }

    public void SetLocaleEng()
    {
        CurrentLocale = "eng";
        //LocalizationSettings.SelectedLocale = _engLocale;
        StartCoroutine(ISetLocale(_engLocale));
    }

    public IEnumerator ISetLocale(Locale locale)
    {
        Debug.Log(LocalizationSettings.InitializationOperation.Status);
        yield return new WaitForEndOfFrame();





        //Addressables.Release(LocalizationSettings.InitializationOperation);
        LocalizationSettings.SelectedLocale = locale;


        AsyncOperationHandle<LocalizationSettings> operation = LocalizationSettings.InitializationOperation;
        yield return operation;

        
        //Debug.Log(LocalizationSettings.InitializationOperation.Status);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public int TotalChallengesCompleted()
    {
        int total = 0;
        for (int i = 0; i < CompletionInfo.Count; i++)
        {
            if (CompletionInfo[i].Score > 0 && CompletionInfo[i].Score <= LevelsInfoSO.LevelsInfo[i].ParTurns)
            {
                total++;
            }
        }
        return total;
    }
}

[System.Serializable]
public class LevelCompletionInfo
{
    public bool Seen;
    public int Score;

    public LevelCompletionInfo(bool seen, int score)
    {
        Seen = seen;
        Score = score;
    }
}

