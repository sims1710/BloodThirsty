using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class GenerateOrder : MonoBehaviour
{
    private Order currentOrder;
    [SerializeField]
    private Storage storage;
    
    public Order CreateOrder()
    {
        //get the unlocked items
        List<Storage.StorageItem> unlockedBloodTypes = storage.GetBloodStorage();
        List<Storage.StorageItem> unlockedIngredients = storage.GetOrganStorage();
        // Debug.Log($"Unlocked blood types: {string.Join(", ", unlockedBloodTypes)}");
        // Debug.Log($"Unlocked ingredients: {string.Join(", ", unlockedIngredients)}");

        //get a random blood type
        string randomBloodType = unlockedBloodTypes[Random.Range(0, unlockedBloodTypes.Count)].itemType;
        //get a random number of ingredients
        Dictionary<string, int> ingredients = new Dictionary<string, int>();
        int numberOfIngredients = Random.Range(1,3);

        for (int i = 0; i< numberOfIngredients; i++)
        {
            string ingredient = unlockedIngredients[Random.Range(0, unlockedIngredients.Count)].itemType;
            //request one of each ingredient
            if (!ingredients.ContainsKey(ingredient))
            {
                ingredients[ingredient] = Random.Range(1,2);
            }
        }
        //create order
        currentOrder = new Order(randomBloodType, ingredients);
        Debug.Log($"Order: {currentOrder.bloodType} ingredients- {string.Join(", ", currentOrder.ingredients)}");
        return currentOrder;
    }

    public Order GetCurrentOrder()
    {
        return currentOrder;
    }

    public void ClearCurrentOrder()
    {
        currentOrder = null;
    }

}
