using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class CashierController : MonoBehaviour, IInteractable
{
    private bool isPlayerInCollision = false;
    [SerializeField]
    private QueueManager queueManager;
    public AudioSource cashierAudio;
    public AudioClip cashierSound;
    public BiteCoins biteCoins;
    public GameObject indicator;
    public GameObject amountReceived;

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
        if (isPlayerInCollision && queueManager.QueueNotEmpty())
        {
            Debug.Log("Process Queue");
            int totalEarnings = queueManager.ProcessQueue();
            if (biteCoins != null)
            {
                biteCoins.AddAmount(totalEarnings);
                StartCoroutine(DisplayPrice(totalEarnings));
            }
            cashierAudio.PlayOneShot(cashierSound);
        }
        else
        {
            Debug.Log("Player did not collide");
        }
    }

    private IEnumerator DisplayPrice(int amount)
    {
        TextMeshProUGUI textComponent = amountReceived.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = $"+{amount}";
            amountReceived.SetActive(true);
            yield return new WaitForSeconds(3f);
            amountReceived.SetActive(false);
        }
    }
}