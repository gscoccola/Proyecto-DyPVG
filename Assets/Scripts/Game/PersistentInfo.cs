using UnityEngine;
using System.Collections.Generic;

public class PersistentInfo : MonoBehaviour
{
    public List<int> LevelScores;

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
    }

    private void Start()
    {
        Load();
    }

    public int HighestAvailableLevel()
    {
        for (int i = 0; i < LevelScores.Count; i++)
        {
            if (LevelScores[i] == 0)
            {
                return i;
            }
        }
        return LevelScores.Count;
    }

    public void Save()
    {
        _saveAndLoader.SaveData(LevelScores);
    }

    public void Load()
    {
        LevelScores = _saveAndLoader.LoadData();
        while (LevelScores.Count < LevelsInfoSO.LevelsInfo.Count)
        {
            LevelScores.Add(1);
        }
    }
}
