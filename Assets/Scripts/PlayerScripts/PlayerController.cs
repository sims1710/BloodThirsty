using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeForce = 10f;
    [SerializeField] private float dodgeCooldown = 0.5f;
    [SerializeField] private float dodgeDuration = 0.4f;
    public bool dungeonWorld = true;
    private bool isDodging = false;
    private Rigidbody2D rb;
    private bool canDodge = true;
    private float lastDodgeTime;
    private float dodgeEndTime;
    public GameObject EPanel;
    private PlayerInput playerInputActions;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isMoving = false;
    private bool canMove = true;
    private bool canAttack = true; // Track if player can attack

    // If you're using a generated class called "PlayerInput" from your input actions asset
    private Vector2 moveDirection = Vector2.zero;
    private HitboxController attackHitbox;
    private Dictionary<string, IAttackStrategy> attackStrategies;
    private IInteractable currentInteractable;
    private bool isHandlingExternalForce = false;
    private float externalForceEndTime = 0f;

    [Header("Sound Settings")]
    private AudioSource audioSource;
    [SerializeField] public AudioClip magicSound;
    [SerializeField] public AudioClip secondMagicSound;

    private GameObject secondaryHitbox;

    [Header("Attack Hitboxes")]
    [SerializeField] private GameObject hitboxParent;         // Parent object containing all hitboxes
    [SerializeField] private GameObject horizontalHitbox;     // Side attack hitbox
    [SerializeField] private GameObject verticalHitbox;     // Up attack hitbox

    [Header("Cafe Specific")]
    private bool isHoldingDrink = false;
    private MixerController mixerController;

    void Awake()
    {
        // Initialize the PlayerInputActions
        playerInputActions = new PlayerInput();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        isMoving = false;
        canMove = true;
        animator.SetBool("Moving", isMoving);

        // Initialise attack strategies
        attackStrategies = new Dictionary<string, IAttackStrategy>
        {
            { "horizontal", new HorizontalAttackStrategy() },
            { "vertical" , new VerticalAttackStrategy() }
        };
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Find the hitbox reference
        attackHitbox = GetComponentInChildren<HitboxController>();
        if (attackHitbox == null)
        {
            Debug.LogWarning("No HitboxController found in children. Hitbox functionality will be disabled.");
        }

        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Get circle collider
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();

        if (circleCollider != null)
        {
            // Offset the collider downward (negative Y) relative to the GameObject
            circleCollider.offset = new Vector2(0, -1.2f);
        }

        // Find or create hitbox parent if not assigned
        if (hitboxParent == null)
        {
            hitboxParent = transform.Find("HitboxParent")?.gameObject;

            if (hitboxParent == null)
            {
                hitboxParent = new GameObject("HitboxParent");
                hitboxParent.transform.SetParent(transform);
                hitboxParent.transform.localPosition = Vector3.zero;
            }
        }

        // Find mixer controller reference
        if (mixerController == null)
        {
            mixerController = FindObjectOfType<MixerController>();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "VampireCafe" || scene.name == "DungeonHub")
        {
            dungeonWorld = false;

            // Disable dungeon actions
            playerInputActions.Player.Attack.Disable();
            playerInputActions.Player.VerticalAttack.Disable();

            // Enable cafe-specific actions
            playerInputActions.Player.Interact.Enable();
        }
        else
        {
            dungeonWorld = true;

            // Enable dungeon actions
            playerInputActions.Player.Attack.Enable();
            playerInputActions.Player.VerticalAttack.Enable();

            // Disable cafe actions
            playerInputActions.Player.Interact.Disable();
        }

        // If we're returning from a dungeon to cafe, might need to restore state
        if (scene.name == "VampireCafe" && PlayerPrefs.HasKey("LastScene"))
        {
            // Restore any saved player state
            // ...

            // Clear the saved scene info
            PlayerPrefs.DeleteKey("LastScene");
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        // Get the input vector (WASD or arrow keys)
        moveDirection = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        // Reset the move direction when keys are released
        moveDirection = Vector2.zero;
    }

    private void OnDodgePerformed(InputAction.CallbackContext context)
    {
        PerformDodge();
    }

    private void PerformDodge()
    {
        // Check if can dodge and not in inventory (with null check for inventory panel)
        if (!canDodge || isDodging || (EPanel != null && EPanel.activeSelf)) return;

        // Get dodge direction (use movement direction or facing direction)
        Vector2 dodgeDirection = moveDirection.normalized;

        // If player isn't moving, dodge in the direction they're facing
        if (dodgeDirection.magnitude < 0.1f)
        {
            dodgeDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        // Set dodge state
        isDodging = true;
        dodgeEndTime = Time.time + dodgeDuration;

        // Apply dodge force as impulse
        rb.linearVelocity = Vector2.zero; // Reset velocity before dodge
        rb.AddForce(dodgeDirection * dodgeForce, ForceMode2D.Impulse);

        // Trigger dodge animation if available
        if (animator != null)
        {
            animator.SetTrigger("Dodge");
        }

        // Set cooldown
        canDodge = false;
        lastDodgeTime = Time.time;

        Debug.Log("Dodge performed in direction: " + dodgeDirection);
    }

    private void PerformAttack(IAttackStrategy attackStrategy)
    {
        if (canAttack)
        {
            // Common attack set up
            animator.SetTrigger(attackStrategy.GetAnimationTrigger());
            canAttack = false;
            canMove = false;

            // Play sound
            AudioClip sound = attackStrategy.GetAttackSound(this);
            if (audioSource != null && sound != null)
            {
                audioSource.clip = sound;
                audioSource.Play();
            }

            // Execute strategy specific logic
            attackStrategy.ExecuteAttack(this);
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        PerformAttack(attackStrategies["horizontal"]);
    }

    private void OnVerticalAttackPerformed(InputAction.CallbackContext input)
    {
        PerformAttack(attackStrategies["vertical"]);
    }

    // This method should be called at the end of your attack animation via Animation Event
    public void OnAttackAnimationComplete()
    {
        // Allow attacking again
        canAttack = true;

        // Enable moving
        canMove = true;
    }

    private void OnToggleInventory(InputAction.CallbackContext context)
    {
        if (EPanel == null) return;

        bool isActive = !EPanel.activeSelf;
        EPanel.SetActive(isActive);
        Time.timeScale = isActive ? 0 : 1;
        if (isActive)
        {
            playerInputActions.Player.Movement.Disable();
        }
        else
        {
            playerInputActions.Player.Movement.Enable();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object is interactable
        IInteractable interactable = collision.gameObject.GetComponent<IInteractable>();
        Debug.Log("Collision Entered with: " + collision.gameObject.name);
        if (interactable != null)
        {
            currentInteractable = interactable;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Clear the current interactable when exiting the collision
        IInteractable interactable = collision.gameObject.GetComponent<IInteractable>();
        if (interactable != null && interactable == currentInteractable)
        {
            currentInteractable = null;
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        // Interact with the current interactable object
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    // Helper method to check if movement keys are pressed
    private bool IsMovementPressed()
    {
        // Check if any movement key is pressed
        return playerInputActions.Player.Movement.IsPressed();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if dodge has ended
        if (isDodging && Time.time >= dodgeEndTime)
        {
            isDodging = false;
        }

        // Check if external force handling has ended
        if (isHandlingExternalForce && Time.time >= externalForceEndTime)
        {
            isHandlingExternalForce = false;

            // When external force ends, check if player should be moving
            if (!IsMovementPressed())
            {
                rb.linearVelocity = Vector2.zero;
                moveDirection = Vector2.zero;
            }
        }

        // If player is attacking, ensure they don't move
        if (!canMove && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // Apply movement in Update
            Move();
        }

        UpdateSpriteDirection();

        // Reset dodge cooldown
        if (!canDodge && Time.time > lastDodgeTime + dodgeCooldown)
        {
            canDodge = true;
        }

        // Check for drink status (only in cafe world)
        if (!dungeonWorld && Time.frameCount % 15 == 0)  // Check every ~0.25 seconds
        {
            if (mixerController != null)
            {
                bool hasDrink = mixerController.HasDrink();
                if (hasDrink != isHoldingDrink)
                {
                    UpdateDrinkAnimation(hasDrink);
                }
            }
        }
    }

    void Move()
    {
        // Don't move during dodge or external force
        if (isDodging || isHandlingExternalForce) return;

        // Only apply velocity if there's input AND movement keys are pressed
        if (IsMovementPressed() && moveDirection.magnitude > 0.1f)
        {
            // Apply the movement to the rigidbody
            rb.linearVelocity = moveDirection * moveSpeed;

            // Update animator state
            if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerAttack"))
            {
                isMoving = true;
                animator.SetBool("Moving", isMoving);
            }
        }
        else
        {
            // No movement input, stop the player
            rb.linearVelocity = Vector2.zero;

            // Update animator state
            if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName("PlayerAttack"))
            {
                isMoving = false;
                animator.SetBool("Moving", isMoving);
            }
        }
    }

    // Update the UpdateSpriteDirection method to also update drink position
    void UpdateSpriteDirection()
    {
        // If moving horizontally, update the sprite direction
        if (moveDirection.x != 0)
        {
            // Get old facing direction first
            bool wasFlipped = spriteRenderer.flipX;

            // Flip the sprite based on horizontal movement direction
            spriteRenderer.flipX = moveDirection.x < 0;

            // If the direction changed and player is holding a drink, update drink position
            if (wasFlipped != spriteRenderer.flipX && isHoldingDrink && !dungeonWorld)
            {
                UpdateDrinkPosition();
            }
        }
    }

    // Add this new method to reposition the drink
    void UpdateDrinkPosition()
    {
        // Find the drink as a child of the player
        Transform drinkTransform = null;
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("Drink"))
            {
                drinkTransform = child;
                break;
            }
        }

        // If drink found, update its position
        if (drinkTransform != null)
        {
            bool facingLeft = spriteRenderer.flipX;
            float xOffset = facingLeft ? -1.08f : 1.08f;
            drinkTransform.localPosition = new Vector3(xOffset, 0.76f, -0.2f);

            // Also flip the drink sprite
            SpriteRenderer drinkSprite = drinkTransform.GetComponent<SpriteRenderer>();
            if (drinkSprite != null)
            {
                drinkSprite.flipX = facingLeft;
            }

            Debug.Log($"Updated drink position for facing {(facingLeft ? "left" : "right")}");
        }
    }

    public void UpdateDrinkAnimation(bool holdingDrink)
    {
        isHoldingDrink = holdingDrink;

        if (animator != null)
        {
            Debug.Log($"Setting HasDrink animation to: {holdingDrink}");
            animator.SetBool("HasDrink", holdingDrink);
        }
    }

    private void OnEnable()
    {
        // Enable input actions
        if (playerInputActions != null)
        {
            playerInputActions.Player.Dodge.Enable();
            playerInputActions.Player.Movement.Enable();
            playerInputActions.Player.Inventory.Enable();
            if (dungeonWorld)
            {
                playerInputActions.Player.Attack.Enable();
                playerInputActions.Player.VerticalAttack.Enable();
            }
            else
            {
                playerInputActions.Player.Interact.Enable();
            }
        }

        // Subscribe to input actions
        playerInputActions.Player.Movement.performed += OnMovePerformed;
        playerInputActions.Player.Movement.canceled += OnMoveCanceled;
        playerInputActions.Player.Dodge.performed += OnDodgePerformed;
        playerInputActions.Player.Inventory.performed += OnToggleInventory;
        if (dungeonWorld)
        {
            playerInputActions.Player.Attack.performed += OnAttackPerformed;
            playerInputActions.Player.VerticalAttack.performed += OnVerticalAttackPerformed;
        }
        else
        {
            playerInputActions.Player.Interact.performed += OnInteractPerformed;
        }

        // Register for scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void OnDisable()
    {
        // Disable input actions and unsubscribe
        if (playerInputActions != null)
        {
            playerInputActions.Player.Movement.performed -= OnMovePerformed;
            playerInputActions.Player.Movement.canceled -= OnMoveCanceled;
            playerInputActions.Player.Attack.performed -= OnAttackPerformed;
            playerInputActions.Player.VerticalAttack.performed -= OnVerticalAttackPerformed;
            playerInputActions.Player.Inventory.performed -= OnToggleInventory;
            playerInputActions.Player.Dodge.performed -= OnDodgePerformed; // Unsubscribe from dodge action
            playerInputActions.Player.Interact.performed -= OnInteractPerformed;
            playerInputActions.Player.Movement.Disable();
            playerInputActions.Player.Attack.Disable(); // Don't forget to disable Attack action
            playerInputActions.Player.VerticalAttack.Disable();
            playerInputActions.Player.Inventory.Disable();
            playerInputActions.Player.Dodge.Disable();
            playerInputActions.Player.Interact.Disable();
        }

        // Unregister from scene load events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // New method that activates the correct directional hitboxes
    public void ActivateVerticalHitboxes()
    {
        // First deactivate horizontal hitbox (if it exists)
        if (horizontalHitbox != null)
        {
            horizontalHitbox.SetActive(false);
        }

        // Activate both vertical hitboxes
        if (verticalHitbox != null)
        {
            verticalHitbox.SetActive(true);

            // Get the hitbox controller and activate it
            HitboxController upController = verticalHitbox.GetComponent<HitboxController>();
            if (upController != null)
            {
                upController.Activate();
            }
        }


        Debug.Log("Vertical hitboxes activated");
    }

    // New method for activating horizontal hitbox
    public void ActivateHorizontalHitbox()
    {
        // Deactivate vertical hitboxes
        if (verticalHitbox != null)
        {
            verticalHitbox.SetActive(false);
        }

        // Activate horizontal hitbox
        if (horizontalHitbox != null)
        {
            horizontalHitbox.SetActive(true);

            // Adjust the position based on player facing direction
            bool facingLeft = spriteRenderer.flipX;
            horizontalHitbox.transform.localPosition = new Vector3(
                facingLeft ? -3f : 3f,
                -1f,
                0f
            );

            // Get the hitbox controller and activate it
            HitboxController horizontalController = horizontalHitbox.GetComponent<HitboxController>();
            if (horizontalController != null)
            {
                horizontalController.Activate();
            }
        }

        Debug.Log("Horizontal hitbox activated");
    }

    // animation event
    public void DeactivateHitbox()
    {
        Debug.Log("DeactivateHitbox called (animation event)");

        // Call Deactivate instead of just SetActive(false)
        if (verticalHitbox != null)
        {
            HitboxController vertController = verticalHitbox.GetComponent<HitboxController>();
            if (vertController != null)
            {
                vertController.Deactivate();
            }
            else
            {
                verticalHitbox.SetActive(false);
            }
        }

        if (horizontalHitbox != null)
        {
            HitboxController horizController = horizontalHitbox.GetComponent<HitboxController>();
            if (horizController != null)
            {
                horizController.Deactivate();
            }
            else
            {
                horizontalHitbox.SetActive(false);
            }
        }
    }
}

public interface IAttackStrategy
{
    string GetAnimationTrigger();
    void ExecuteAttack(PlayerController player);
    AudioClip GetAttackSound(PlayerController player);
}

public class HorizontalAttackStrategy : IAttackStrategy
{
    public string GetAnimationTrigger() => "Attack";

    public void ExecuteAttack(PlayerController player)
    {
        // Use the new method to activate the horizontal hitbox
        player.ActivateHorizontalHitbox();
    }

    public AudioClip GetAttackSound(PlayerController player) => player.magicSound;
}

public class VerticalAttackStrategy : IAttackStrategy
{
    public string GetAnimationTrigger() => "VerticalAttack";

    public void ExecuteAttack(PlayerController player)
    {
        // Use the new method to activate vertical hitboxes
        player.ActivateVerticalHitboxes();
    }

    public AudioClip GetAttackSound(PlayerController player) => player.secondMagicSound;
}
