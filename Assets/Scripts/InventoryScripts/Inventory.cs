using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Inventory", menuName = "ScriptableObjects/Inventory", order = 1)]
public class Inventory : ScriptableObject
{
    [System.Serializable]
    public class InventoryItem
    {
        public string itemType;
        public int count;
    }

    private List<InventoryItem> organInventory = new List<InventoryItem>();
    private List<InventoryItem> bloodInventory = new List<InventoryItem>();

    public OrganInventoryUpgrade organUpgrade;
    public BloodInventoryUpgrade bloodUpgrade;

    private int OrganCapacity => organUpgrade != null ? organUpgrade.GetCurrentCapacity() : 20;
    private int BloodCapacity => bloodUpgrade != null ? bloodUpgrade.GetCurrentCapacity() : 20;

    public List<InventoryItem> GetOrganInventory()
    {
        return organInventory;
    }

    public List<InventoryItem> GetBloodInventory()
    {
        return bloodInventory;
    }

    public int GetOrganCapacity()
    {
        return OrganCapacity;
    }

    public int GetBloodCapacity()
    {
        return BloodCapacity;
    }

    public int GetItemCount(string itemType, List<InventoryItem> inventory)
    {
        foreach (var item in inventory)
        {
            if (item.itemType == itemType)
            {
                return item.count;
            }
        }
        return 0;
    }

    public void SetItemCount(string itemType, int count, List<InventoryItem> inventory)
    {
        foreach (var item in inventory)
        {
            if (item.itemType == itemType)
            {
                item.count = count;
                return;
            }
        }
        inventory.Add(new InventoryItem { itemType = itemType, count = count });
    }

    public int GetTotalItemCount(List<InventoryItem> inventory)
    {
        int totalCount = 0;
        foreach (var item in inventory)
        {
            totalCount += item.count;
        }
        return totalCount;
    }

    public void ClearInventory()
    {
        organInventory = new List<InventoryItem>();
        bloodInventory = new List<InventoryItem>();
    }
}