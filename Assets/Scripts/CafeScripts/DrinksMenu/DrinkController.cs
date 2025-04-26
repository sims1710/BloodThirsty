using UnityEngine;
using System.Collections.Generic;
public class DrinkController : MonoBehaviour
{
    public IngredientPrices drinkConstants;
    public BloodType bloodType;
    public List<Ingredients> ingredients = new List<Ingredients>();
    public int price;
    void Start()
    {
        // CalculatePrice();
    }

    // public void CalculatePrice()
    // {
    //     price = 0;
    //     for (int i = 0; i<ingredients.Count; i++)
    //     {
    //         //sum of all ingredients
    //         price += drinkConstants.GetPrice(ingredients[i]);
    //     }
    //     //multiply by blood multiplier
    //     price *= (int)bloodType;
    //     Debug.Log($"Drink Price: {price} Bitecoin");
    // }
}
