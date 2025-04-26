using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxController : MonoBehaviour
{
    public int baseDamage = 50;

    [Header("Upgrade Settings")]
    [Tooltip("Current attack upgrade level scriptable object")]
    public WeaponAttack currentUpgrade;
    private bool isActive = false;

    // Reference to the player's sprite renderer to match hitbox orientation
    private SpriteRenderer playerSpriteRenderer;

    // Cache objects that are currently in contact with the hitbox
    private List<Collider2D> collidersInContact = new List<Collider2D>();

    // Collider references - support both types
    private Collider2D activeCollider;
    private BoxCollider2D boxCollider;
    private CircleCollider2D circleCollider;

    void Start()
    {
        // Get player sprite renderer (parent should be the player)
        playerSpriteRenderer = transform.parent.GetComponent<SpriteRenderer>();

        // Try to get both collider types
        boxCollider = GetComponent<BoxCollider2D>();
        circleCollider = GetComponent<CircleCollider2D>();

        // Set the active collider based on what's attached
        if (boxCollider != null)
        {
            activeCollider = boxCollider;
            boxCollider.isTrigger = true;
        }
        else if (circleCollider != null)
        {
            activeCollider = circleCollider;
            circleCollider.isTrigger = true;
        }
        else
        {
            // Default to box collider if none exists
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            activeCollider = boxCollider;
        }

        // Disable the collider at start
        if (activeCollider != null)
        {
            activeCollider.enabled = false;
        }

        // Logging of attack upgrade level
        if (currentUpgrade != null)
        {
            Debug.Log("Attack upgrade level: " + currentUpgrade);
        }
        else
        {
            Debug.Log("Attack upgrade not assigned");
        }
    }

    // Methods to configure the hitbox size/position
    public void SetPosition(Vector2 localPosition)
    {
        transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0);
    }

    public void SetSize(Vector2 size)
    {
        if (boxCollider != null)
        {
            boxCollider.size = size;
        }
        else if (circleCollider != null)
        {
            // For circle collider, use the average of width and height as radius
            circleCollider.radius = (size.x + size.y) / 4f;
        }
    }

    public void SetOffset(Vector2 offset)
    {
        if (boxCollider != null)
        {
            boxCollider.offset = offset;
        }
        else if (circleCollider != null)
        {
            circleCollider.offset = offset;
        }
    }

    // Add this method to set the rotation of the hitbox
    public void SetRotation(float zRotation)
    {
        transform.localRotation = Quaternion.Euler(0, 0, zRotation);
    }

    // Get player facing direction
    public bool IsPlayerFacingLeft()
    {
        return playerSpriteRenderer != null && playerSpriteRenderer.flipX;
    }

    // Calculate the actual damage based on current upgrade
    public int CalculateAttackDamage()
    {
        return currentUpgrade.GetAttackDamage();
    }

    public void Activate()
    {
        isActive = true;

        // Enable whichever collider is attached
        if (activeCollider != null)
        {
            activeCollider.enabled = true;
        }

        // Create a copy of the list to avoid modification during iteration
        List<Collider2D> collidersCopy = new List<Collider2D>(collidersInContact);

        // Iterate over the copy instead of the original list
        foreach (Collider2D collider in collidersCopy)
        {
            if (collider != null)
            {
                DealDamage(collider);
            }
        }

        // Debug which type of collider was activated
        string colliderType = boxCollider != null ? "Box Collider" :
                             (circleCollider != null ? "Circle Collider" : "Unknown");
        Debug.Log($"Activated {colliderType} hitbox on {gameObject.name}");
    }

    public void Deactivate()
    {
        isActive = false;

        // Disable whichever collider is attached
        if (activeCollider != null)
        {
            activeCollider.enabled = false;
        }

        // collidersInContact.Clear();
    }

    private void DealDamage(Collider2D collider)
    {
        // Check if the object has health/enemy component
        IEnemyHitHandler enemy = collider.GetComponent<IEnemyHitHandler>();
        if (enemy != null)
        {
            // Use the calculated damage value based on current upgrade
            int damageToApply = CalculateAttackDamage();
            enemy.TakeHit(damageToApply);
            Debug.Log($"Hit enemy with {damageToApply} damage (Level {currentUpgrade?.GetCurrentLevel() ?? 0})");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Filter out non-enemy objects if needed
        if (other.CompareTag("Enemy"))
        {
            collidersInContact.Add(other);

            if (isActive)
            {
                DealDamage(other);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (collidersInContact.Contains(other))
        {
            collidersInContact.Remove(other);
        }
    }
}

// Interface for objects that can take damage
public interface IEnemyHitHandler
{
    void TakeHit(int damage);
}
