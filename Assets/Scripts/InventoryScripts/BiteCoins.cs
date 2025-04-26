using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BiteCoins", menuName = "ScriptableObjects/BiteCoins", order = 1)]
public class BiteCoins : ScriptableObject
{
    public int amount;

    public int GetCurrentAmount()
    {
        if (PlayerPrefs.HasKey("BiteCoinsAmount"))
        {
            amount = PlayerPrefs.GetInt("BiteCoinsAmount");
        }
        else
        {
            amount = 0;
            PlayerPrefs.SetInt("BiteCoinsAmount", amount);
            PlayerPrefs.Save();
        }
        return amount;
    }

    public void SetCurrentAmount(int value)
    {
        amount = value;
        PlayerPrefs.SetInt("BiteCoinsAmount", amount);
        PlayerPrefs.Save();
    }

    public void AddAmount(int value)
    {
        amount += value;
        PlayerPrefs.SetInt("BiteCoinsAmount", amount);
        PlayerPrefs.Save();
    }

    public bool SubtractAmount(int value)
    {
        if (amount >= value)
        {
            amount -= value;
            PlayerPrefs.SetInt("BiteCoinsAmount", amount);
            PlayerPrefs.Save();
            return true;
        }
        else
        {
            return false;
        }
    }
}