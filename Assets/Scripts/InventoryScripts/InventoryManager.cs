using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public Inventory inventoryData;
    public UnlockedItems unlockedItems;
    public GameObject organSlotPrefab;
    public GameObject bloodVialPrefab;
    public Transform organSlotContainer;
    public Transform bloodVialContainer;
    public TextMeshProUGUI organCapacityText;
    public TextMeshProUGUI bloodCapacityText;
    private Dictionary<string, OrganSlot> organSlots = new Dictionary<string, OrganSlot>();
    private Dictionary<string, BloodVial> bloodVials = new Dictionary<string, BloodVial>();

    private const string ORGAN_PREFIX = "Organ_";
    private const string BLOOD_PREFIX = "Blood_";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (Instance == this)
        {
            // Reset all inventory counts to 0
            foreach (var organType in unlockedItems.GetUnlockedOrgans())
            {
                inventoryData.SetItemCount(organType, 0, inventoryData.GetOrganInventory());
            }
            foreach (var bloodType in unlockedItems.GetUnlockedBloodTypes())
            {
                inventoryData.SetItemCount(bloodType, 0, inventoryData.GetBloodInventory());
            }

            // Skip loading saved data for now
            // LoadInventory();
            GenerateOrganSlots();
            GenerateBloodVials();
            UpdateCapacityTexts();

            gameObject.SetActive(false);
        }
    }

    private void LoadInventory()
    {
        // Load organ counts
        foreach (var organType in unlockedItems.GetUnlockedOrgans())
        {
            string key = ORGAN_PREFIX + organType;
            int count = PlayerPrefs.GetInt(key, 0);
            inventoryData.SetItemCount(organType, count, inventoryData.GetOrganInventory());
        }

        // Load blood vial counts
        foreach (var bloodType in unlockedItems.GetUnlockedBloodTypes())
        {
            string key = BLOOD_PREFIX + bloodType;
            int count = PlayerPrefs.GetInt(key, 0);
            inventoryData.SetItemCount(bloodType, count, inventoryData.GetBloodInventory());
        }
    }

    private void SaveInventory()
    {
        // Save organ counts
        foreach (var item in inventoryData.GetOrganInventory())
        {
            string key = ORGAN_PREFIX + item.itemType;
            PlayerPrefs.SetInt(key, item.count);
        }

        // Save blood vial counts
        foreach (var item in inventoryData.GetBloodInventory())
        {
            string key = BLOOD_PREFIX + item.itemType;
            PlayerPrefs.SetInt(key, item.count);
        }

        PlayerPrefs.Save();
    }

    private void GenerateOrganSlots()
    {
        foreach (Transform child in organSlotContainer)
        {
            Destroy(child.gameObject);
        }

        organSlots.Clear();

        foreach (var organType in unlockedItems.GetUnlockedOrgans())
        {
            GameObject slotObject = Instantiate(organSlotPrefab, organSlotContainer);
            OrganSlot slot = slotObject.GetComponent<OrganSlot>();

            if (slot != null)
            {
                slot.SetOrganType(organType);
                int count = inventoryData.GetItemCount(organType, inventoryData.GetOrganInventory());
                slot.UpdateCount(count);
                organSlots.Add(organType, slot);
            }
        }
    }

    private void GenerateBloodVials()
    {
        foreach (Transform child in bloodVialContainer)
        {
            Destroy(child.gameObject);
        }

        bloodVials.Clear();

        foreach (var bloodType in unlockedItems.GetUnlockedBloodTypes())
        {
            GameObject slotObject = Instantiate(bloodVialPrefab, bloodVialContainer);
            BloodVial slot = slotObject.GetComponent<BloodVial>();

            if (slot != null)
            {
                slot.SetBloodType(bloodType);
                int count = inventoryData.GetItemCount(bloodType, inventoryData.GetBloodInventory());
                slot.UpdateCount(count);
                bloodVials.Add(bloodType, slot);
            }
        }
    }

    public void AddOrgan(string organType, int amount = 1)
    {
        if (!unlockedItems.GetUnlockedOrgans().Contains(organType))
            return;

        int totalOrgans = GetOrgansCount();
        if (totalOrgans + amount > inventoryData.GetOrganCapacity()) return;

        int currentCount = inventoryData.GetItemCount(organType, inventoryData.GetOrganInventory());
        inventoryData.SetItemCount(organType, currentCount + amount, inventoryData.GetOrganInventory());

        if (organSlots.ContainsKey(organType))
        {
            organSlots[organType].UpdateCount(currentCount + amount);
            unlockedItems.AddCollectedItem(organType);
        }
        
        UpdateCapacityTexts();
        SaveInventory();
    }

    public void AddBloodVial(string bloodType, int amount = 1)
    {
        if (!unlockedItems.GetUnlockedBloodTypes().Contains(bloodType))
            return;

        int currentCount = inventoryData.GetItemCount(bloodType, inventoryData.GetBloodInventory());
        inventoryData.SetItemCount(bloodType, currentCount + amount, inventoryData.GetBloodInventory());

        if (bloodVials.ContainsKey(bloodType))
        {
            bloodVials[bloodType].UpdateCount(currentCount + amount);
        }

        UpdateCapacityTexts();
        SaveInventory();
    }

    public bool RemoveOrgan(string organType, int amount = 1)
    {
        int currentCount = inventoryData.GetItemCount(organType, inventoryData.GetOrganInventory());
        if (currentCount < amount)
            return false;

        inventoryData.SetItemCount(organType, currentCount - amount, inventoryData.GetOrganInventory());

        if (organSlots.ContainsKey(organType))
        {
            organSlots[organType].UpdateCount(currentCount - amount);
        }

        UpdateCapacityTexts();
        SaveInventory();
        return true;
    }

    public bool RemoveBloodVial(string bloodType, int amount = 1)
    {
        int currentCount = inventoryData.GetItemCount(bloodType, inventoryData.GetBloodInventory());
        if (currentCount < amount)
            return false;

        inventoryData.SetItemCount(bloodType, currentCount - amount, inventoryData.GetBloodInventory());

        if (bloodVials.ContainsKey(bloodType))
        {
            bloodVials[bloodType].UpdateCount(currentCount - amount);
        }

        UpdateCapacityTexts();
        SaveInventory();
        return true;
    }

    public int GetOrgansCount()
    {
        int totalOrgans = 0;
        foreach (var item in inventoryData.GetOrganInventory())
        {
            totalOrgans += item.count;
        }
        return totalOrgans;
    }

    public int GetVialsCount()
    {
        int totalBloodVials = 0;
        foreach (var item in inventoryData.GetBloodInventory())
        {
            totalBloodVials += item.count;
        }
        return totalBloodVials;
    }

    private void UpdateCapacityTexts()
    {
        int totalOrgans = GetOrgansCount();
        organCapacityText.text = $"{totalOrgans}/{inventoryData.GetOrganCapacity()}";

        int totalBloodVials = GetVialsCount();
        bloodCapacityText.text = $"{totalBloodVials}/{inventoryData.GetBloodCapacity()}";
    }

    public bool IsWithinOrganCapacity()
    {
        return GetOrgansCount() < inventoryData.GetOrganCapacity();
    }

    public bool IsWithinBloodCapacity()
    {
        return GetVialsCount() < inventoryData.GetBloodCapacity();
    }

    private void OnDestroy()
    {
        SaveInventory();
    }
}