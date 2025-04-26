using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public Inventory inventory;
    public Storage storage;
    public UnlockedItems unlockedItems;
    public string levelToLoad;
    private InventoryToStorage inventoryToStorage;
    private string newOrgan;
    private UnlockedOrganController unlockedOrganController;


    [Header("Progress Tracking")]
    [SerializeField] private LevelProgress levelProgress;
    public int level;

    private void Start()
    {
        inventoryToStorage = FindObjectOfType<InventoryToStorage>();
        if (inventoryToStorage == null)
        {
            Debug.LogError("InventoryToStorage not found in the scene.");
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
        {
            StartCoroutine(HandlePortalInteraction());
        }
    }

    private IEnumerator HandlePortalInteraction()
    {
        yield return StartCoroutine(TransferInventoryToStorage());

        if (unlockedItems.CheckNewUnlock())
        {
            yield return StartCoroutine(NewUnlock());
        }
        else
        {
            Debug.Log("CheckNewUnlock returned false.");
        }

        if (level != 0)
        {
            levelProgress.SetLevelComplete(level);
        }

        yield return StartCoroutine(WaitAndLoadScene());
    }

    private IEnumerator NewUnlock()
    {
        newOrgan = unlockedItems.GetNewOrganName();
        unlockedOrganController = FindObjectOfType<UnlockedOrganController>();
        unlockedOrganController.SetNewOrgan(newOrgan);
        storage.SetItemCount(newOrgan, 0, "Organ");
        Debug.Log("CheckNewUnlock returned true.");
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator WaitAndLoadScene()
    {
        yield return new WaitForSeconds(0.5f); 
        SceneManager.LoadScene(levelToLoad); 
    }

    private IEnumerator TransferInventoryToStorage()
    {
        if (inventory.GetTotalItemCount(inventory.GetOrganInventory()) > (storage.GetOrganCapacity() - storage.GetTotalItemCount("Organ")))
        {
            inventoryToStorage.OnStorageFull();
            yield return new WaitUntil(() => inventoryToStorage.IsComplete()); // Wait for the "OnComplete" button
            Debug.Log("User pressed the 'Complete' button. Proceeding...");
            yield break;
        }
        if (inventory.GetTotalItemCount(inventory.GetBloodInventory()) > (storage.GetBloodCapacity() - storage.GetTotalItemCount("Blood")))
        {
            inventoryToStorage.OnStorageFull();
            yield return new WaitUntil(() => inventoryToStorage.IsComplete()); // Wait for the "OnComplete" button
            Debug.Log("User pressed the 'Complete' button. Proceeding...");
            yield break;
        }

        // Check and transfer organ inventory
        foreach (var organItem in inventory.GetOrganInventory())
        {
            int currentCount = storage.GetItemCount(organItem.itemType, "Organ");
            int availableCapacity = storage.GetOrganCapacity() - currentCount;
            storage.SetItemCount(organItem.itemType, currentCount + organItem.count, "Organ");
        }

        // Check and transfer blood inventory
        foreach (var bloodItem in inventory.GetBloodInventory())
        {
            int currentCount = storage.GetItemCount(bloodItem.itemType, "Blood");
            int availableCapacity = storage.GetBloodCapacity() - currentCount;
            storage.SetItemCount(bloodItem.itemType, currentCount + bloodItem.count, "Blood");
        }

        inventory.ClearInventory();
        Debug.Log("All items have been successfully transferred from Inventory to Storage.");
    }
}
