using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeldSlot : MonoBehaviour
{
    public Image itemIcon;
    private string itemType;
    private string itemCategory;
    private HeldStorageManager storageManager;

    public string GetItemType()
    {
        return itemType;
    }

    public void SetItem(string type, string category)
    {
        itemType = type;
        itemCategory = category;
        LoadItemIcon();
    }

    public void SetStorageManager(HeldStorageManager manager)
    {
        storageManager = manager;
    }

    public void OnSlotButtonClicked()
    {
        storageManager.RemoveHeldItem(itemType, itemCategory);
        Destroy(gameObject);
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
}