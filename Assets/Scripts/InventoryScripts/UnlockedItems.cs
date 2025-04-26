using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnlockedItems", menuName = "ScriptableObjects/UnlockedItems", order = 1)]
public class UnlockedItems : ScriptableObject
{
    private List<string> unlockableOrgans = new List<string>
    { "Eyes", "Finger", "Stomach", "Kidney", "Intestine", "Brain", "Heart" };

    private List<string> unlockedOrgans;
    private List<string> unlockedBloodTypes;
    public Dictionary<string, int> collectedOrgansCount;
    private string newOrganName;

    public List<string> GetUnlockedBloodTypes()
    {
        int unlockedBloodCount = PlayerPrefs.GetInt("UnlockedBlood_Count", 0);
        unlockedBloodTypes = new List<string>();
        for (int i = 0; i < unlockedBloodCount; i++)
        {
            unlockedBloodTypes.Add(PlayerPrefs.GetString("UnlockedBlood_" + i));
        }
        Debug.Log(unlockedBloodTypes);
        return unlockedBloodTypes;
    }

    public void SetUnlockedBloodTypes(List<string> bloodTypes)
    {
        PlayerPrefs.SetInt("UnlockedBlood_Count", bloodTypes.Count);
        for (int i = 0; i < bloodTypes.Count; i++)
        {
            PlayerPrefs.SetString("UnlockedBlood_" + i, bloodTypes[i]);
        }

        PlayerPrefs.Save();
    }

    public List<string> GetUnlockedOrgans()
    {
        int unlockedOrgansCount = PlayerPrefs.GetInt("UnlockedOrgans_Count", 0);
        unlockedOrgans = new List<string>();
        for (int i = 0; i < unlockedOrgansCount; i++)
        {
            unlockedOrgans.Add(PlayerPrefs.GetString("UnlockedOrgans_" + i));
        }
        Debug.Log(unlockedOrgans);
        return unlockedOrgans;
    }

    public void SetUnlockedOrgans(List<string> organs)
    {
        PlayerPrefs.SetInt("UnlockedOrgans_Count", organs.Count);
        for (int i = 0; i < organs.Count; i++)
        {
            PlayerPrefs.SetString("UnlockedOrgans_" + i, organs[i]);
        }

        PlayerPrefs.Save();
    }

    public Dictionary<string, int> GetCollectedOrgansCount()
    {
        int collectedCount = PlayerPrefs.GetInt("CollectedOrgans_Count", 0);
        collectedOrgansCount = new Dictionary<string, int>();
        for (int i = 0; i < collectedCount; i++)
        {
            string key = PlayerPrefs.GetString("CollectedOrgans_Key_" + i);
            int value = PlayerPrefs.GetInt("CollectedOrgans_Value_" + i);
            collectedOrgansCount[key] = value;
        }
        return collectedOrgansCount;
    }

    public void SetCollectedOrgansCount(Dictionary<string, int> collectedOrgansCount)
    {
        PlayerPrefs.SetInt("CollectedOrgans_Count", collectedOrgansCount.Count);
        int index = 0;
        foreach (var pair in collectedOrgansCount)
        {
            PlayerPrefs.SetString("CollectedOrgans_Key_" + index, pair.Key);
            PlayerPrefs.SetInt("CollectedOrgans_Value_" + index, pair.Value);
            index++;
        }

        PlayerPrefs.Save();
    }

    public void AddCollectedItem(string itemName)
    {
        collectedOrgansCount = GetCollectedOrgansCount();
        if (collectedOrgansCount.ContainsKey(itemName))
        {
            collectedOrgansCount[itemName]++;
        }
        else
        {
            collectedOrgansCount[itemName] = 1;
        }
        SetCollectedOrgansCount(collectedOrgansCount);
    }

    public bool CheckNewUnlock()
    {
        unlockedOrgans = GetUnlockedOrgans();
        collectedOrgansCount = GetCollectedOrgansCount();
        string mostRecentUnlocked = unlockedOrgans[unlockedOrgans.Count - 1];
        if (collectedOrgansCount.ContainsKey(mostRecentUnlocked))
        {
            Debug.Log("Most recent unlocked organ: " + mostRecentUnlocked);
            Debug.Log("Collected count: " + collectedOrgansCount[mostRecentUnlocked]);
            if (collectedOrgansCount[mostRecentUnlocked] >= 20)
            {
                UnlockNextOrgan();
                return true;
            }
        }
        return false;
    }

    private void UnlockNextOrgan()
    {
        unlockedOrgans = GetUnlockedOrgans();
        collectedOrgansCount = GetCollectedOrgansCount();
        // Find the next organ to unlock
        foreach (string organ in unlockableOrgans)
        {
            if (!unlockedOrgans.Contains(organ))
            {
                unlockedOrgans.Add(organ);
                collectedOrgansCount[organ] = 0;
                newOrganName = organ;
                Debug.Log($"Unlocking new organ: {newOrganName}");
                PlayerPrefs.SetString("NewOrganName", newOrganName);
                PlayerPrefs.Save();
                SetUnlockedOrgans(unlockedOrgans);
                SetCollectedOrgansCount(collectedOrgansCount);
                break;
            }
        }
    }

    public string GetNewOrganName()
    {
        newOrganName = PlayerPrefs.GetString("NewOrganName");
        Debug.Log(newOrganName);
        return newOrganName;
    }

}
