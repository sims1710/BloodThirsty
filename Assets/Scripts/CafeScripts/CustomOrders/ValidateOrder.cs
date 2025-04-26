using System.Collections.Generic;
using UnityEngine;

public class OrderValidator : MonoBehaviour
{
    [SerializeField]
    private IngredientPrices ingredientPrices;
    public bool ValidateOrder(Order currentOrder, List<string> mixedItems)
    {
        //check if blood matches between current order and mixed items
        string bloodType = mixedItems.Find(item => ingredientPrices.IsBlood(item));
        if (bloodType != currentOrder.bloodType)
        {
            Debug.Log($"Blood type mismatch: {bloodType} != {currentOrder.bloodType}");
            return false;
        }

        //check if all ingredients match between current order and mixed items
        Dictionary<string, int> mixedIngredients = new Dictionary<string, int>();
        foreach (var item in mixedItems)
        {
            if (ingredientPrices.IsOrgan(item))
            {
                if (!mixedIngredients.ContainsKey(item))
                {
                    mixedIngredients[item] = 0;
                }
                mixedIngredients[item]++;
            }
        }

        //if the ingredient count from mixed drink is less than the order then false
        foreach (var ingredient in currentOrder.ingredients)
        {
            if (!mixedIngredients.ContainsKey(ingredient.Key) || mixedIngredients[ingredient.Key] < ingredient.Value)
            {
                Debug.Log($"Missing or insufficient ingredient: {ingredient.Key}");
                return false;
            }
        }

        //if ingredients that is not suppose to appear appear in mixed drink then false
        foreach (var mixedItem in mixedIngredients)
        {
            if (!currentOrder.ingredients.ContainsKey(mixedItem.Key))
            {
                Debug.Log($"Unexpected ingredient: {mixedItem.Key}");
                return false;
            }
        }

        return true;
    }

}