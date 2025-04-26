using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Storage", menuName = "ScriptableObjects/Storage", order = 1)]
public class Storage : ScriptableObject
{
    [System.Serializable]
    public class StorageItem
    {
        public string itemType;
        public int count;
    }
    public List<StorageItem> organStorage;
    public List<StorageItem> bloodStorage;

    public OrganStorageUpgrade organUpgrade;
    public BloodStorageUpgrade bloodUpgrade;
    private int OrganCapacity => organUpgrade != null ? organUpgrade.GetCurrentCapacity() : 20;
    private int BloodCapacity => bloodUpgrade != null ? bloodUpgrade.GetCurrentCapacity() : 20;

    public List<StorageItem> GetOrganStorage()
    {
        LoadStorage();
        return organStorage;
    }

    public List<StorageItem> GetBloodStorage()
    {
        LoadStorage();
        return bloodStorage;
    }

    public int GetOrganCapacity()
    {
        return OrganCapacity;
    }

    public int GetBloodCapacity()
    {
        return BloodCapacity;
    }

    public int GetItemCount(string itemType, string storageName)
    {
        LoadStorage();
        List<StorageItem> storage;
        if (storageName == "Organ")
        {
            storage = organStorage;
        }
        else
        {
            storage = bloodStorage;
        }

        foreach (var item in storage)
        {
            if (item.itemType == itemType)
            {
                return item.count;
            }
        }
        return 0;
    }

    public void SetItemCount(string itemType, int count, string storageName)
    {
        LoadStorage();
        if (storageName == "Organ")
        {
            foreach (var item in organStorage)
            {
                if (item.itemType == itemType)
                {
                    item.count = count;
                    SaveStorage();
                    return;
                }
            }
            organStorage.Add(new StorageItem { itemType = itemType, count = count });
        }
        else
        {
            foreach (var item in bloodStorage)
            {
                if (item.itemType == itemType)
                {
                    item.count = count;
                    SaveStorage();
                    return;
                }
            }
            bloodStorage.Add(new StorageItem { itemType = itemType, count = count });
        }
        SaveStorage();
    }

    public int GetTotalItemCount(string storageName)
    {
        int totalCount = 0;
        LoadStorage();
        List<StorageItem> storage;
        if (storageName == "Organ")
        {
            storage = organStorage;
        }
        else
        {
            storage = bloodStorage;
        }

        foreach (var item in storage)
        {
            totalCount += item.count;
        }
        return totalCount;
    }

    public void ClearStorage()
    {
        organStorage = new List<StorageItem>() { new StorageItem { itemType = "Eyes", count = 6 } };
        bloodStorage = new List<StorageItem>() {
            new StorageItem { itemType = "Red", count = 2 },
            new StorageItem { itemType = "Blue", count = 2 },
            new StorageItem { itemType = "Purple", count = 2 }};
        SaveStorage();
    }

    public void SaveStorage()
    {
        SaveStorageList("OrganStorage", organStorage);
        SaveStorageList("BloodStorage", bloodStorage);
        PlayerPrefs.Save();
    }

    private void SaveStorageList(string keyPrefix, List<StorageItem> storage)
    {
        PlayerPrefs.SetInt(keyPrefix + "_Count", storage.Count);
        for (int i = 0; i < storage.Count; i++)
        {
            PlayerPrefs.SetString($"{keyPrefix}_Item_{i}_Type", storage[i].itemType);
            PlayerPrefs.SetInt($"{keyPrefix}_Item_{i}_Count", storage[i].count);
        }
    }

    public void LoadStorage()
    {
        organStorage = LoadStorageList("OrganStorage");
        bloodStorage = LoadStorageList("BloodStorage");
    }

    private List<StorageItem> LoadStorageList(string keyPrefix)
    {
        List<StorageItem> loadedList = new List<StorageItem>();
        int count = PlayerPrefs.GetInt(keyPrefix + "_Count", 0);
        for (int i = 0; i < count; i++)
        {
            string itemType = PlayerPrefs.GetString($"{keyPrefix}_Item_{i}_Type", "");
            int itemCount = PlayerPrefs.GetInt($"{keyPrefix}_Item_{i}_Count", 0);
            if (!string.IsNullOrEmpty(itemType))
            {
                loadedList.Add(new StorageItem { itemType = itemType, count = itemCount });
            }
        }
        return loadedList;
    }
}