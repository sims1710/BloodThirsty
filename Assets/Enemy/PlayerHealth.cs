using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public MaxHealth maxHealthScriptableObject;
    private int maxHealth;
    public int currentHealth { get; private set; }

    // Invincibility properties
    private bool isInvincible = false;
    public bool IsInvincible { get { return isInvincible; } }

    // Optional UI elements
    public UnityEngine.UI.Slider healthSlider;

    // Sound effects
    [Header("Sound Effects")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField][Range(0f, 1f)] private float hurtVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float deathVolume = 1f;

    // Animation parameters
    private const string HURT_TRIGGER = "Hurt";
    private const string DEATH_TRIGGER = "Death";
    private const string DEATH_BOOL = "isDead";

    private Animator animator;
    private AudioSource audioSource;
    private DeathScreenUI deathScreen;
    private bool isDead = false;

    void Start()
    {
        maxHealth = maxHealthScriptableObject.GetCurrentHealth();
        currentHealth = maxHealth;
        Debug.Log($"PlayerHealth: Initialized with {currentHealth}/{maxHealth} health");
        UpdateHealthUI();

        // Get the Animator component
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("PlayerHealth: No Animator component found on player!");
        }

        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Find the death screen UI
        deathScreen = FindObjectOfType<DeathScreenUI>();
        if (deathScreen == null)
        {
            Debug.LogWarning("PlayerHealth: No DeathScreenUI found in scene!");
        }
    }

    public void TakeDamage(int damage = 10)
    {
        // Don't take damage during invincibility or if already dead
        if (isInvincible || isDead)
        {
            Debug.Log("PlayerHealth: Damage blocked due to invincibility or death");
            return;
        }

        int previousHealth = currentHealth;
        currentHealth -= damage;
        Debug.Log($"PlayerHealth: Took {damage} damage. Health: {previousHealth} -> {currentHealth}");

        // Play hurt animation
        if (animator != null)
        {
            animator.SetTrigger(HURT_TRIGGER);
            Debug.Log("PlayerHealth: Triggered hurt animation");
        }

        // Play hurt sound
        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound, hurtVolume);
        }

        // Clamp health to prevent negative values
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Debug.Log("PlayerHealth: Player has died!");
            Die();
        }
        else
        {
            StartInvincibility(0.5f);
        }
    }

    public void Heal(int amount = 1)
    {
        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"PlayerHealth: Healed for {amount}. Health: {previousHealth} -> {currentHealth}");
    }

    public void StartInvincibility(float duration)
    {
        isInvincible = true;
        Debug.Log($"PlayerHealth: Invincibility started for {duration} seconds");

        // Use effect manager for visual feedback
        EffectManager.Instance.StartInvincibilityEffect(gameObject, duration);

        // Start coroutine to reset invincibility after duration
        StartCoroutine(ResetInvincibilityAfterDelay(duration));
    }

    private IEnumerator ResetInvincibilityAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        isInvincible = false;
        Debug.Log("PlayerHealth: Invincibility ended");
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
            Debug.Log($"PlayerHealth: UI updated - Health: {currentHealth}/{maxHealth} ({healthSlider.value * 100}%)");
        }
    }

    void Die()
    {
        if (isDead) return; // Prevent multiple death calls
        isDead = true;

        Debug.Log("PlayerHealth: Player death sequence started");

        // Trigger death animation
        if (animator != null)
        {
            // Try both trigger and bool methods for death animation
            animator.SetTrigger(DEATH_TRIGGER);
            animator.SetBool(DEATH_BOOL, true);

            // Start coroutine to freeze animation after it completes
            StartCoroutine(FreezeAfterDeathAnimation());

            Debug.Log("PlayerHealth: Triggered death animation");
        }

        // Play death sound
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound, deathVolume);
        }

        // Show death screen
        if (deathScreen != null)
        {
            deathScreen.ShowDeathScreen();
        }

        // Disable all movement-related components
        DisableMovementComponents();

        // Disable colliders
        DisableColliders();
    }

    private IEnumerator FreezeAfterDeathAnimation()
    {
        // Wait for the death animation to complete
        // You might need to adjust this time based on your animation length
        yield return new WaitForSeconds(2.0f);

        // Freeze the animation
        if (animator != null)
        {
            animator.speed = 0f;
            Debug.Log("PlayerHealth: Death animation frozen on last frame");
        }
    }

    private void DisableMovementComponents()
    {
        // Disable Rigidbody2D if it exists
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Disable any movement scripts and sprite flipping
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            // Skip this script and the animator
            if (script == this || script is Animator) continue;

            // Disable any script that might control movement or sprite flipping
            string scriptName = script.GetType().Name.ToLower();
            if (scriptName.Contains("move") ||
                scriptName.Contains("controller") ||
                scriptName.Contains("player") ||
                scriptName.Contains("character") ||
                scriptName.Contains("flip") ||
                scriptName.Contains("sprite"))
            {
                script.enabled = false;
            }
        }

        // Also check children for any sprite-related components
        MonoBehaviour[] childScripts = GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour script in childScripts)
        {
            string scriptName = script.GetType().Name.ToLower();
            if (scriptName.Contains("flip") || scriptName.Contains("sprite"))
            {
                script.enabled = false;
            }
        }

        // Lock the sprite's rotation and scale
        Transform spriteTransform = transform;
        if (spriteTransform != null)
        {
            // Store the current scale to prevent flipping
            Vector3 currentScale = spriteTransform.localScale;
            spriteTransform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    private void DisableColliders()
    {
        // Disable all colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        // Also disable colliders in children
        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in childColliders)
        {
            collider.enabled = false;
        }
    }
}
