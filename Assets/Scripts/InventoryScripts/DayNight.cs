using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DayNight", menuName = "ScriptableObjects/DayNight", order = 1)]
public class DayNight : ScriptableObject
{
    private bool isDay;

    public bool GetTime()
    {
        isDay = PlayerPrefs.GetInt("IsDay", 0) == 1;
        return isDay;
    }

    public void SetTime(string time)
    {
        PlayerPrefs.SetInt("IsDay", time == "Day" ? 1 : 0);
        PlayerPrefs.Save();
    }
}
