using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;
using TMPro;

public class MixerController : MonoBehaviour, IInteractable
{
    public IngredientPrices ingredientPrices;
    public HeldStorageManager heldStorageManager;
    private HashSet<string> heldItems = new HashSet<string>();
    private int drinkPrice = 0;
    private bool isPlayerInCollision = false;
    public GameObject drinkPrefab;
    public Transform playerTransform;
    public GameObject indicator;
    public TMP_Text indicatorText;
    private GameObject currentDrink;
    private List<string> mixedItems = new List<string>();
    private List<string> mixedBlood = new List<string>();
    private List<string> mixedOrgans = new List<string>();
    [SerializeField] private Material organMaterial;
    [SerializeField] private DrinkData drinkData;
    public TMP_Text errorText;
    private AudioSource audioSource;
    public AudioClip mixingSound;
    // Add a drinkHeld boolean to track state
    private bool drinkHeld = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void MixDrinks()
    {
        heldItems = new HashSet<string>(heldStorageManager.GetHeldItems());

        if (heldItems.Count == 0)
        {
            Debug.Log("No items to mix.");
            ShowErrorMessage("No items to mix.");
            return;
        }

        // Check if at least one blood and one organ are present
        bool hasBlood = heldItems.Any(item => ingredientPrices.IsBlood(item));
        bool hasOrgan = heldItems.Any(item => ingredientPrices.IsOrgan(item));

        if (!hasBlood || !hasOrgan)
        {
            ShowErrorMessage("Need at least 1 blood and 1 organ to mix.");
            Debug.Log("You need at least one blood and one organ to mix.");
            return;
        }

        // Calculate the total price of the drink
        drinkPrice = CalculateDrinkPrice(heldItems.ToList());
        foreach (var item in heldItems)
        {
            Debug.Log($"Has Drink item: {item}");
        }
        //store drink items for order validation
        mixedItems = heldItems.ToList();
        Debug.Log($"Mixed items: {string.Join(", ", mixedItems)}");

        heldStorageManager.RemoveAllHeldItems(); // Clear the held items after mixing

        Debug.Log($"Mixing drinks with the following items:");

        Debug.Log($"Total drink price: {drinkPrice}");
        if (drinkPrefab != null && playerTransform != null)
        {
            //create drink prefab
            CreateDrinkPrefab();

            // //attach the drink to the vampire parent
            // currentDrink = Instantiate(drinkPrefab, playerTransform.position, Quaternion.identity);
            // currentDrink.transform.SetParent(playerTransform);
            // currentDrink.transform.localPosition = new Vector3(1.08f, 0.76f, -0.2f);
            // Debug.Log("Drink has been created and attached to the player!");

            // Set drinkHeld flag
            drinkHeld = true;

            // Update player animation
            PlayerController player = playerTransform.GetComponent<PlayerController>();
            if (player != null)
            {
                player.UpdateDrinkAnimation(true);
            }
        }

    }

