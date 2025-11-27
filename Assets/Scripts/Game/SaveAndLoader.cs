using UnityEngine;
using System.Collections.Generic;

public class SaveAndLoader : MonoBehaviour
{
    public void SaveData(List<LevelCompletionInfo> CompletionInfo, int musicVolume, int sFXVolume)
    {
        string serializedScores = "";
        string serializedSeenStatus = "";

        foreach (var info in CompletionInfo)
        {
            serializedScores += info.Score.ToString() + ",";
            serializedSeenStatus += (info.Seen ? "1" : "0") + ",";
        }


        PlayerPrefs.SetString("LevelScores", serializedScores);
        PlayerPrefs.SetString("SeenLevels", serializedSeenStatus);
        PlayerPrefs.SetString("MusicVolume", musicVolume.ToString());
        PlayerPrefs.SetString("SFXVolume", sFXVolume.ToString());

    }

    public List<LevelCompletionInfo> LoadLevelData()
    {
        if (!PlayerPrefs.HasKey("LevelScores") || !PlayerPrefs.HasKey("SeenLevels"))
        {
            Debug.Log("No saved data found.");
            return new List<LevelCompletionInfo>();
        }
        /*Debug.Log(PlayerPrefs.GetString("LevelScores"));
        Debug.Log(PlayerPrefs.GetString("SeenLevels"));*/

        string[] splitScores = PlayerPrefs.GetString("LevelScores").Split(',', System.StringSplitOptions.RemoveEmptyEntries);
        string[] splitSeen = PlayerPrefs.GetString("SeenLevels").Split(',', System.StringSplitOptions.RemoveEmptyEntries);

        List<LevelCompletionInfo> CompletionInfo = new();
        for (int i = 0; i < splitScores.Length; i++)
        {
            if (!int.TryParse(splitScores[i], out int score)) Debug.LogWarning("Failed to parse score: " + splitScores[i]);
            if (!int.TryParse(splitSeen[i], out int seen)) Debug.LogWarning("Failed to parse score: " + splitSeen[i]);

            CompletionInfo.Add(new LevelCompletionInfo(seen == 1, score));
        }
        return CompletionInfo;
    }

    public int LoadMusicVolume()
    {
        if (!PlayerPrefs.HasKey("MusicVolume"))
        {
            Debug.Log("No saved music volume found. Using default.");
            return 4;
        }
        if (!int.TryParse(PlayerPrefs.GetString("MusicVolume"), out int musicVolume))
        {
            Debug.LogWarning("Failed to parse music volume. Using default.");
            return 4;
        }
        return musicVolume;
    }

    public int LoadSFXVolume()
    {
        if (!PlayerPrefs.HasKey("SFXVolume"))
        {
            Debug.Log("No saved SFX volume found. Using default.");
            return 4;
        }
        if (!int.TryParse(PlayerPrefs.GetString("SFXVolume"), out int sFXVolume))
        {
            Debug.LogWarning("Failed to parse SFX volume. Using default.");
            return 4;
        }
        return sFXVolume;
    }
}
