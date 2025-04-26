using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradesController : MonoBehaviour
{
    public BiteCoins biteCoins;
    public ScriptableObject scriptableObject;
    public TextMeshProUGUI nextLevelText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI errorText;
    public Button upgradeButton;
    private IUpgradeable upgradeable;

    public void Start()
    {
        // Cast the ScriptableObject to IUpgradeable
        upgradeable = scriptableObject as IUpgradeable;

        if (upgradeable == null)
        {
            Debug.LogError("The assigned ScriptableObject does not implement IUpgradeable.");
            return;
        }

        UpdateUI();
    }

    public void UpgradeBtn()
    {
        if (upgradeable == null) return;
        if (biteCoins.SubtractAmount(upgradeable.GetNextUpgrade().cost) == false)
        {
            setErrorMessageTimer("Not enough Bitecoins!");
            return;
        }
        var nextUpgrade = upgradeable.GetNextUpgrade();
        Debug.Log($"Attempting to upgrade. Cost: {nextUpgrade.cost}, Description: {nextUpgrade.description}");
        upgradeable.UpgradeNextLevel();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (upgradeable == null) return;

        var nextUpgrade = upgradeable.GetNextUpgrade();
        Debug.Log($"Updating UI. Next Level Description: {nextUpgrade.description}, Cost: {nextUpgrade.cost}");
        nextLevelText.text = nextUpgrade.description;
        costText.text = $"{nextUpgrade.cost} Bitecoins";

        // Check if the upgrade is at max level
        if (nextUpgrade.cost == 0)
        {
            costText.text = "";
            upgradeButton.interactable = false;
            var buttonColors = upgradeButton.colors;
            buttonColors.normalColor = Color.gray;
            upgradeButton.colors = buttonColors;
        }
        else
        {
            upgradeButton.interactable = true;
            var buttonColors = upgradeButton.colors;
            buttonColors.normalColor = Color.white;
            upgradeButton.colors = buttonColors;
        }
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