using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayOrder : MonoBehaviour
{
    [SerializeField]
    private Transform orderDisplayParent;
    [SerializeField]
    private GameObject orderSpritePrefab;
    [SerializeField]
    private OrderData orderData;
    private float spriteSpacing = 0.45f;
    private float leftOffset = -0.4f;

    public void ShowOrder(Order order)
    {
        // clear existing sprites
        foreach (Transform child in orderDisplayParent)
        {
            Destroy(child.gameObject);
        }
        Vector3 currentPosition = new Vector3(leftOffset, 0.1f, 0);
        // display bloodtype sprite
        Sprite bloodSprite = orderData.GetSprite(order.bloodType);
        CreateOrderSprite(bloodSprite, 1, currentPosition);
        currentPosition.x += spriteSpacing;

        HashSet<string> displayedIngredients = new HashSet<string>();

        // display organs sprites
        foreach (var ingredient in order.ingredients)
        {
            if (!displayedIngredients.Contains(ingredient.Key))
            {
                Sprite ingredientSprite = orderData.GetSprite(ingredient.Key);
                if (ingredientSprite != null)
                {
                    CreateOrderSprite(ingredientSprite, ingredient.Value, currentPosition);
                    currentPosition.x += spriteSpacing;
                    //add into displayed ingredients
                    displayedIngredients.Add(ingredient.Key);
                }
            }
        }
    }

    private void CreateOrderSprite(Sprite sprite, int quantity, Vector3 position)
    {
        GameObject orderSpriteObject = Instantiate(orderSpritePrefab, orderDisplayParent);
        //set position of sprite
        orderSpriteObject.transform.localPosition = position;
        SpriteRenderer spriteComponent = orderSpriteObject.GetComponent<SpriteRenderer>();
        if (spriteComponent != null)
        {
            spriteComponent.sprite = sprite;
            float targetSize = 1f;
            Vector2 spriteSize = sprite.bounds.size;
            float scaleFactor = targetSize / Mathf.Max(spriteSize.x, spriteSize.y);
            
            // Counteract parent scaling by applying inverse scale
            Vector3 parentScale = orderDisplayParent.lossyScale;
            orderSpriteObject.transform.localScale = new Vector3(
                scaleFactor / parentScale.x,
                scaleFactor / parentScale.y,
                1f
            );
        }

        spriteComponent.sortingLayerName = "Item";
        spriteComponent.sortingOrder = 3;
    }

    private void SetBubbleSortingLayer(GameObject bubble, string sortingLayerName)
{
    if (bubble != null)
    {
        SpriteRenderer bubbleRenderer = bubble.GetComponent<SpriteRenderer>();
        if (bubbleRenderer != null)
        {
            bubbleRenderer.sortingLayerName = sortingLayerName;
        }
    }
}
}