using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    public BiteCoins biteCoins;
    public BloodInventoryUpgrade bloodInventoryUpgrade;
    public OrganInventoryUpgrade organInventoryUpgrade;
    public BloodStorageUpgrade bloodStorageUpgrade;
    public OrganStorageUpgrade organStorageUpgrade;
    public Inventory inventory;
    public Storage storage;
    public MaxHealth maxHealth;
    public WeaponAttack weaponAttack;
    public UnlockedItems unlockedItems;
    public LevelProgress levelProgress;
    public InstructionsTracker instructionsTracker;
    public SpawnSettings spawnSettings;
    public CatSaved catSaved;
    public DayNight dayNight;
    public GameObject section1;
    public GameObject section2;
    public GameObject section3;
    public GameObject section4;
    public GameObject section5;
    public GameObject section6;
    public GameObject section7;
    public void StartNew()
    {
        ResetProgress();

        section1.SetActive(false);
        section2.SetActive(true);
    }

    private void ResetProgress()
    {
        // Reset BiteCoins to 0
        biteCoins.SetCurrentAmount(200);

        // Reset inventory and storage levels to their initial state
        bloodInventoryUpgrade.ResetUpgrade();
        organInventoryUpgrade.ResetUpgrade();
        bloodStorageUpgrade.ResetUpgrade();
        organStorageUpgrade.ResetUpgrade();

        inventory.ClearInventory();
        storage.ClearStorage();

        // Reset MaxHealth and WeaponAttack levels
        maxHealth.ResetUpgrade();
        weaponAttack.ResetUpgrade();

        // Reset unlocked items
        unlockedItems.SetUnlockedOrgans(new List<string> { "Eyes" });
        unlockedItems.SetUnlockedBloodTypes(new List<string> { "Red", "Blue", "Purple" });
        unlockedItems.SetCollectedOrgansCount(new Dictionary<string, int> { { "Eyes", 0 } });

        // Reset level progress
        levelProgress.ResetLevelProgress();

        // Reset instructions tracker
        instructionsTracker.SetIsNewPlayerCafe(true);
        instructionsTracker.SetIsNewPlayerDungeonHub(true);

        // Reset day night 
        dayNight.SetTime("Day");

        // Reset cat
        catSaved.SetCatSaved(false);

        // Reset spawn settings
        spawnSettings.ResetWithCustomMax(20);
    }

    public void LoadGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("DungeonHub");
    }

    public void OnNext(string sectionName)
    {
        switch (sectionName)
        {
            case "Section2":
                section2.SetActive(false);
                section3.SetActive(true);
                break;
            case "Section3":
                section3.SetActive(false);
                section4.SetActive(true);
                break;
            case "Section4":
                section4.SetActive(false);
                section5.SetActive(true);
                break;
            case "Section5":
                section5.SetActive(false);
                section6.SetActive(true);
                break;
            case "Section6":
                section6.SetActive(false);
                section7.SetActive(true);
                break;
            case "Section7":
                ResetProgress();
                UnityEngine.SceneManagement.SceneManager.LoadScene("VampireCafe");
                break;
            default:
                Debug.LogError("Unknown section name: " + sectionName);
                break;
        }
    }
}
