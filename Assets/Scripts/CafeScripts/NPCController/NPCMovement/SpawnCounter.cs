using UnityEngine;
using TMPro;

public class SpawnCounter : MonoBehaviour
{
    public SpawnSettings spawnSettings;
    public TextMeshProUGUI amountText;
    void Start()
    {
        // initialize current amount to 0
        spawnSettings.currentAmount = 0; 
        amountText.text = "Customers: " + spawnSettings.currentAmount.ToString() + "/" + spawnSettings.GetMaxVampires().ToString();
    }

    // Update is called once per frame
    void Update()
    {
        amountText.text = "Customers: " + spawnSettings.currentAmount.ToString() + "/" + spawnSettings.GetMaxVampires().ToString();
    }
}
