using UnityEngine;

public class CafeStatsManager : MonoBehaviour
{
    public int npcDrinkPickupCount = 0;
    public int npc2DrinkReceiveCount = 0;
    public int totalEarnings = 0;

    // increment the count of drinks picked up by NPCs
    public void IncrementRedNPCDrinkPickup()
    {
        npcDrinkPickupCount++;
    }

    // increment the count of drinks received by NPC2
    public void IncrementVIPDrinkReceive()
    {
        npc2DrinkReceiveCount++;
    }

    // increment the total earnings
    public void AddEarnings(int amount)
    {
        totalEarnings += amount;
        Debug.Log("Total earnings: " + totalEarnings);
    }

    //reset the statistics
    public void ResetStatistics()
    {
        npcDrinkPickupCount = 0;
        npc2DrinkReceiveCount = 0;
        totalEarnings = 0;
    }
}