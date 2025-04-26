using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("Health Bar Components")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient healthGradient;
    
    public MaxHealth maxHealthScriptableObject;
    private int maxHealth;

    [Header("Health Bar Settings")]
    [SerializeField] private float smoothSpeed = 0.1f;
    [SerializeField] private bool showHealthText = true;
    [SerializeField] private TextMeshProUGUI healthText;

    private float targetValue;
    private PlayerHealth playerHealth;

    void Start()
    {
        // Find the player health component
        playerHealth = FindObjectOfType<PlayerHealth>();
        maxHealth = maxHealthScriptableObject.GetCurrentHealth();
        if (playerHealth == null)
        {
            Debug.LogError("HealthBarUI: No PlayerHealth component found in scene!");
            return;
        }

        // Initialize the health bar
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = playerHealth.currentHealth;
            targetValue = playerHealth.currentHealth;

            // Set initial color
            if (fillImage != null)
            {
                fillImage.color = healthGradient.Evaluate(1f);
            }
        }

        // Update health text if enabled
        if (showHealthText && healthText != null)
        {
            healthText.text = $"{playerHealth.currentHealth}/{maxHealth}";
        }
    }

    void Update()
    {
        if (playerHealth == null || healthSlider == null) return;

        // Smoothly update the health bar value
        targetValue = playerHealth.currentHealth;
        healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, smoothSpeed * Time.deltaTime);

        // Update the fill color based on health percentage
        if (fillImage != null)
        {
            float healthPercentage = healthSlider.value / healthSlider.maxValue;
            fillImage.color = healthGradient.Evaluate(healthPercentage);
        }

        // Update health text if enabled
        if (showHealthText && healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(healthSlider.value)}/{maxHealth}";
        }
    }
}