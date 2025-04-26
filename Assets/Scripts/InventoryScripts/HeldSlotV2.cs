using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeldSlotV2 : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI itemCountText;
    private int itemCount;
    private string itemType;
    private string itemCategory;
    private HeldStorageManagerV2 storageManager;

    public string GetItemType()
    {
        return itemType;
    }

    public int GetCount()
    {
        return itemCount;
    }

    public void SetCount(int count)
    {
        itemCount = count;
        UpdateCount();
    }

    public void SetItem(string type, string category, int count)
    {
        itemType = type;
        itemCount = count;
        itemCategory = category;
        LoadItemIcon();
        UpdateCount();
    }

    public void SetStorageManager(HeldStorageManagerV2 manager)
    {
        storageManager = manager;
    }

    public void OnSlotButtonClicked()
    {
        if (storageManager.RemoveHeldItem(itemType, itemCategory))
        {
            UpdateCount();
            if (itemCount == 0) { Destroy(gameObject); }
        }
    }

    private void LoadItemIcon()
    {
        if (itemIcon != null)
        {
            if (itemCategory == "Organ")
            {
                Sprite itemSprite = Resources.Load<Sprite>("Organs/" + itemType.ToLower());

                if (itemSprite != null)
                {
                    itemIcon.sprite = itemSprite;
                }
            }
            else if (itemCategory == "BloodVial")
            {
                string spriteName = "blood-" + itemType.ToLower();
                Sprite[] allSprites = Resources.LoadAll<Sprite>("BloodPotions/potions-base");

                foreach (Sprite sprite in allSprites)
                {
                    if (sprite.name == spriteName)
                    {
                        itemIcon.sprite = sprite;
                    }
                }
            }
        }
    }

    public void UpdateCount()
    {
        if (itemCountText != null)
        {
            itemCountText.text = itemCount.ToString();
        }
    }
}