using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DeathScreenUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image blackOverlay;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float textDelay = 1f;
    [SerializeField] private string deathMessage = "You Died";
    [SerializeField] private Button button;


    private void Start()
    {
        // Ensure the overlay and text are hidden at start
        if (blackOverlay != null)
        {
            blackOverlay.color = new Color(0, 0, 0, 0);
            blackOverlay.gameObject.SetActive(false);
        }
        if (deathText != null)
        {
            deathText.color = new Color(1, 1, 1, 0);
            deathText.gameObject.SetActive(false);
        }
        if (button != null)
        {
            button.gameObject.SetActive(false);
        }
    }

    public void ShowDeathScreen()
    {
        StartCoroutine(DeathScreenSequence());
    }

    private IEnumerator DeathScreenSequence()
    {
        // Activate the overlay
        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            // Fade in the black overlay
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                float alpha = Mathf.Lerp(0f, 0.8f, elapsedTime / fadeDuration);
                blackOverlay.color = new Color(0, 0, 0, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            blackOverlay.color = new Color(0, 0, 0, 0.8f);
        }

        // Wait before showing text
        yield return new WaitForSeconds(textDelay);

        // Show and fade in the death text
        if (deathText != null)
        {
            deathText.gameObject.SetActive(true);
            deathText.text = deathMessage;
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                deathText.color = new Color(1, 1, 1, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            deathText.color = new Color(1, 1, 1, 1f);
        }

        yield return new WaitForSeconds(textDelay);
        if (button != null)
        {
            button.gameObject.SetActive(true);
        }
    }

    public void Respawn()
    {
        SceneManager.LoadScene("DungeonHub");
    }
}