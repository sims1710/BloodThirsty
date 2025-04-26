using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaxHealth", menuName = "ScriptableObjects/MaxHealth", order = 1)]
public class MaxHealth : ScriptableObject, IUpgradeable
{
    private int baseHealth = 100;
    private int currentMaxHealth;
    private int currentLevel;
    private Dictionary<int, (float multiplier, int cost, string description)> levelData = new Dictionary<int, (float multiplier, int cost, string description)>
    {
        { 0, (1.0f, 0, "Base Health") },
        { 1, (1.5f, 400, "50% Health") },
        { 2, (2.0f, 500, "100% Health") },
        { 3, (3.0f, 800, "200% Health") }
    };

    public int GetCurrentHealth()
    {
        return (int)(baseHealth * levelData[GetCurrentLevel()].multiplier);
    }

    public int GetCurrentLevel()
    {
        if (PlayerPrefs.HasKey("MaxHealthLevel"))
        {
            currentLevel = PlayerPrefs.GetInt("MaxHealthLevel");
        }
        else
        {
            currentLevel = 0;
            PlayerPrefs.SetInt("MaxHealthLevel", currentLevel);
            PlayerPrefs.Save();
        }
        return currentLevel;
    }


    public (float multiplier, int cost, string description) GetNextUpgrade()
    {
        if (levelData.ContainsKey(currentLevel + 1))
        {
            return levelData[currentLevel + 1];
        }
        return (0, 0, "MAX Health");
    }

    public void UpgradeNextLevel()
    {
        if (levelData.ContainsKey(GetCurrentLevel() + 1))
        {
            currentLevel++;
            PlayerPrefs.SetInt("MaxHealthLevel", currentLevel);
            PlayerPrefs.Save();
            currentMaxHealth = GetCurrentHealth();
        }
        else
        {
            Debug.LogWarning("No more upgrades available.");
        }
    }

    public void ResetUpgrade()
    {
        currentLevel = 0;
        PlayerPrefs.SetInt("MaxHealthLevel", currentLevel);
        PlayerPrefs.Save();
        currentMaxHealth = baseHealth;
    }
}