using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeldStorageManager : MonoBehaviour
{
    public GameObject heldSlotPrefab;
    public Transform heldSlotContainer;
    public UnlockedItems unlockedItems;
    public OrganStorageManager organManager;
    public BloodStorageManager bloodManager;
    public TextMeshProUGUI placeholderText;
    public AudioSource itemPopSound;
    private Queue<string> heldItems = new Queue<string>();
    public int organCount = 0;
    public int bloodVialCount = 0;
    public int totalCount = 0;

    private void CreateHeldSlot(string itemType, string itemCategory)
    {
        GameObject slotObject = Instantiate(heldSlotPrefab, heldSlotContainer);
        HeldSlot slot = slotObject.GetComponent<HeldSlot>();

        if (slot != null)
        {
            slot.SetItem(itemType, itemCategory);
            slot.SetStorageManager(this);
        }
    }

    // FOR CAFE ONLY
    public void AddHeldItem(string itemType, string itemCategory)
    {
        heldItems.Enqueue(itemType);
        CreateHeldSlot(itemType, itemCategory);
        if (itemCategory == "Organ")
        {
            organCount++;
        }
        else if (itemCategory == "BloodVial")
        {
            bloodVialCount++;
        }
        ChangeTextState();
        itemPopSound.Play();
        Debug.Log(heldItems);
    }

    // FOR CAFE ONLY
    public void RemoveHeldItem(string itemType, string itemCategory)
    {
        Queue<string> tempQueue = new Queue<string>();
        while (heldItems.Count > 0)
        {
            string currentItem = heldItems.Dequeue();
            if (currentItem != itemType)
            {
                tempQueue.Enqueue(currentItem);
            }
        }
        heldItems = tempQueue;
        
        if (itemCategory == "Organ")
        {
            organCount--;
            organManager.AddOrgan(itemType);
        }
        else if (itemCategory == "BloodVial")
        {
            bloodVialCount--;
            bloodManager.AddBlood(itemType);
        }
        ChangeTextState();
        itemPopSound.Play();
    }

    public bool ContainsItem(string itemType)
    {
        return heldItems.Contains(itemType);
    }

    public void ChangeTextState()
    {
        if (heldItems.Count == 0)
        {
            placeholderText.gameObject.SetActive(true);
        }
        else
        {
            placeholderText.gameObject.SetActive(false);
        }
    }

    public Queue<string> GetHeldItems()
    {
        return heldItems;
    }

    public void RemoveAllHeldItems()
    {
        foreach (Transform child in heldSlotContainer)
        {
            Destroy(child.gameObject);
        }
        heldItems.Clear();
        organCount = 0;
        bloodVialCount = 0;
        ChangeTextState();
    }
}