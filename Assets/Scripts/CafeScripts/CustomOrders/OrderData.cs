using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "OrderData", menuName = "ScriptableObjects/OrderData", order = 1)]
public class OrderData : ScriptableObject
{
    public List<orderSpriteMapping> ingredientMappings;

    [System.Serializable]
    public class orderSpriteMapping
    {
        public string orderName; 
        public Sprite orderSprite;
    }

    public Sprite GetSprite(string orderName)
    {
        foreach (var mapping in ingredientMappings)
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