using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class FridgeController : MonoBehaviour, IInteractable
{
    public GameObject fridgeUI;
    public GameObject indicator;
    public TMP_Text indicatorText;
    private bool isPlayerInCollision = false;
    public AudioClip fridgeOpenSound;
    public AudioClip fridgeCloseSound;
    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
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
            fridgeUI.SetActive(!fridgeUI.activeSelf);
            if (fridgeUI.activeSelf)
            {
                Time.timeScale = 0;
                source.PlayOneShot(fridgeOpenSound);
            }
            else
            {
                Time.timeScale = 1;
                source.PlayOneShot(fridgeCloseSound);
            }
        }
    }
}
