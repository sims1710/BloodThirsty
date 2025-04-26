using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CatSaved", menuName = "ScriptableObjects/CatSaved", order = 1)]
public class CatSaved : ScriptableObject
{
    private bool catSaved;

    public bool GetCatSaved()
    {
        catSaved = PlayerPrefs.GetInt("CatSaved", 0) == 1;
        return catSaved;
    }

    public void SetCatSaved(bool save)
    {
        PlayerPrefs.SetInt("CatSaved", save ? 1 : 0);
        PlayerPrefs.Save();
    }
}
