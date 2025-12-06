using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

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
        CurrentLocale = _saveAndLoader.LoadLocale();
        LocalizationSettings.SelectedLocale = CurrentLocale == "esp" ? _argLocale : _engLocale;
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
        LocalizationSettings.SelectedLocale = _argLocale;
        
    }

    public void SetLocaleEng()
    {
        CurrentLocale = "eng";
        LocalizationSettings.SelectedLocale = _engLocale;
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

