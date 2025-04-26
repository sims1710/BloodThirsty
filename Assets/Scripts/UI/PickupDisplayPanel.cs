using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PickupDisplayPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI pickupText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Vector2 slideOffset = new Vector2(-50f, 0f);

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Coroutine displayCoroutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;

        // Hide the panel initially
        canvasGroup.alpha = 0f;
    }

    public void DisplayPickup(string itemName, string itemType)
    {
        // Stop any existing display coroutine
        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);

        // Load the appropriate sprite based on item type and name
        string spritePath = itemType == "Organ" ? $"Organs/{itemName.ToLower()}" : $"BloodPotions/{itemName.ToLower()}blood";
        Sprite itemSprite = Resources.Load<Sprite>(spritePath);

        if (itemSprite != null)
        {
            itemImage.sprite = itemSprite;
            itemImage.preserveAspect = true;
        }
        else
        {
            Debug.LogError($"Could not load sprite at path: {spritePath}");
        }

        // Set the pickup text
        pickupText.text = itemType == "Blood"
            ? $"Added {itemName} blood to inventory!"
            : $"Added {itemName} to inventory!";

        // Start the display coroutine
        displayCoroutine = StartCoroutine(DisplayPickupCoroutine());
    }

    private IEnumerator DisplayPickupCoroutine()
    {
        // Reset position and show the panel
        rectTransform.anchoredPosition = originalPosition + slideOffset;
        canvasGroup.alpha = 0f;

        // Slide in and fade in
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;

            canvasGroup.alpha = t;
            rectTransform.anchoredPosition = Vector2.Lerp(
                originalPosition + slideOffset,
                originalPosition,
                t
            );

            yield return null;
        }

        // Wait for display duration
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;

            canvasGroup.alpha = 1f - t;
            rectTransform.anchoredPosition = Vector2.Lerp(
                originalPosition,
                originalPosition - slideOffset,
                t
            );

            yield return null;
        }

        // Ensure the panel is hidden
        canvasGroup.alpha = 0f;
    }
}