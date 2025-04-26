using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DrinkPlacerController : MonoBehaviour, IInteractable
{
    private bool isPlayerInCollision = false;
    private int drinkPrice = 0;
    public GameObject indicator;
    private bool hasDrinkOnTable = false;
    public TMP_Text errorText;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Drink"))
        {
            isPlayerInCollision = true;
            indicator.SetActive(true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Drink"))
        {
            isPlayerInCollision = false;
            indicator.SetActive(false);
        }
    }

    public void Interact()
    {
        if (isPlayerInCollision)
        {
            MixerController mixerController = FindObjectOfType<MixerController>();
            if (mixerController != null && mixerController.HasDrink())
            {
                PlaceDrinkOnTable(mixerController);
            }
            else
            {
                Debug.Log("No drink to place on the table.");
            }
        }
    }

    // Modify PlaceDrinkOnTable to update player animation
    public void PlaceDrinkOnTable(MixerController mixerController)
    {
        GameObject drinkPrefab = mixerController.GetCurrentDrink();
        int drinkPrice = mixerController.GetDrinkPrice();

        if (drinkPrefab != null)
        {
            if (hasDrinkOnTable)
            {
                ShowErrorMessage("Table has been occupied!");
                Debug.Log("There is already a drink on the table.");
                return;
            }
            else
            {
                hasDrinkOnTable = true;
                // set the table as the parent of the drink
                drinkPrefab.transform.SetParent(transform);
                Debug.Log("Drink has been placed on the table!");
                indicator.SetActive(false);
                //set drink position to be above table
                Vector3 tableSurfacePosition = transform.position;
                tableSurfacePosition.y += 0.5f;
                drinkPrefab.transform.position = tableSurfacePosition;
                // set the drink price
                SetDrinkPrice(drinkPrice);

                // Clear the drink from the mixer controller
                mixerController.ClearDrink();
                mixerController.PlaceDrinkOnTable();

                // Update player animation directly to ensure it's immediate
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    player.UpdateDrinkAnimation(false);
                }

                Debug.Log("Drink has been placed on table!" + drinkPrice);
            }
        }
        else
        {
            Debug.Log("No drink found");
        }
    }

    //set drink price
    public void SetDrinkPrice(int price)
    {
        drinkPrice = price;
    }

    //get drink price
    public int GetDrinkPrice()
    {
        return drinkPrice;
    }

    public bool HasDrinkOnTable()
    {
        return hasDrinkOnTable;
    }

    public void TakeDrink()
    {
        hasDrinkOnTable = false;
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

}
