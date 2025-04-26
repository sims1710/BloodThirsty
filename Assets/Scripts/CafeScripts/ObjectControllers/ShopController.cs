using UnityEngine;
using UnityEngine.InputSystem;

public class ShopController : MonoBehaviour, IInteractable
{
    public ShopPanelController shopPanel;
    public GameObject indicator;
    private bool isPlayerInCollision = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInCollision = true;
            indicator.SetActive(true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInCollision = false;
            indicator.SetActive(false);
        }
    }

    public void Interact()
    {
        if (isPlayerInCollision)
        {
            Debug.Log("Player interacted with shop.");
            shopPanel.OpenShopPanel();
        }
    }
}
