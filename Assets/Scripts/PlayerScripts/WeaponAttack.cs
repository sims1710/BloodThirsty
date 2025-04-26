using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponAttack", menuName = "ScriptableObjects/WeaponAttack", order = 1)]
public class WeaponAttack : ScriptableObject, IUpgradeable
{
    private int baseAttack = 50;
    private int currentAttack;
    private int currentLevel = 0;
    private Dictionary<int, (float multiplier, int cost, string description)> levelData = new Dictionary<int, (float multiplier, int cost, string description)>
    {
        { 0, (1.0f, 0, "Base Damage") },
        { 1, (1.5f, 300, "50% Damage") },
        { 2, (2.0f, 400, "100% Damage") },
        { 3, (3.0f, 600, "200% Damage") }
    };

    public int GetAttackDamage()
    {
        return (int)(baseAttack * levelData[GetCurrentLevel()].multiplier);
    }

    public int GetCurrentLevel()
    {
        if (PlayerPrefs.HasKey("WeaponAttackLevel"))
        {
            currentLevel = PlayerPrefs.GetInt("WeaponAttackLevel");
        }
        else
        {
            currentLevel = 0;
            PlayerPrefs.SetInt("WeaponAttackLevel", currentLevel);
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
        return (0, 0, "MAX Damage");
    }

    public void UpgradeNextLevel()
    {
        if (levelData.ContainsKey(GetCurrentLevel() + 1))
        {
            currentLevel++;
            PlayerPrefs.SetInt("WeaponAttackLevel", currentLevel);
            PlayerPrefs.Save();
            currentAttack = GetAttackDamage();
        }
        else
        {
            Debug.LogWarning("No more upgrades available.");
        }
    }

    public void ResetUpgrade()
    {
        currentLevel = 0;
        PlayerPrefs.SetInt("WeaponAttackLevel", currentLevel);
        PlayerPrefs.Save();
        currentAttack = baseAttack;
    }
}
