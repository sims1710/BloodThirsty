using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeHitbox : MonoBehaviour
{
    public int damage = 10;
    public float activeTime = 0.2f; // How long the hitbox stays active

    [Header("Knockback Settings")]
    public bool appliesKnockback = true;
    public float knockbackForce = 20f;
    public float knockbackDistance = 10f;

    [Header("Special Attack Settings")]
    public bool hasSpecialAttackConfig = true;  // Whether to use different values for special attacks
    public int specialAttackDamage = 20;        // Special attack damage
    public float specialKnockbackForce = 30f;   // Special attack knockback force
    public float specialKnockbackDistance = 15f; // Special attack knockback distance

    private bool isActive = false;
    private Transform ownerTransform; // The enemy/character that owns this hitbox
    private SPUM_Prefabs spumPrefab;
    private Vector3 originalLocalPosition;
    private Vector3 originalLocalScale;
    private Collider2D ownerCollider;

    private void Start()
    {
        // Get parent transform (the enemy/owner)
        ownerTransform = transform.parent;

        // Find the SPUM_Prefabs component
        spumPrefab = ownerTransform.GetComponentInChildren<SPUM_Prefabs>();

        // Get the owner's collider
        ownerCollider = ownerTransform.GetComponent<Collider2D>();

        // Store original values
        originalLocalPosition = transform.localPosition;
        originalLocalScale = transform.localScale;

        // Start with hitbox disabled
        gameObject.SetActive(false);
    }

    public void UpdatePositionAndFlip()
    {
        if (spumPrefab == null) return;

        // Check if the sprite is flipped
        bool isFacingRight = spumPrefab.transform.localScale.x < 0;

        if (isFacingRight)
        {
            // Mirror position on X-axis
            transform.localPosition = new Vector3(
                -originalLocalPosition.x,
                originalLocalPosition.y,
                originalLocalPosition.z
            );

            // Flip the scale
            transform.localScale = new Vector3(
                -originalLocalScale.x,
                originalLocalScale.y,
                originalLocalScale.z
            );
        }
        else
        {
            // Restore original position and scale
            transform.localPosition = originalLocalPosition;
            transform.localScale = originalLocalScale;
        }
    }

    public void Activate(bool isSpecialAttack = false)
    {
        if (!isActive)
        {
            // Update position and flip before activating
            UpdatePositionAndFlip();

            // Apply special attack values if needed
            if (isSpecialAttack && hasSpecialAttackConfig)
            {
                // Store original values to restore later
                int originalDamage = damage;
                float originalKnockbackForce = knockbackForce;
                float originalKnockbackDistance = knockbackDistance;

                // Apply special attack values
                damage = specialAttackDamage;
                knockbackForce = specialKnockbackForce;
                knockbackDistance = specialKnockbackDistance;

                // Start deactivation coroutine with value restoration
                isActive = true;
                gameObject.SetActive(true);
                StartCoroutine(DeactivateWithRestore(originalDamage, originalKnockbackForce, originalKnockbackDistance));
            }
            else
            {
                // Normal attack - use standard deactivation
                isActive = true;
                gameObject.SetActive(true);
                StartCoroutine(DeactivateAfterDelay());
            }
        }
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(activeTime);
        isActive = false;
        gameObject.SetActive(false);
    }

    private IEnumerator DeactivateWithRestore(int originalDamage, float originalForce, float originalDistance)
    {
        yield return new WaitForSeconds(activeTime);

        // Restore original values
        damage = originalDamage;
        knockbackForce = originalForce;
        knockbackDistance = originalDistance;

        // Deactivate
        isActive = false;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger once while active
        if (!isActive) return;

        // Check if we hit the player
        if (other.CompareTag("Player"))
        {
            // Get player health component
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // Check if player is invincible
            if (playerHealth != null && playerHealth.IsInvincible)
            {
                Debug.Log("Player is invincible - no damage applied");
                return;
            }

            // Deal damage
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                playerHealth.StartInvincibility(1.0f);

                // Apply knockback
                if (appliesKnockback)
                {
                    Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        // Direction is from owner to player
                        Vector2 knockbackDir = (other.transform.position - ownerTransform.position).normalized;

                        if (playerRb.bodyType == RigidbodyType2D.Kinematic)
                        {
                            Vector2 targetPosition = (Vector2)other.transform.position +
                                                   (knockbackDir * knockbackDistance);
                            StartCoroutine(KinematicKnockback(other.transform, targetPosition));
                        }
                        else
                        {
                            playerRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                        }
                    }
                }

                // Add stun effect
                StunPlayer(other.gameObject, 0.1f);

                Debug.Log($"Player hit by melee attack for {damage} damage");
            }
        }
    }

    private IEnumerator KinematicKnockback(Transform target, Vector2 endPosition)
    {
        Vector2 startPosition = target.position;
        float duration = 0.2f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            target.position = Vector2.Lerp(startPosition, endPosition, Mathf.SmoothStep(0, 1, t));

            yield return null;
        }
    }

    private void StunPlayer(GameObject player, float duration)
    {
        MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
        List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            string scriptName = script.GetType().Name.ToLower();

            if (scriptName.Contains("move") ||
                scriptName.Contains("controller") ||
                scriptName.Contains("player") ||
                scriptName.Contains("character"))
            {
                script.enabled = false;
                disabledScripts.Add(script);
            }
        }

        EffectManager.Instance.StunPlayerWithRestore(disabledScripts, duration);
    }
}