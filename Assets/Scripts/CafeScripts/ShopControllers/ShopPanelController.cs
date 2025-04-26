using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopPanelController : MonoBehaviour
{
    public GameObject shopPanel;
    public GameObject cafeUpgradePanel;
    public GameObject dungeonUpgradePanel;
    public GameObject catSurgeryPanel;
    public Image cafeBtn;
    public Image dungeonBtn;
    public Image catSurgeryBtn;
    public CatSaved catSaved;

    public Color selectedColor = new Color(0.4188f, 0.4188f, 0.4188f, 1f);
    public Color normalColor = Color.white;

    void Start()
    {
        if (catSaved.GetCatSaved()) {
            catSurgeryBtn.gameObject.SetActive(false);
        }
    }

    public void OpenCafeUpgrade()
    {
        dungeonUpgradePanel.SetActive(false);
        cafeUpgradePanel.SetActive(true);
        catSurgeryPanel.SetActive(false);
        cafeBtn.color = selectedColor;
        dungeonBtn.color = normalColor;
        catSurgeryBtn.color = normalColor;
    }

    public void OpenDungeonUpgrade()
    {
        cafeUpgradePanel.SetActive(false);
        dungeonUpgradePanel.SetActive(true);
        catSurgeryPanel.SetActive(false);
        dungeonBtn.color = selectedColor;
        cafeBtn.color = normalColor;
        catSurgeryBtn.color = normalColor;
    }

    public void OpenCatSurgery()
    {
        cafeUpgradePanel.SetActive(false);
        dungeonUpgradePanel.SetActive(false);
        catSurgeryPanel.SetActive(true);
        dungeonBtn.color = normalColor;
        cafeBtn.color = normalColor;
        catSurgeryBtn.color = selectedColor;
    }

    public void OpenShopPanel()
    {
        shopPanel.SetActive(true);
        Time.timeScale = 0;
        Debug.Log("ShopPanelController: shopPanel set to active");
    }

    public void CloseShopPanel()
    {
        cafeUpgradePanel.SetActive(false);
        dungeonUpgradePanel.SetActive(false);
        catSurgeryPanel.SetActive(false);
        catSurgeryBtn.color = normalColor;
        dungeonBtn.color = normalColor;
        cafeBtn.color = normalColor;
        Time.timeScale = 1;
        shopPanel.SetActive(false);
    }
}
