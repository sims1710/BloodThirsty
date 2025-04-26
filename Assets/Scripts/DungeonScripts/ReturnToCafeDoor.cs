using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ReturnToCafeDoor : MonoBehaviour, IInteractable
{
    [Header("Return Settings")]
    [Tooltip("Name of the cafe scene")]
    public string cafeScene = "VampireCafe";
    public DayNight dayNight;
    public TextMeshProUGUI errorText;
    private bool isPlayerInRange = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    public void Interact()
    {
        if (isPlayerInRange)
        {
            if (dayNight.GetTime())
            {
                SceneManager.LoadScene(cafeScene);
            }
            else {
                errorText.text = "It is nighttime. Go hunt for ingredients!";
                StartCoroutine(CloseText());
            }
        }
    }

    private IEnumerator CloseText()
    {
        yield return new WaitForSeconds(2f);
        errorText.text = "";
    }
}
