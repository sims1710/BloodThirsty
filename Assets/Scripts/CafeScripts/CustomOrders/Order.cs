using System.Collections.Generic;

//custom order for npc2
[System.Serializable]
public class Order
{
    //request blood type
    public string bloodType;
    //request ingredients
    public Dictionary<string, int> ingredients;
    public Order(string bloodType, Dictionary<string, int> ingredients)
    {
        this.bloodType = bloodType;
        this.ingredients = ingredients;
    }
}