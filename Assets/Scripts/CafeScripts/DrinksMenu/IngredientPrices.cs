using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "IngredientPrices", menuName = "ScriptableObjects/IngredientPrices", order = 1)]
public class IngredientPrices : ScriptableObject
{
    private Dictionary<string, int> ingredientPrices;
    private Dictionary<string, float> bloodColorMultipliers;
    private HashSet<string> bloodItems;
    private HashSet<string> organItems;

    private void OnEnable()
    {
        // Base prices for ingredients
        ingredientPrices = new Dictionary<string, int>
        {
            { "Eyes", 10 },
            { "Finger", 15 },
            { "Stomach", 18 },
            { "Kidney", 20 },
            { "Intestine", 25 },
            { "Brain", 40 },
            { "Heart", 50 },
        };

        // Multipliers for blood colors
        bloodColorMultipliers = new Dictionary<string, float>
        {
            { "Red", 1.0f },
            { "Blue", 2.0f },
            { "Purple", 5.0f }
        };

        // Define blood and organ items
        bloodItems = new HashSet<string> { "Red", "Blue", "Purple" };
        organItems = new HashSet<string> { "Eyes", "Finger", "Stomach", "Kidney", "Intestine", "Brain", "Heart" };
    }

    // Get the base price of an ingredient
    public int GetPrice(string ingredientName)
    {
        if (ingredientPrices.TryGetValue(ingredientName, out int price))
        {
            return price;
        }
        Debug.LogWarning($"Ingredient {ingredientName} not found in prices.");
        return 0;
    }

    // Get the multiplier for a blood color
    public float GetMultiplier(string bloodColor)
    {
        if (bloodColorMultipliers.TryGetValue(bloodColor, out float multiplier))
        {
            return multiplier;
        }
        Debug.LogWarning($"Blood color {bloodColor} not found in multipliers.");
        return 1.0f; // Default multiplier
    }

    // // Get the final price of an ingredient based on the blood color multiplier
    // public int GetFinalPrice(string ingredientName, string bloodColor)
    // {
    //     int basePrice = GetPrice(ingredientName);
    //     float multiplier = GetMultiplier(bloodColor);
    //     return Mathf.RoundToInt(basePrice * multiplier);
    // }

    // Check if an item is a blood type
    public bool IsBlood(string itemName)
    {
        return bloodItems.Contains(itemName);
    }

    // Check if an item is an organ
    public bool IsOrgan(string itemName)
    {
        return organItems.Contains(itemName);
    }
}