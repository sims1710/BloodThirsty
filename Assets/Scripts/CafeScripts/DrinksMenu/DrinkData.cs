using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DrinkData", menuName = "ScriptableObjects/DrinkData", order = 1)]
public class DrinkData : ScriptableObject
{
    public List<orderSpriteMapping> bloodMappings;
    public List<orderSpriteMapping> organMappings;

    [System.Serializable]
    public class orderSpriteMapping
    {
        public string orderName; 
        public Sprite orderSprite;
    }

    public Sprite GetBloodSprite(string orderName)
    {
        foreach (var mapping in bloodMappings)
        {
            if (mapping.orderName == orderName)
            {
                return mapping.orderSprite;
            }
        }
        Debug.LogWarning($"No sprite found for item: {orderName}");
        return null;
    }

    public Sprite GetOrganSprite(string orderName)
    {
        foreach (var mapping in organMappings)
        {
            if (mapping.orderName == orderName)
            {
                return mapping.orderSprite;
            }
        }
        Debug.LogWarning($"No sprite found for item: {orderName}");
        return null;
    }
}