    private void CreateDrinkPrefab()
    {
        string bloodType = mixedItems.FirstOrDefault(item => ingredientPrices.IsBlood(item));
        //List<string> organTypes = mixedItems.Where(item => ingredientPrices.IsOrgan(item)).ToList();
        List<string> organTypes = mixedItems
        .Where(item => ingredientPrices.IsOrgan(item))
        .Distinct()
        .ToList();

        // Get player's sprite renderer to check facing direction
        SpriteRenderer playerSprite = playerTransform.GetComponent<SpriteRenderer>();
        bool facingLeft = playerSprite != null && playerSprite.flipX;

        // instantiate the drink prefab
        currentDrink = Instantiate(drinkPrefab, playerTransform.position, Quaternion.identity);
        //attach drink to the player
        currentDrink.transform.SetParent(playerTransform);

        // Position drink based on player's facing direction
        float xOffset = facingLeft ? -1.08f : 1.08f;
        currentDrink.transform.localPosition = new Vector3(xOffset, 0.76f, -0.2f);

        Debug.Log($"Created drink with player facing {(facingLeft ? "left" : "right")}, xOffset: {xOffset}");

        // setting the blood sprite
        SpriteRenderer bloodRenderer = currentDrink.GetComponent<SpriteRenderer>();
        if (bloodRenderer != null && bloodType != null)
        {
            Sprite bloodSprite = drinkData.GetBloodSprite(bloodType);
            if (bloodSprite != null)
            {
                bloodRenderer.sprite = bloodSprite;
            }

            // Also flip the drink sprite when player faces left
            bloodRenderer.flipX = facingLeft;
        }
        float organOffset = 0.06f;
        int organIndex = 1;
        
        // adding organ sprites to the drink prefab
        foreach (var organ in organTypes)
        {
            Debug.Log("ORGAN: " + organ);
            float zOffset = -0.1f - (0.01f * organIndex);
            Sprite organSprite = drinkData.GetOrganSprite(organ);
            Debug.Log($"Retrieved sprite for organ '{organ}': {organSprite}");
            if (organSprite != null)
            {
                GameObject organObject = new GameObject(organ);
                organObject.transform.SetParent(currentDrink.transform);
                organObject.transform.localScale = new Vector3(0.001f, 0.001f, 1f);
                // organObject.transform.localPosition = new Vector3(0f, 0f, -0.1f);
                organObject.transform.localPosition = new Vector3(
                    0f, 
                    (organOffset *  organIndex) - 0.1f,
                    zOffset
                );
                SpriteRenderer organRenderer = organObject.AddComponent<SpriteRenderer>();
                organRenderer.sprite = organSprite;
                //set layer otherwise cannot see the organ
                organRenderer.sortingLayerName = "TopDecor";
                organRenderer.sortingOrder = 3 + organIndex;
                if (organMaterial != null)
                {
                    organRenderer.material = organMaterial;
                }
                organObject.SetActive(false);
                organObject.SetActive(true);
                organIndex++;
            }
        }
    }


    private int CalculateDrinkPrice(List<string> items)
    {
        int totalPrice = 0;
        string bloodColor = items.FirstOrDefault(item => ingredientPrices.IsBlood(item));

        foreach (var item in items)
        {
            if (ingredientPrices.IsOrgan(item))
            {
                int organPrice = ingredientPrices.GetPrice(item);
                totalPrice += Mathf.RoundToInt(organPrice);
            }
        }
        float multiplier = ingredientPrices.GetMultiplier(bloodColor);
        totalPrice = Mathf.RoundToInt(totalPrice * multiplier);

        return totalPrice;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInCollision = true;
            indicator.SetActive(true);
            indicatorText.color = Color.white;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInCollision = false;
            indicator.SetActive(false);
            indicatorText.color = Color.black;
        }
    }

    public void Interact()
    {
        if (isPlayerInCollision)
        {
            MixDrinks();
            audioSource.PlayOneShot(mixingSound);

        }
        else
        {
            Debug.Log("Player did not collide");
        }
    }

    //helper methods
    // Update HasDrink to use the tracking boolean
    public bool HasDrink()
    {
        return drinkHeld && currentDrink != null;
    }

    public GameObject GetCurrentDrink()
    {
        return currentDrink;
    }

    // Update ClearDrink to set drinkHeld to false
    public void ClearDrink()
    {
        currentDrink = null;
        drinkHeld = false;

        // Update player animation if possible
        if (playerTransform != null)
        {
            PlayerController player = playerTransform.GetComponent<PlayerController>();
            if (player != null)
            {
                player.UpdateDrinkAnimation(false);
            }
        }
    }

    //retrieve drink price
    public int GetDrinkPrice()
    {
        return drinkPrice;
    }

    //retrieve mixed items
    public List<string> GetMixedItems()
    {
        return mixedItems;
    }

    private void ShowErrorMessage(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
        Invoke("HideErrorText", 2f);
    }

    private void HideErrorText()
    {
        errorText.gameObject.SetActive(false);
    }

    // Add a method to allow DrinkPlacerController to notify when drink is placed
    public void PlaceDrinkOnTable()
    {
        drinkHeld = false;
    }
}
