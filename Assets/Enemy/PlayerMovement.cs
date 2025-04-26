using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Movement settings
    public float moveSpeed = 5f;

    // Reference to the sprite transform for flipping
    private Transform spriteTransform;
    private SPUM_Prefabs spumPrefab;

    // Animation indices
    public int idleAnimIndex = 0;
    public int moveAnimIndex = 0;

    // State tracking
    private bool isMoving = false;

    void Start()
    {
        // Try to find the SPUM prefab component if available
        spumPrefab = GetComponentInChildren<SPUM_Prefabs>();

        if (spumPrefab != null)
        {
            spriteTransform = spumPrefab.transform;

            // Initialize animations if using SPUM
            spumPrefab.OverrideControllerInit();
            if (!spumPrefab.allListsHaveItemsExist())
            {
                spumPrefab.PopulateAnimationLists();
            }

            // Play idle animation by default
            spumPrefab.PlayAnimation(PlayerState.IDLE, idleAnimIndex);
        }
        else
        {
            // If no SPUM prefab found, try to find any child with a sprite renderer
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                spriteTransform = sr.transform;
            }
            else
            {
                // Default to this object if no sprite found
                spriteTransform = transform;
            }
        }
    }

    void Update()
    {
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate movement
        Vector3 movement = new Vector3(horizontal, vertical, 0f).normalized;

        // Apply movement
        transform.position += movement * moveSpeed * Time.deltaTime;

        // Handle animation states
        if (spumPrefab != null)
        {
            bool wasMoving = isMoving;
            isMoving = movement.magnitude > 0.1f;

            // Only change animation state when needed
            if (isMoving != wasMoving)
            {
                if (isMoving)
                {
                    spumPrefab.PlayAnimation(PlayerState.MOVE, moveAnimIndex);
                }
                else
                {
                    spumPrefab.PlayAnimation(PlayerState.IDLE, idleAnimIndex);
                }
            }

            // Flip sprite based on horizontal movement
            if (horizontal != 0)
            {
                // Note: The sprite direction may need to be reversed depending on your asset
                if (horizontal > 0)
                {
                    spriteTransform.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    spriteTransform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
    }
}