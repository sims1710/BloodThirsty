using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeldStorageManagerV2 : MonoBehaviour
{
    public Inventory inventoryData;
    // public UnlockedItems unlockedItems;
    public GameObject heldSlotPrefab;
    public Transform organSlotContainer;
    public Transform bloodSlotContainer;
    public OrganStorageManager organManager;
    public BloodStorageManager bloodManager;
    public AudioSource itemPopSound;
    private Queue<string> heldItems = new Queue<string>();
    private Dictionary<string, HeldSlotV2> organSlots = new Dictionary<string, HeldSlotV2>();
    private Dictionary<string, HeldSlotV2> bloodVials = new Dictionary<string, HeldSlotV2>();

    private void Start()
    {
        GenerateBloodVials();
        GenerateOrganSlots();
    }

    private void GenerateOrganSlots()
    {
        foreach (Transform child in organSlotContainer)
        {
            Destroy(child.gameObject);
        }

        organSlots.Clear();

        foreach (var inventoryItem in inventoryData.GetOrganInventory())
        {
            var organType = inventoryItem.itemType;
            int count = inventoryData.GetItemCount(organType, inventoryData.GetOrganInventory());

            if (count == 0)
            {
                continue;
            }

            GameObject slotObject = Instantiate(heldSlotPrefab, organSlotContainer);
            HeldSlotV2 slot = slotObject.GetComponent<HeldSlotV2>();

            if (slot != null)
            {
                slot.SetItem(organType, "Organ", count);
                slot.SetStorageManager(this);
                organSlots.Add(organType, slot);
            }
        }
    }

    private void GenerateBloodVials()
    {
        // Clear existing slots
        foreach (Transform child in bloodSlotContainer)
        {
            Destroy(child.gameObject);
        }

        bloodVials.Clear();

        // Create slots for each unlocked blood type
        foreach (var inventoryItem in inventoryData.GetBloodInventory())
        {
            var bloodType = inventoryItem.itemType;
            int count = inventoryData.GetItemCount(bloodType, inventoryData.GetBloodInventory());

            if (count == 0)
            {
                continue;
            }

            GameObject slotObject = Instantiate(heldSlotPrefab, bloodSlotContainer);
            HeldSlotV2 slot = slotObject.GetComponent<HeldSlotV2>();

            if (slot != null)
            {
                slot.SetItem(bloodType, "BloodVial", count);
                slot.SetStorageManager(this);
                bloodVials.Add(bloodType, slot);
            }
        }
    }

    // FOR CAFE ONLY
    public void AddHeldItem(string itemType, string itemCategory)
    {
        if (itemCategory == "Organ")
        {
            if (organSlots.ContainsKey(itemType))
            {
                organSlots[itemType].SetCount(organSlots[itemType].GetCount() + 1);
            }
            else
            {
                Debug.LogWarning($"Organ slot for {itemType} does not exist.");
            }
        }
        else if (itemCategory == "BloodVial")
        {
            if (bloodVials.ContainsKey(itemType))
            {
                bloodVials[itemType].SetCount(bloodVials[itemType].GetCount() + 1);
            }
            else
            {
                Debug.LogWarning($"Blood vial slot for {itemType} does not exist.");
            }
        }

        itemPopSound.Play();
    }

    // FOR CAFE ONLY
    public bool RemoveHeldItem(string itemType, string itemCategory)
    {
        if (itemCategory == "Organ" && organSlots.ContainsKey(itemType))
        {
            if (organManager.IsWithinCapacity())
            {
                HeldSlotV2 slot = organSlots[itemType];
                slot.SetCount(slot.GetCount() - 1);

                if (slot.GetCount() <= 0)
                {
                    Destroy(slot.gameObject);
                    organSlots.Remove(itemType);
                }

                organManager.AddOrgan(itemType);
                itemPopSound.Play();
                return true;
            }

        }
        else if (itemCategory == "BloodVial" && bloodVials.ContainsKey(itemType))
        {
            if (bloodManager.IsWithinCapacity())
            {
                HeldSlotV2 slot = bloodVials[itemType];
                slot.SetCount(slot.GetCount() - 1);

                if (slot.GetCount() <= 0)
                {
                    Destroy(slot.gameObject);
                    bloodVials.Remove(itemType);
                }

                bloodManager.AddBlood(itemType);
                itemPopSound.Play();
                return true;
            }
        }

        return false;
    }

    public Dictionary<string, HeldSlotV2> getexistingOrganSlots()
    {
        return organSlots;
    }

    public Dictionary<string, HeldSlotV2> getexistingBloodVials()
    {
        return bloodVials;
    }

    public void RemoveAllHeldItems()
    {
        foreach (Transform child in organSlotContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in bloodSlotContainer)
        {
            Destroy(child.gameObject);
        }

        organSlots.Clear();
        bloodVials.Clear();
    }
}