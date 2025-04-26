using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    public float lifetime = 5f; // Destroy after this many seconds
    public float damage = 10f; // Amount of damage this projectile deals

    [Header("Collision Settings")]
    public LayerMask collisionLayers; // Set this in inspector to include layers the projectile should collide with

    [Header("Knockback Settings")]
    public bool appliesKnockback = true;  // Toggle for enabling/disabling knockback
    public float knockbackForce = 20f;    // Force for non-kinematic rigidbodies
    public float knockbackDistance = 10f; // Distance for kinematic rigidbodies

    public void Initialize(Vector3 dir, float spd)
    {
        direction = dir;
        speed = spd;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit the player
        if (other.CompareTag("Player"))
        {
            // Get player health component first
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // Check if player is currently invincible
            if (playerHealth != null && playerHealth.IsInvincible)
            {
                // Player is in iframes - no damage or knockback
                Debug.Log("Player is invincible - no damage applied");
                Destroy(gameObject);
                return;
            }

            // Deal damage to player
            if (playerHealth != null)
            {
                playerHealth.TakeDamage((int)damage);

                // Make player invincible after hit
                playerHealth.StartInvincibility(1.0f);

                // Apply knockback with much stronger force
                Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                if (playerRb != null && appliesKnockback)  // Only apply knockback if the flag is true
                {
                    // Use projectile's direction for knockback
                    Vector2 knockbackDir = direction.normalized;

                    // Check bodyType
                    if (playerRb.bodyType == RigidbodyType2D.Kinematic)
                    {
                        Vector2 targetPosition = (Vector2)other.transform.position +
                                                (knockbackDir * knockbackDistance);

                        StartCoroutine(KinematicKnockback(other.transform, targetPosition));
                        Debug.Log("Applied kinematic knockback");
                    }
                    else
                    {
                        playerRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                        Debug.Log("Applied force knockback");
                    }
                }
                else
                {
                    Debug.LogWarning("Player has no Rigidbody2D for knockback");
                }

                // Flash player red
                // FlashPlayerRed(other.gameObject);

                // Add stun effect
                StunPlayer(other.gameObject, 0.1f);
            }

            // Destroy the projectile
            Destroy(gameObject);
        }
        // Check if we hit anything in our collision layers
        else if (((1 << other.gameObject.layer) & collisionLayers.value) != 0)
        {
            Destroy(gameObject);
        }
    }

    // private void FlashPlayerRed(GameObject player)
    // {
    //     // Get all sprite renderers on the player
    //     SpriteRenderer[] renderers = player.GetComponentsInChildren<SpriteRenderer>();

    //     if (renderers.Length == 0)
    //     {
    //         Debug.LogWarning("No SpriteRenderers found on player for color flash");
    //     }

    //     foreach (SpriteRenderer renderer in renderers)
    //     {
    //         // Store original color properly
    //         Color originalColor = renderer.color;
    //         // Use effect manager instead of local coroutine
    //         EffectManager.Instance.FlashColor(renderer, Color.red, originalColor, 0.1f);
    //     }
    // }

    private IEnumerator KinematicKnockback(Transform target, Vector2 endPosition)
    {
        Vector2 startPosition = target.position;
        float duration = 0.2f; // How long the knockback takes (adjust as needed)
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Smoothed movement
            target.position = Vector2.Lerp(startPosition, endPosition, Mathf.SmoothStep(0, 1, t));

            yield return null;
        }
    }

    private void StunPlayer(GameObject player, float duration)
    {
        // Try to find the player's movement script(s)
        MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();

        List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            string scriptName = script.GetType().Name.ToLower();

            // Check for common movement script names
            if (scriptName.Contains("move") ||
                scriptName.Contains("controller") ||
                scriptName.Contains("player") ||
                scriptName.Contains("character"))
            {
                // Disable the script
                script.enabled = false;
                disabledScripts.Add(script);
                Debug.Log($"Disabled script during stun: {script.GetType().Name}");
            }
        }

        // Use effect manager instead of local coroutine
        EffectManager.Instance.StunPlayerWithRestore(disabledScripts, duration);
    }
}

public class EffectManager : MonoBehaviour
{
    // Singleton instance
    private static EffectManager instance;

    public static EffectManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("EffectManager");
                instance = go.AddComponent<EffectManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    // public void FlashColor(SpriteRenderer renderer, Color flashColor, Color originalColor, float duration)
    // {
    //     StartCoroutine(FlashRoutine(renderer, flashColor, originalColor, duration));
    // }

    public void StunPlayerWithRestore(List<MonoBehaviour> scripts, float duration)
    {
        StartCoroutine(ReenableScriptsAfterStun(scripts, duration));
    }

    public void StartInvincibilityEffect(GameObject player, float duration)
    {
        // Get all sprite renderers
        SpriteRenderer[] renderers = player.GetComponentsInChildren<SpriteRenderer>();

        // Store original colors
        Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
        foreach (SpriteRenderer renderer in renderers)
        {
            originalColors[renderer] = renderer.color;
        }

        // Start flashing coroutine
        StartCoroutine(InvincibilityFlashRoutine(renderers, originalColors, duration));
    }

    private IEnumerator FlashRoutine(SpriteRenderer renderer, Color flashColor, Color originalColor, float duration)
    {
        if (renderer != null)
        {
            renderer.color = flashColor;
            yield return new WaitForSeconds(duration);
            if (renderer != null) // Check again in case the object was destroyed
            {
                renderer.color = originalColor;
            }
        }
    }

    private IEnumerator ReenableScriptsAfterStun(List<MonoBehaviour> scripts, float duration)
    {
        yield return new WaitForSeconds(duration);

        foreach (MonoBehaviour script in scripts)
        {
            if (script != null) // Check in case object was destroyed
            {
                script.enabled = true;
            }
        }
    }

    private IEnumerator InvincibilityFlashRoutine(SpriteRenderer[] renderers, Dictionary<SpriteRenderer, Color> originalColors, float duration)
    {
        float elapsedTime = 0f;
        float flashRate = 0.1f; // Flash every 0.1 seconds
        bool isRed = false;

        while (elapsedTime < duration)
        {
            // Toggle between red and white
            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.color = isRed ? Color.white : Color.red;
                }
            }

            isRed = !isRed;
            elapsedTime += flashRate;
            yield return new WaitForSeconds(flashRate);
        }

        // Ensure all renderers return to white at the end
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.color = Color.white;
            }
        }
    }
}