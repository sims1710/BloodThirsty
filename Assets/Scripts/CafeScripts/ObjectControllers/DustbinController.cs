using UnityEngine;
using UnityEngine.InputSystem;

public class DustbinController : MonoBehaviour, IInteractable
{
    public GameObject dustbinUI;
    public GameObject indicator;
    public GameObject canvas;
    private bool isPlayerInCollision = false; 

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInCollision = true;
            indicator.SetActive(true);
            canvas.SetActive(true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInCollision = false;
            indicator.SetActive(false);
            canvas.SetActive(false);
        }
    }

    public void Interact()
    {
        //if player has drink and collide with dustbin
        if (isPlayerInCollision)
        {
            MixerController mixerController = FindObjectOfType<MixerController>();
            if (mixerController != null && mixerController.HasDrink())
            {
                GameObject drink = mixerController.GetCurrentDrink();
                if (drink != null)
                {
                    // Destroy the drink object
                    Destroy(drink);
                    Debug.Log("Drink destroyed: " + drink.name);
                    mixerController.ClearDrink();
                }
                else
                {
                    Debug.Log("No drink to destroy.");
                }
            }
        }
    }
}