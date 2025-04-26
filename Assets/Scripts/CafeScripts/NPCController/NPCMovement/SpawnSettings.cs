using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SpawnSettings", menuName = "ScriptableObjects/SpawnSettings", order = 1)]
public class SpawnSettings : ScriptableObject
{
    public int maxVampires;
    public float minSpawnTime;
    public float maxSpawnTime;
    public int currentAmount;

    public void ResetWithCustomMax(int newMax)
    {
        maxVampires = newMax;
        PlayerPrefs.SetInt("MaxVampires", newMax);
        PlayerPrefs.Save();
        currentAmount = 0;
    }

    public int GetMaxVampires()
    {
        return PlayerPrefs.GetInt("MaxVampires");
    }
}