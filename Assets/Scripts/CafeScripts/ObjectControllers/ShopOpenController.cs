using UnityEngine;
using UnityEngine.InputSystem;

public class ShopOpenController : MonoBehaviour, IInteractable
{
    public GameObject shopPanelUI;
    public GameObject indicator;
    public GameObject shopClosedUI;
    public GameObject shopOpenUI;
    [SerializeField]
    private SpawnSettings spawnSettings;
    private bool isPlayerInCollision = false; 
    private bool openShop = false; 
    [SerializeField]
    private RandomWalker randomWalker;
    private CountdownTimer countdownTimer;

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
        countdownTimer = FindObjectOfType<CountdownTimer>();
        //if player has drink and collide with shop
        if (isPlayerInCollision && !openShop)
        {
            openShop = true;
            shopOpenUI.SetActive(true);
            shopClosedUI.SetActive(false);
            if (openShop)
            {
                randomWalker.StartSpawning();
                countdownTimer.StartTimer();
            }
            else
            {
                randomWalker.StopSpawning();
                spawnSettings.currentAmount = 0;
                countdownTimer.StopTimer();
            }
        }
    }

    public void CloseShopUI()
    {
        openShop = false;
        shopOpenUI.SetActive(false);
        shopClosedUI.SetActive(true);
        randomWalker.StopSpawning();
        spawnSettings.currentAmount = 0;
    }
}