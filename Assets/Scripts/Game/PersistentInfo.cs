using UnityEngine;
using System.Collections.Generic;

public class PersistentInfo : MonoBehaviour
{
    public List<LevelCompletionInfo> CompletionInfo;

    public LevelsInfoSO LevelsInfoSO;

    public int MusicVolume = 4;
    public int SFXVolume = 4;

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
        _saveAndLoader.SaveData(CompletionInfo, MusicVolume, SFXVolume);
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
    }

    private void OnApplicationQuit()
    {
        Save();
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