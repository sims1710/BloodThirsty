using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BloodVial : MonoBehaviour
{
    public Image bloodVialIcon;
    public TextMeshProUGUI countText;
    public Button slotButton;
    private string bloodType;
    private int count;
    private BloodStorageManager storageManager;

    public void SetBloodType(string type)
    {
        bloodType = type;
        LoadBloodIcon();
    }

    public void SetStorageManager(BloodStorageManager manager)
    {
        storageManager = manager;
    }

    public void UpdateCount(int newCount)
    {
        count = newCount;
        if (countText != null)
        {
            countText.text = count.ToString();
        }
    }

    public void OnSlotButtonClicked()
    {
        if (count > 0 && storageManager != null)
        {
            storageManager.RemoveBlood(bloodType, 1);
        }
    }

    private void LoadBloodIcon()
    {
        if (bloodVialIcon != null)
        {
            string spriteName = "blood-" + bloodType.ToLower();
            Sprite[] allSprites = Resources.LoadAll<Sprite>("BloodPotions/potions-base");

            foreach (Sprite sprite in allSprites)
            {
                if (sprite.name == spriteName)
                {
                    bloodVialIcon.sprite = sprite;
                    bloodVialIcon.enabled = true;
                }
            }
        }
    }

    public string GetBloodType()
    {
        return bloodType;
    }

    public int GetCount()
    {
        return count;
    }
}