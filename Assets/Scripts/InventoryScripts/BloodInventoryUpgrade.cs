using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BloodInventoryUpgrade", menuName = "ScriptableObjects/BloodInventoryUpgrade", order = 1)]
public class BloodInventoryUpgrade : ScriptableObject, IUpgradeable
{
    private int baseCapacity = 20;
    private int currentCapacity;
    private int currentLevel = 0;

    private Dictionary<int, (float multiplier, int cost, string description)> levelData = new Dictionary<int, (float multiplier, int cost, string description)>
    {
        { 0, (1.0f, 0, "Base Capacity") },
        { 1, (1.5f, 200, "50% Capacity") },
        { 2, (2.0f, 300, "100% Capacity") },
        { 3, (3.0f, 500, "200% Capacity") }
    };

    public int GetCurrentCapacity()
    {
        return (int)(baseCapacity * levelData[GetCurrentLevel()].multiplier);
    }

    public int GetCurrentLevel()
    {
        if (PlayerPrefs.HasKey("BloodInventoryLevel"))
        {
            currentLevel = PlayerPrefs.GetInt("BloodInventoryLevel");
        }
        else
        {
            currentLevel = 0;
            PlayerPrefs.SetInt("BloodInventoryLevel", currentLevel);
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
        return (0, 0, "MAX Capacity");
    }

    public void UpgradeNextLevel()
    {
        if (levelData.ContainsKey(GetCurrentLevel() + 1))
        {
            currentLevel++;
            PlayerPrefs.SetInt("BloodInventoryLevel", currentLevel);
            PlayerPrefs.Save();
            currentCapacity = GetCurrentCapacity();
        }
        else
        {
            Debug.LogWarning("No more upgrades available.");
        }
    }

    public void ResetUpgrade()
    {
        currentLevel = 0;
        PlayerPrefs.SetInt("BloodInventoryLevel", currentLevel);
        PlayerPrefs.Save();
        currentCapacity = baseCapacity;
    }
}