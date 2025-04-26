using UnityEngine;

public class InventoryToStorage : MonoBehaviour
{
    public GameObject panel;
    private bool isComplete = false;

    public void OnStorageFull()
    {
        Debug.Log("OnStorageFull called. Activating panel.");
        if (panel == null)
        {
            Debug.LogError("Panel is not assigned in the InventoryToStorage script.");
            return;
        }
        panel.SetActive(true);
        Debug.Log($"Panel active state after activation: {panel.activeSelf}");
        isComplete = false;
        Time.timeScale = 0; 
    }

    public void OnComplete()
    {
        Debug.Log("OnComplete called. Deactivating panel.");
        panel.SetActive(false);
        isComplete = true;
        Time.timeScale = 1; 
    }

    public bool IsComplete()
    {
        return isComplete;
    }
}
