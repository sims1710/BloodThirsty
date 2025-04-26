using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrganSlot : MonoBehaviour
{
    public Image organIcon;
    public TextMeshProUGUI countText;
    public Button slotButton;
    private string organType;
    private int count;
    private OrganStorageManager storageManager;

    public void SetOrganType(string type)
    {
        organType = type;
        LoadOrganIcon();
    }

    public void SetStorageManager(OrganStorageManager manager)
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
            storageManager.RemoveOrgan(organType, 1);
        }
    }

    private void LoadOrganIcon()
    {
        if (organIcon != null)
        {
            Sprite organSprite = Resources.Load<Sprite>("Organs/" + organType.ToLower());

            if (organSprite != null)
            {
                organIcon.sprite = organSprite;
            }
            else
            {
                Debug.LogWarning("No icon found for organ type: " + organType);
            }
        }
    }
}