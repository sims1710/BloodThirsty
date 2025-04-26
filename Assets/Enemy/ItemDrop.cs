using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class ItemDrop : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRadius = 2f;      // Distance player can pick up from
    public float dropRange = 1.5f;       // Range within which items can drop from enemies

    [Header("Push Settings")]
    public float pushSpeed = 0.5f;         // Speed at which items are pushed away when inventory is full
    public float pushHeight = 2f;        // Height of the bounce when items are pushed away

    public string itemId;                // Identifier for the item type
    public int value = 1;                // Value or amount of the item
    public string itemType;              // "Organ" or "Blood"
    public float baseScale = 2f;         // Base scale for the item

    private Transform playerTransform;
    private bool canBePickedUp = false;
    private float pickupDelay = 0.5f;    // Time before item can be picked up
    private InventoryManager inventoryManager;
    private UnlockedItems unlockedItems;
    private bool hasInitialized = false;

    // Bounce parameters
    private bool isBouncing = false;
    private Vector2 bounceDirection;
    private float bounceSpeed;
    private float bounceTotalHeight;

    private SpriteRenderer spriteRenderer;
    private string selectedOrgan; // Keep track of which organ this drop represents

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (hasInitialized) return;

        // Get or add sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Find the player and get managers
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            // Try to find InventoryManager in the scene
            inventoryManager = InventoryManager.Instance;
            if (inventoryManager == null)
            {
                Debug.LogError("ItemDrop: InventoryManager not found in scene! Make sure the InventoryManager is in the scene and properly configured.");
                // Try to find it again in a few seconds
                Invoke("TryFindInventoryManager", 1f);
            }
            else
            {
                Debug.Log($"ItemDrop: Found InventoryManager: {inventoryManager.gameObject.name}");
                unlockedItems = inventoryManager.unlockedItems;
            }

            if (itemType == "Organ")
            {
                baseScale = 0.13f; // Default organ scale
                HandleOrganSetup();
            }
            else if (itemType == "Blood")
            {
                baseScale = 2f; // Blood vial scale
            }
        }
        else
        {
            Debug.LogError("ItemDrop: Player GameObject not found");
            // Try to find the player again in a few seconds
            Invoke("TryFindPlayer", 1f);
        }

        // Prevent immediate pickup
        Invoke("EnablePickup", pickupDelay);
        hasInitialized = true;
    }

    private void TryFindInventoryManager()
    {
        inventoryManager = InventoryManager.Instance;
        if (inventoryManager != null)
        {
            Debug.Log($"ItemDrop: Found InventoryManager on retry: {inventoryManager.gameObject.name}");
            unlockedItems = inventoryManager.unlockedItems;
        }
        else
        {
            Debug.LogError("ItemDrop: Still could not find InventoryManager. Make sure the InventoryManager is in the scene and properly configured.");
        }
    }

    private void TryFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Initialize();
        }
        else
        {
            Debug.LogError("ItemDrop: Still could not find Player");
        }
    }

    private void HandleOrganSetup()
    {
        if (inventoryManager != null && inventoryManager.unlockedItems != null)
        {
            unlockedItems = inventoryManager.unlockedItems;
            if (unlockedItems != null)
            {
                var validOrgans = unlockedItems.GetUnlockedOrgans();
                if (validOrgans != null && validOrgans.Count > 0)
                {
                    // Select a random organ type and set its sprite
                    selectedOrgan = validOrgans[Random.Range(0, validOrgans.Count)];
                    itemId = selectedOrgan; // Set the itemId to match the organ type
                    Debug.Log($"ItemDrop: Selected organ type: {selectedOrgan}");

                    // Load sprite from Resources/Organs folder
                    string spritePath = "Organs/" + selectedOrgan.ToLower();
                    Sprite organSprite = Resources.Load<Sprite>(spritePath);
                    if (organSprite != null)
                    {
                        spriteRenderer.sprite = organSprite;
                        spriteRenderer.sortingLayerName = "Item";
                        spriteRenderer.sortingOrder = 1;

                        // Set base size based on organ type
                        switch (selectedOrgan.ToLower())
                        {
                            case "eyes":
                            case "finger":
                                baseScale = 0.01f;
                                break;
                            case "stomach":
                            case "heart":
                                baseScale = 0.01f;
                                break;
                            case "brain":
                            case "intestine":
                            case "kidney":
                                baseScale = 0.01f;
                                break;
                            default:
                                baseScale = 0.01f;
                                break;
                        }
                        transform.localScale = new Vector3(baseScale, baseScale, baseScale);
                        Debug.Log($"ItemDrop: Set {selectedOrgan} scale to {baseScale}");
                    }
                    else
                    {
                        Debug.LogError($"ItemDrop: No sprite found for organ at path: {spritePath}");
                    }
                }
                else
                {
                    Debug.LogError("ItemDrop: No valid organs found in unlockedItems");
                    // If no organs are unlocked, destroy this drop
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogError("ItemDrop: unlockedItems is null");
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogError("ItemDrop: InventoryManager or unlockedItems is null");
            Destroy(gameObject);
        }
    }

    public void InitializeBounce(Vector2 direction, float speed, float height)
    {
        // Generate a random angle between 0 and 360 degrees
        float randomAngle = Random.Range(0f, 360f);

        // Convert the angle to radians and create a direction vector
        float angleInRadians = randomAngle * Mathf.Deg2Rad;
        bounceDirection = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        bounceSpeed = speed;
        bounceTotalHeight = height;
        isBouncing = true;

        // Start the bounce animation
        StartCoroutine(DoubleBounceAnimation());
    }

    private IEnumerator DoubleBounceAnimation()
    {
        Debug.Log($"Starting bounce animation with base scale: {baseScale}");
        // First bounce
        float startTime = Time.time;
        float duration = 0.6f; // Time for the first bounce (slightly faster)
        Vector2 startPosition = transform.position;

        while (Time.time - startTime < duration)
        {
            float elapsed = Time.time - startTime;
            float normalizedTime = elapsed / duration; // 0 to 1

            // Calculate horizontal movement (slowing down over time)
            float horizontalFactor = Mathf.Pow(1f - normalizedTime, 0.7f);  // Ease out
            Vector2 horizontalOffset = bounceDirection * bounceSpeed * horizontalFactor * Time.deltaTime * 20f;

            // Calculate vertical movement (parabolic)
            float height = bounceTotalHeight * 4f * normalizedTime * (1f - normalizedTime);

            // Apply movement
            transform.position = new Vector3(
                transform.position.x + horizontalOffset.x,
                transform.position.y + horizontalOffset.y,
                transform.position.z
            );

            // Apply the bounce height as a scale modifier
            float bounceScale = baseScale * (1f + (height / bounceTotalHeight) * 0.3f);
            transform.localScale = new Vector3(bounceScale, bounceScale, bounceScale);

            yield return null;
        }

        // Store position after first bounce
        Vector2 secondStartPosition = transform.position;

        // Second, smaller bounce
        startTime = Time.time;
        duration = 0.4f; // Shorter duration for second bounce
        float secondBounceHeight = bounceTotalHeight * 0.3f; // 30% of original height
        float secondBounceSpeed = bounceSpeed * 0.3f; // 30% of original speed

        while (Time.time - startTime < duration)
        {
            float elapsed = Time.time - startTime;
            float normalizedTime = elapsed / duration; // 0 to 1

            // Calculate horizontal movement (slowing down over time)
            float horizontalFactor = Mathf.Pow(1f - normalizedTime, 0.7f);  // Ease out
            Vector2 horizontalOffset = bounceDirection * secondBounceSpeed * horizontalFactor * Time.deltaTime * 20f;

            // Calculate vertical movement (parabolic)
            float height = secondBounceHeight * 4f * normalizedTime * (1f - normalizedTime);

            // Apply movement
            transform.position = new Vector3(
                transform.position.x + horizontalOffset.x,
                transform.position.y + horizontalOffset.y,
                transform.position.z
            );

            // Apply the bounce height as a scale modifier
            float bounceScale = baseScale * (1f + (height / secondBounceHeight) * 0.3f);
            transform.localScale = new Vector3(bounceScale, bounceScale, bounceScale);

            yield return null;
        }

        // Reset scale to base size
        transform.localScale = new Vector3(baseScale, baseScale, baseScale);
        Debug.Log($"Bounce animation complete. Final scale: {transform.localScale}");

        isBouncing = false;
    }

    void EnablePickup()
    {
        canBePickedUp = true;
    }

    void Update()
    {
        if (playerTransform == null || !canBePickedUp || isBouncing) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        if (distance <= pickupRadius)
        {
            if (inventoryManager != null)
            {
                if (itemType == "Organ")
                {
                    HandleOrganPickup();
                }
                else if (itemType == "Blood")
                {
                    HandleBloodPickup();
                }
            }
            else
            {
                Debug.LogError("ItemDrop: Cannot pick up item - InventoryManager is null");
                // Try to find the inventory manager again
                TryFindInventoryManager();
            }
        }
    }

    private void HandleOrganPickup()
    {
        if (selectedOrgan != null && inventoryManager != null)
        {
            // Log current inventory state
            Debug.Log($"=== Attempting to pick up organ: {selectedOrgan} ===");
            Debug.Log($"Current organ capacity: {inventoryManager.GetOrgansCount()}/{inventoryManager.inventoryData.GetOrganCapacity()}");
            foreach (var item in inventoryManager.inventoryData.GetOrganInventory())
            {
                Debug.Log($"- {item.itemType}: {item.count}");
            }

            if (inventoryManager.IsWithinOrganCapacity())
            {
                Debug.Log($"ItemDrop: Picking up organ {selectedOrgan}");
                inventoryManager.AddOrgan(selectedOrgan, 1);

                // Log updated inventory state
                Debug.Log($"=== After picking up {selectedOrgan} ===");
                Debug.Log($"Updated organ capacity: {inventoryManager.GetOrgansCount()}/{inventoryManager.inventoryData.GetOrganCapacity()}");
                foreach (var item in inventoryManager.inventoryData.GetOrganInventory())
                {
                    Debug.Log($"- {item.itemType}: {item.count}");
                }

                // Show pickup display
                PickupDisplayPanel pickupDisplay = FindObjectOfType<PickupDisplayPanel>();
                if (pickupDisplay != null)
                {
                    pickupDisplay.DisplayPickup(selectedOrgan, "Organ");
                }
                else
                {
                    Debug.LogError("ItemDrop: PickupDisplayPanel not found");
                }

                PlayPickupEffect();
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("ItemDrop: Organ capacity full, pushing away");
                // Push the organ away when inventory is full
                Vector2 pushDirection = (transform.position - playerTransform.position).normalized;
                InitializeBounce(pushDirection, pushSpeed, pushHeight);
            }
        }
        else
        {
            Debug.LogError($"ItemDrop: Cannot pick up organ. SelectedOrgan: {selectedOrgan}, InventoryManager: {inventoryManager != null}");
        }
    }

    private void HandleBloodPickup()
    {
        if (itemId != null && inventoryManager != null)
        {
            // Log current inventory state
            Debug.Log($"=== Attempting to pick up blood vial: {itemId} ===");
            Debug.Log($"Current blood capacity: {inventoryManager.GetVialsCount()}/{inventoryManager.inventoryData.GetBloodCapacity()}");
            foreach (var item in inventoryManager.inventoryData.GetBloodInventory())
            {
                Debug.Log($"- {item.itemType}: {item.count}");
            }

            if (inventoryManager.IsWithinBloodCapacity())
            {
                Debug.Log($"ItemDrop: Picking up blood vial {itemId}");
                inventoryManager.AddBloodVial(itemId, 1);

                // Log updated inventory state
                Debug.Log($"=== After picking up {itemId} ===");
                Debug.Log($"Updated blood capacity: {inventoryManager.GetVialsCount()}/{inventoryManager.inventoryData.GetBloodCapacity()}");
                foreach (var item in inventoryManager.inventoryData.GetBloodInventory())
                {
                    Debug.Log($"- {item.itemType}: {item.count}");
                }

                // Show pickup display
                PickupDisplayPanel pickupDisplay = FindObjectOfType<PickupDisplayPanel>();
                if (pickupDisplay != null)
                {
                    pickupDisplay.DisplayPickup(itemId, "Blood");
                }
                else
                {
                    Debug.LogError("ItemDrop: PickupDisplayPanel not found");
                }

                PlayPickupEffect();
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("ItemDrop: Blood capacity full, pushing away");
                // Push the blood vial away when inventory is full
                Vector2 pushDirection = (transform.position - playerTransform.position).normalized;
                InitializeBounce(pushDirection, pushSpeed, pushHeight);
            }
        }
        else
        {
            Debug.LogError($"ItemDrop: Cannot pick up blood vial. ItemId: {itemId}, InventoryManager: {inventoryManager != null}");
        }
    }

    private void PlayPickupEffect()
    {
        // Optional: Play a sound
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);
        }

        // Optional: Create a particle effect
        ParticleSystem particles = GetComponent<ParticleSystem>();
        if (particles != null)
        {
            ParticleSystem newParticles = Instantiate(particles, transform.position, Quaternion.identity);
            newParticles.Play();
            Destroy(newParticles.gameObject, 2f); // Destroy after particles finish
        }
    }
}