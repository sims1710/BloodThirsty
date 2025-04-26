using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BloodStorageManager : MonoBehaviour
{
    public Storage storageData;
    public UnlockedItems unlockedItems;
    public GameObject bloodSlotPrefab;
    public Transform bloodSlotContainer;
    public TextMeshProUGUI bloodCapacityText;
    public TextMeshProUGUI errorText;
    private Dictionary<string, BloodVial> bloodVials = new Dictionary<string, BloodVial>();
    public HeldStorageManager heldStorageManager;
    public HeldStorageManagerV2 heldStorageManagerV2;

    private void Start()
    {
        GenerateBloodVials();
        UpdateCapacityText();
    }

    private void GenerateBloodVials()
    {
        foreach (var bloodType in unlockedItems.GetUnlockedBloodTypes())
        {
            Debug.Log("Unlocked blood type: " + bloodType);
        }
        
        // Clear existing slots
        foreach (Transform child in bloodSlotContainer)
        {
            Destroy(child.gameObject);
        }

        bloodVials.Clear();

        // Create slots for each unlocked blood type
        foreach (var bloodType in unlockedItems.GetUnlockedBloodTypes())
        {
            GameObject slotObject = Instantiate(bloodSlotPrefab, bloodSlotContainer);
            BloodVial slot = slotObject.GetComponent<BloodVial>();

            if (slot != null)
            {
                slot.SetBloodType(bloodType);
                slot.SetStorageManager(this);

                // Set the count from the Storage ScriptableObject
                int count = storageData.GetItemCount(bloodType, "Blood");
                slot.UpdateCount(count);

                bloodVials.Add(bloodType, slot);
            }
        }
    }

    public bool AddBlood(string bloodType, int amount = 1)
    {
        // Check if adding blood exceeds the storage capacity
        if (GetTotalBloodCount() + amount > storageData.GetBloodCapacity())
        {
            setErrorMessageTimer("Storage is full!");
            return false;
        }

        // Update the count in the Storage ScriptableObject
        int currentCount = storageData.GetItemCount(bloodType, "Blood");
        storageData.SetItemCount(bloodType, currentCount + amount, "Blood");

        // Update the UI slot if it exists
        if (bloodVials.ContainsKey(bloodType))
        {
            bloodVials[bloodType].UpdateCount(currentCount + amount);
        }

        UpdateCapacityText();
        return true;
    }

    public bool RemoveBlood(string bloodType, int amount = 1)
    {
        // Check if there is enough blood to remove
        int currentCount = storageData.GetItemCount(bloodType, "Blood");
        if (currentCount < amount)
        {
            setErrorMessageTimer("Not enough blood vials in storage!");
            return false;
        }

        if (heldStorageManager != null)
        {
            if (heldStorageManager.bloodVialCount > 0)
            {
                setErrorMessageTimer("You can only hold one vial of blood at a time!");
                return false;
            }

            storageData.SetItemCount(bloodType, currentCount - amount, "Blood");
            heldStorageManager.AddHeldItem(bloodType, "BloodVial");

            if (bloodVials.ContainsKey(bloodType))
            {
                bloodVials[bloodType].UpdateCount(currentCount - amount);
            }

            UpdateCapacityText();
            return true;
        }

        if (heldStorageManagerV2 != null)
        {
            if (heldStorageManagerV2.getexistingBloodVials().ContainsKey(bloodType))
            {
                storageData.SetItemCount(bloodType, currentCount - amount, "Blood");
                heldStorageManagerV2.AddHeldItem(bloodType, "BloodVial");

                if (bloodVials.ContainsKey(bloodType))
                {
                    bloodVials[bloodType].UpdateCount(currentCount - amount);
                }

                UpdateCapacityText();
                return true;
            }
            return false;
        }

        return false;
    }

    public int GetTotalBloodCount()
    {
        int total = 0;
        foreach (var item in storageData.GetBloodStorage())
        {
            total += item.count;
        }
        return total;
    }

    private void UpdateCapacityText()
    {
        int total = GetTotalBloodCount();
        bloodCapacityText.text = $"{total}/{storageData.GetBloodCapacity()}";
    }

    public bool IsWithinCapacity()
    {
        int total = GetTotalBloodCount();
        return total < storageData.GetBloodCapacity();
    }

    private void setErrorMessageTimer(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
        Invoke("HideErrorText", 2f); // Schedule hiding the error text after 2 seconds
    }

    private void HideErrorText()
    {
        errorText.gameObject.SetActive(false); // Hide the error text
    }
}