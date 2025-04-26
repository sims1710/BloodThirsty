using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PriceMenuController : MonoBehaviour, IInteractable
{
    public GameObject priceMenuUI;
    public GameObject indicator;
    public TMP_Text indicatorText;
    private bool isPlayerInCollision = false; 

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
            priceMenuUI.SetActive(!priceMenuUI.activeSelf);
            if (priceMenuUI.activeSelf)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1; 
            }
        }
    }
}