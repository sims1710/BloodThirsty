using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InstructionsTracker", menuName = "ScriptableObjects/InstructionsTracker", order = 1)]
public class InstructionsTracker : ScriptableObject
{
    private bool isNewPlayerCafe;
    private bool isNewPlayerDungeonHub;

    public bool GetIsNewPlayerCafe()
    {
        if (PlayerPrefs.HasKey("isNewPlayerCafe"))
        {
            isNewPlayerCafe = PlayerPrefs.GetInt("isNewPlayerCafe") == 1;
        }
        else
        {
            isNewPlayerCafe = false;
            PlayerPrefs.SetInt("isNewPlayerCafe", isNewPlayerCafe ? 1 : 0);
            PlayerPrefs.Save();
        }
        return isNewPlayerCafe;
    }

    public void SetIsNewPlayerCafe(bool change)
    {
        isNewPlayerCafe = change;
        PlayerPrefs.SetInt("isNewPlayerCafe", isNewPlayerCafe ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool GetIsNewPlayerDungeonHub()
    {
        if (PlayerPrefs.HasKey("isNewPlayerDungeonHub"))
        {
            isNewPlayerDungeonHub = PlayerPrefs.GetInt("isNewPlayerDungeonHub") == 1;
        }
        else
        {
            isNewPlayerDungeonHub = false;
            PlayerPrefs.SetInt("isNewPlayerDungeonHub", isNewPlayerDungeonHub ? 1 : 0);
            PlayerPrefs.Save();
        }
        return isNewPlayerDungeonHub;
    }

    public void SetIsNewPlayerDungeonHub(bool change)
    {
        isNewPlayerDungeonHub = change;
        PlayerPrefs.SetInt("isNewPlayerDungeonHub", isNewPlayerDungeonHub ? 1 : 0);
        PlayerPrefs.Save();
    }
}