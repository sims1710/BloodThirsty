using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatisticsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI npcDrinkText;
    [SerializeField] private TextMeshProUGUI vipDrinkText;
    [SerializeField] private TextMeshProUGUI earningsText;
    private CafeStatsManager cafeStatsManager;
    private void Start()
    {
        cafeStatsManager = FindObjectOfType<CafeStatsManager>();
    }

    private void Update()
    {
        npcDrinkText.text = "Number of Red Reaper served: " + (cafeStatsManager.npcDrinkPickupCount).ToString();
        vipDrinkText.text = "Number of VIP Reaper served: " + (cafeStatsManager.npc2DrinkReceiveCount).ToString();
        earningsText.text = "Total Earnings: $" + (cafeStatsManager.totalEarnings).ToString();
    }
}