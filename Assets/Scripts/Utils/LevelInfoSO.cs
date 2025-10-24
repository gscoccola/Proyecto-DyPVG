using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelsInfoSO", menuName = "ScriptableObjects/LevelsInfoSO")]
public class LevelsInfoSO : ScriptableObject
{
    public List<LevelTurnInfo> LevelsInfo = new();
}

[System.Serializable]
public class LevelTurnInfo
{
    public int Index;
    public int MaxTurns;
    public int ParTurns;
    public Sprite TutorialImage;
    public Sprite TutorialImageInner1;
    public Sprite TutorialImageInner2;
    public bool SkipTutorial;
}
