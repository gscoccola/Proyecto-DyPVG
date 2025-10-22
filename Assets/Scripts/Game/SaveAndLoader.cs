using UnityEngine;
using System.Collections.Generic;

public class SaveAndLoader : MonoBehaviour
{
    public void SaveData(List<int> LevelScores)
    {
        string serialized = string.Join(",", LevelScores);
        PlayerPrefs.SetString("LevelScores", serialized);
    }

    public List<int> LoadData()
    {
        if (!PlayerPrefs.HasKey("LevelScores"))
        {
            Debug.Log("No saved data found.");
            return new List<int>();
        }
        string serialized = PlayerPrefs.GetString("LevelScores");
        string[] split = serialized.Split(',');
        List<int> LevelScores = new List<int>();
        foreach (string s in split)
        {
            if (int.TryParse(s, out int score)) LevelScores.Add(score);
            else Debug.LogWarning("Failed to parse score: " + s);
        }
        return LevelScores;
    }
}
