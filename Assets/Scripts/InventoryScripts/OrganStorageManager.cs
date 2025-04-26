using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OrganStorageManager : MonoBehaviour
{
    public Storage storageData;
    public UnlockedItems unlockedItems;
    public GameObject organSlotPrefab;
    public Transform organSlotContainer;
    public TextMeshProUGUI organCapacityText;
    public TextMeshProUGUI errorText;
    private Dictionary<string, OrganSlot> organSlots = new Dictionary<string, OrganSlot>();
    public HeldStorageManager heldStorageManager;
    public HeldStorageManagerV2 heldStorageManagerV2;

    private void Start()
    {
        GenerateOrganSlots();
        UpdateCapacityText();
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
                slot.SetStorageManager(this);

                // Set the count from the Storage ScriptableObject
                int count = storageData.GetItemCount(organType, "Organ");
                slot.UpdateCount(count);

                organSlots.Add(organType, slot);
            }
        }
    }

    public bool AddOrgan(string organType, int amount = 1)
    {
        int totalOrgans = GetTotalOrganCount();
        if (totalOrgans + amount > storageData.GetOrganCapacity())
        {
            setErrorMessageTimer("Storage is full!");
            return false;
        }

        int currentCount = storageData.GetItemCount(organType, "Organ");
        storageData.SetItemCount(organType, currentCount + amount, "Organ");

        if (organSlots.ContainsKey(organType))
        {
            organSlots[organType].UpdateCount(currentCount + amount);
        }

        UpdateCapacityText();
        return true;
    }

    public bool RemoveOrgan(string organType, int amount = 1)
    {
        int currentCount = storageData.GetItemCount(organType, "Organ");
        if (currentCount < amount)
        {
            setErrorMessageTimer("Not enough organs in storage!");
            return false;
        }

        if (heldStorageManager != null)
        {
            if (heldStorageManager.organCount >= 2)
            {
                setErrorMessageTimer("You can only hold two organs at a time!");
                return false;
            }

            if (heldStorageManager.ContainsItem(organType))
            {
                setErrorMessageTimer("You already have this organ!");
                return false;
            }

            storageData.SetItemCount(organType, currentCount - amount, "Organ");
            heldStorageManager.AddHeldItem(organType, "Organ");

            if (organSlots.ContainsKey(organType))
            {
                organSlots[organType].UpdateCount(currentCount - amount);
            }

            UpdateCapacityText();
            return true;
        }

        if (heldStorageManagerV2 != null)
        {
            if (heldStorageManagerV2.getexistingOrganSlots().ContainsKey(organType))
            {
                storageData.SetItemCount(organType, currentCount - amount, "Organ");
                heldStorageManagerV2.AddHeldItem(organType, "Organ");

                if (organSlots.ContainsKey(organType))
                {
                    organSlots[organType].UpdateCount(currentCount - amount);
                }

                UpdateCapacityText();
                return true;
            }
        }

        return false;
    }

    public int GetTotalOrganCount()
    {
        int total = 0;
        foreach (var item in storageData.GetOrganStorage())
        {
            total += item.count;
        }
        return total;
    }

    private void UpdateCapacityText()
    {
        int total = GetTotalOrganCount();
        organCapacityText.text = $"{total}/{storageData.GetOrganCapacity()}";
    }

    public bool IsWithinCapacity()
    {
        int total = GetTotalOrganCount();
        return total < storageData.GetOrganCapacity();
    }

    private void setErrorMessageTimer(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
        Invoke("HideErrorText", 2f);
    }

    private void HideErrorText()
    {
        errorText.gameObject.SetActive(false);
    }
}