using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelProgress", menuName = "ScriptableObjects/LevelProgress")]
public class LevelProgress : ScriptableObject
{
    [System.Serializable]
    public class LevelInfo
    {
        public int levelNum;
        public bool isCompleted;
    }

    [System.Serializable]
    private class LevelListWrapper
    {
        public List<LevelInfo> levels;
    }

    private List<LevelInfo> levels = new List<LevelInfo>();

    public void SetLevelComplete(int levelNum)
    {
        foreach (var level in levels)
        {
            if (level.levelNum == levelNum)
            {
                level.isCompleted = true;
                string levelsJson = JsonUtility.ToJson(new LevelListWrapper { levels = this.levels });
                PlayerPrefs.SetString("LevelProgress", levelsJson);
                PlayerPrefs.Save();
                return;
            }
        }
    }

    public bool IsLevelCompleted(int levelNum)
    {
        if (PlayerPrefs.HasKey("LevelProgress"))
        {
            string levelsJson = PlayerPrefs.GetString("LevelProgress");
            levels = JsonUtility.FromJson<LevelListWrapper>(levelsJson).levels;
            Debug.Log("LevelProgress loaded.");
        }
        else
        {
            Debug.Log("No saved LevelProgress found. Initializing empty progress.");
            levels = new List<LevelInfo>();
        }

        foreach (var level in levels)
        {
            if (level.levelNum == levelNum)
                return level.isCompleted;
        }
        return false;
    }

    public void ResetLevelProgress()
    {
        levels = new List<LevelInfo>
        {
        new LevelInfo { levelNum = 1, isCompleted = false },
        new LevelInfo { levelNum = 2, isCompleted = false },
        new LevelInfo { levelNum = 3, isCompleted = false }
        };

        string levelsJson = JsonUtility.ToJson(new LevelListWrapper { levels = this.levels });
        PlayerPrefs.SetString("LevelProgress", levelsJson);
        PlayerPrefs.Save();
    }
}
