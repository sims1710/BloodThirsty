using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Implements IEnemyHitHandler
public class EnemyController : MonoBehaviour, IEnemyHitHandler
{
    // References
    private SPUM_Prefabs spumPrefab;
    private Transform playerTransform;
    private Rigidbody2D rb; // Reference to rigidbody for physics‑based movement

    // Health and Damage Settings
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    // Configuration
    public float detectionRange = 8f;
    public float attackRange = 1.5f;
    public float moveSpeed = 2f;
    public float attackCooldown = 1.5f; // Base cooldown between attacks
    public KeyCode hurtTestKey = KeyCode.H; // Press to test hurt animation
    public KeyCode deathTestKey = KeyCode.K; // Press to test death animatio

    // State tracking
    private PlayerState currentState = PlayerState.IDLE;
    private float attackTimer = 0f;
    private bool isHurt = false;
    private float hurtTimer = 0f;
    private float hurtDuration = 0.5f;
    private bool isDead = false;
    private bool attackFinishing = false;
    private float attackFinishTimer = 0f;
    private float attackFinishDelay = 0.1f; // Small delay to detect end of attack
    private bool attackEventTriggered = false;

    // Attack Type Configuration
    [Header("Attack Type Settings")]
    public bool isRangedAttacker = false;
    public bool isMagicAttacker = false;
    public float projectileSpeed = 5f;
    public float rangedAttackRange = 5f; // Typically larger than melee attack range
    public GameObject arrowPrefab; // Assign in inspector for ranged attackers
    public GameObject magicOrbPrefab; // Assign in inspector for magic attackers

    [Header("Projectile Spawn Offset")]
    public float projectileOffsetX = 0.5f; // Positive value for right offset, negative for left
    public float projectileOffsetY = 0f;   // Adjust height of spawn point if needed

    [Header("Special Attack Settings")]
    public bool hasSpecialAttack = true;
    public int minAttacksBeforeSpecial = 3;
    public int maxAttacksBeforeSpecial = 5;
    public int specialAnimIndex = 0;
    public GameObject specialProjectilePrefab; // Special projectile for ranged/magic attackers
    public float specialProjectileSpeed = 7f;  // Can be faster/slower than regular projectiles

    // Track attacks for special attack timing
    private int attacksSinceSpecial = 0;
    private int attacksUntilSpecial;

    // Add this field near your other configuration variables
    [Header("Attack Timing")]
    public float normalAttackAnimationDelay = 0.5f;  // Base delay for normal attacks
    public float specialAttackAnimationDelay = 0.7f; // Base delay for special attacks
    public float normalProjectileOffset = 0.1f;      // Additional delay after sound before projectile spawns
    public float specialProjectileOffset = 0.15f;    // Additional delay for special projectile spawn

    // Add these near your other range configurations
    [Header("Melee Attack Range Settings")]
    public bool useRectangularRange = false;  // Toggle for melee characters
    public float attackRangeX = 1.5f;        // Horizontal range (narrower)
    public float attackRangeY = 3f;          // Vertical range (taller)
    public float attackRangeYOffset = -1f;   // Offset the Y range downward (negative value) or upward (positive value)

    // Add these near the top of your class with other configuration
    [Header("Drop System")]
    public bool dropsItems = true;
    [SerializeField] private UnlockedItems unlockedItems; // Reference to UnlockedItems ScriptableObject
    [Range(0, 5)] public int minOrganDrops = 0;
    [Range(0, 5)] public int maxOrganDrops = 2;
    [Range(0, 5)] public int minBloodDrops = 2;
    [Range(0, 5)] public int maxBloodDrops = 5;
    [Range(0.1f, 5f)] public float dropRange = 1.5f;      // How far items can drop from the enemy

    // Track which items have been dropped by this enemy
    private List<string> droppedItemTypes = new List<string>();

    [Header("Combat Settings")]
    public int attackDamage = 10;       // Damage dealt to player
    public float attackKnockback = 2f;  // Force applied to player when hit
    public LayerMask playerLayer;       // Set to player's layer in inspector

    // Add this field with your other declarations
    [Header("Melee Settings")]
    public MeleeHitbox meleeHitbox; // Assign in inspector

    [Header("Contact Damage Settings")]
    public bool doesContactDamage = true;     // Toggle for contact damage
    public int contactDamage = 5;             // Damage on contact (usually less than attack damage)
    public float contactKnockbackForce = 15f; // Knockback force for contact
    public float contactKnockbackDistance = 5f; // Knockback distance for kinematic bodies
    public float contactDamageInterval = 0.3f; // Minimum time between contact damage hits

    private float lastContactDamageTime = 0f;  // Track when we last did contact damage

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip normalAttackSound;    // Renamed from attackSound
    [SerializeField] private AudioClip specialAttackSound;   // New special attack sound
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField][Range(0f, 1f)] private float normalAttackVolume = 1f;    // Renamed from attackVolume
    [SerializeField][Range(0f, 1f)] private float specialAttackVolume = 1f;   // New special attack volume
    [SerializeField][Range(0f, 1f)] private float hurtVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float deathVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float normalAttackSoundDelay = 0f;    // Renamed from attackSoundDelay
    [SerializeField][Range(0f, 1f)] private float specialAttackSoundDelay = 0f;   // New special attack delay

    private Coroutine currentAttackSoundCoroutine = null;  // Add this field at the top with other private fields
    private bool attackSoundPlayed = false;          // Track if sound has played for current attack

    private bool isWandering = false;
    private Coroutine wanderCoroutine;

    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Get the SPUM_Prefabs component
        spumPrefab = GetComponentInChildren<SPUM_Prefabs>();
        if (spumPrefab == null)
        {
            Debug.LogError("No SPUM_Prefabs component found in children!");
            return;
        }

        // Setup URP for all child components
        SetupURPComponents();

        // Make sure animations are initialized - SIMPLIFIED
        if (!spumPrefab.allListsHaveItemsExist())
        {
            spumPrefab.PopulateAnimationLists();
        }

        // No more OverrideControllerInit call here!

        // Reset any animator parameters
        ResetAnimatorState();
        // Find the player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("No GameObject with tag 'Player' found!");
        }

        // Adjust attack range if this is a ranged attacker
        if (isRangedAttacker || isMagicAttacker)
        {
            attackRange = rangedAttackRange;
        }

        // Validate projectile settings
        if (isRangedAttacker && arrowPrefab == null)
        {
            Debug.LogError("Ranged attacker needs an arrow prefab assigned!");
        }
        if (isMagicAttacker && magicOrbPrefab == null)
        {
            Debug.LogError("Magic attacker needs a magic orb prefab assigned!");
        }

        // Initialize special attack counter
        ResetSpecialAttackCounter();

        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Cache Rigidbody2D for physics movement
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Configure rigidbody for dynamic physics
        rb.mass = 100f; // Increased mass for more weight
        rb.linearDamping = 2f; // Increased drag for more resistance to movement
        rb.angularDamping = 2f; // Increased angular drag to prevent spinning
        rb.gravityScale = 0f; // No gravity for top-down / platformer enemies
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smoother movement
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep; // Always active
        rb.freezeRotation = true; // Prevent unwanted rotation
        rb.bodyType = RigidbodyType2D.Dynamic; // Ensure dynamic for collision response
    }

    private void SetupURPComponents()
    {
        // Get all renderers in children (including inactive ones)
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in allRenderers)
        {
            SetupRendererURP(renderer);
        }

        // Get all sprite renderers specifically (in case they were missed)
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            SetupRendererURP(renderer);
        }

        // Get all particle system renderers
        ParticleSystemRenderer[] particleRenderers = GetComponentsInChildren<ParticleSystemRenderer>(true);
        foreach (ParticleSystemRenderer renderer in particleRenderers)
        {
            SetupRendererURP(renderer);
        }

        // Get all mesh renderers
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer renderer in meshRenderers)
        {
            SetupRendererURP(renderer);
        }

        // Get all skinned mesh renderers
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
        {
            SetupRendererURP(renderer);
        }

        // Get all line renderers
        LineRenderer[] lineRenderers = GetComponentsInChildren<LineRenderer>(true);
        foreach (LineRenderer renderer in lineRenderers)
        {
            SetupRendererURP(renderer);
        }

        // Get all trail renderers
        TrailRenderer[] trailRenderers = GetComponentsInChildren<TrailRenderer>(true);
        foreach (TrailRenderer renderer in trailRenderers)
        {
            SetupRendererURP(renderer);
        }
    }

    private void SetupRendererURP(Renderer renderer)
    {
        if (renderer == null) return;

        // Set URP properties for all renderers
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;

        // Handle materials
        Material[] materials = renderer.sharedMaterials;
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null) continue;

            // Check if material is already URP
            if (!materials[i].shader.name.Contains("Universal"))
            {
                // Create new URP material based on renderer type
                Material urpMaterial = null;
                if (renderer is SpriteRenderer)
                {
                    urpMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
                }
                else if (renderer is ParticleSystemRenderer)
                {
                    urpMaterial = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                }
                else if (renderer is LineRenderer || renderer is TrailRenderer)
                {
                    urpMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                }
                else
                {
                    urpMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                }

                if (urpMaterial != null)
                {
                    // Copy properties from old material
                    urpMaterial.mainTexture = materials[i].mainTexture;
                    urpMaterial.color = materials[i].color;

                    // Only copy properties that exist in the source material
                    if (materials[i].HasProperty("_Surface")) urpMaterial.SetFloat("_Surface", materials[i].GetFloat("_Surface"));
                    if (materials[i].HasProperty("_Blend")) urpMaterial.SetFloat("_Blend", materials[i].GetFloat("_Blend"));
                    if (materials[i].HasProperty("_AlphaClip")) urpMaterial.SetFloat("_AlphaClip", materials[i].GetFloat("_AlphaClip"));
                    if (materials[i].HasProperty("_Cutoff")) urpMaterial.SetFloat("_Cutoff", materials[i].GetFloat("_Cutoff"));
                    if (materials[i].HasProperty("_Smoothness")) urpMaterial.SetFloat("_Smoothness", materials[i].GetFloat("_Smoothness"));
                    if (materials[i].HasProperty("_Metallic")) urpMaterial.SetFloat("_Metallic", materials[i].GetFloat("_Metallic"));
                    if (materials[i].HasProperty("_BumpScale")) urpMaterial.SetFloat("_BumpScale", materials[i].GetFloat("_BumpScale"));
                    if (materials[i].HasProperty("_Parallax")) urpMaterial.SetFloat("_Parallax", materials[i].GetFloat("_Parallax"));
                    if (materials[i].HasProperty("_OcclusionStrength")) urpMaterial.SetFloat("_OcclusionStrength", materials[i].GetFloat("_OcclusionStrength"));
                    if (materials[i].HasProperty("_ClearCoatMask")) urpMaterial.SetFloat("_ClearCoatMask", materials[i].GetFloat("_ClearCoatMask"));
                    if (materials[i].HasProperty("_ClearCoatSmoothness")) urpMaterial.SetFloat("_ClearCoatSmoothness", materials[i].GetFloat("_ClearCoatSmoothness"));

                    // Copy textures if they exist
                    if (materials[i].HasProperty("_MainTex")) urpMaterial.SetTexture("_MainTex", materials[i].GetTexture("_MainTex"));
                    if (materials[i].HasProperty("_BumpMap")) urpMaterial.SetTexture("_BumpMap", materials[i].GetTexture("_BumpMap"));
                    if (materials[i].HasProperty("_MetallicGlossMap")) urpMaterial.SetTexture("_MetallicGlossMap", materials[i].GetTexture("_MetallicGlossMap"));
                    if (materials[i].HasProperty("_OcclusionMap")) urpMaterial.SetTexture("_OcclusionMap", materials[i].GetTexture("_OcclusionMap"));
                    if (materials[i].HasProperty("_EmissionMap")) urpMaterial.SetTexture("_EmissionMap", materials[i].GetTexture("_EmissionMap"));
                    if (materials[i].HasProperty("_DetailMask")) urpMaterial.SetTexture("_DetailMask", materials[i].GetTexture("_DetailMask"));
                    if (materials[i].HasProperty("_DetailAlbedoMap")) urpMaterial.SetTexture("_DetailAlbedoMap", materials[i].GetTexture("_DetailAlbedoMap"));
                    if (materials[i].HasProperty("_DetailNormalMap")) urpMaterial.SetTexture("_DetailNormalMap", materials[i].GetTexture("_DetailNormalMap"));

                    // Set the new material
                    materials[i] = urpMaterial;
                }
                else
                {
                    Debug.LogWarning($"Failed to create URP material for {renderer.gameObject.name}");
                }
            }
        }

        // Apply the materials back to the renderer
        renderer.sharedMaterials = materials;
    }

    void ResetAnimatorState()
    {
        // Reset all important animator parameters
        if (spumPrefab._anim != null)
        {
            spumPrefab._anim.SetBool("isDeath", false);

            // Reset all triggers
            foreach (AnimatorControllerParameter param in spumPrefab._anim.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                {
                    spumPrefab._anim.ResetTrigger(param.name);
                }
            }
        }
    }

    void ResetSpecialAttackCounter()
    {
        if (hasSpecialAttack)
        {
            attacksUntilSpecial = Random.Range(minAttacksBeforeSpecial, maxAttacksBeforeSpecial + 1);
        }
    }

    void Update()
    {

        // If dead, don't process any other inputs or states
        if (isDead)
            return;

        // Check for death test key
        if (Input.GetKeyDown(deathTestKey))
        {
            TriggerDeath();
            return; // Skip other processing when dead
        }

        // Check for hurt test key
        if (Input.GetKeyDown(hurtTestKey) && !isHurt)
        {
            TriggerHurt();
        }

        // Update hurt state
        if (isHurt)
        {
            hurtTimer += Time.deltaTime;
            if (hurtTimer >= hurtDuration)
            {
                isHurt = false;
                hurtTimer = 0f;
                // Return to previous state
                DetermineStateByDistance();
            }
            return; // Skip other processing while hurt
        }

        // Check if attack is finishing and transition to idle
        if (attackFinishing)
        {
            attackFinishTimer += Time.deltaTime;
            if (attackFinishTimer >= attackFinishDelay)
            {
                attackFinishing = false;
                attackFinishTimer = 0f;

                // Then determine what to do next
                DetermineStateByDistance();
            }
            return; // Skip other processing while transitioning
        }
        if (playerTransform == null)
        {
            // Try to find player again if missing
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                // Player not found, stay in idle
                if (currentState != PlayerState.IDLE)
                {
                    currentState = PlayerState.IDLE;
                    spumPrefab.PlayAnimation(PlayerState.IDLE, 0);
                }
                return;
            }
        }

        // Update attack timer
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // Check for next attack key
        DetermineStateByDistance();
    }

    void DetermineStateByDistance()
    {
        // Skip state determination if currently attacking
        if (currentState == PlayerState.ATTACK && attackTimer > 0)
        {
            // Even while attacking, keep facing the player if ranged
            if (isRangedAttacker || isMagicAttacker)
            {
                FacePlayer();
            }
            return;
        }

        // Check if player is dead
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null && playerHealth.currentHealth <= 0)
        {
            // Player is dead, start wandering if not already
            if (!isWandering)
            {
                isWandering = true;
                wanderCoroutine = StartCoroutine(WanderBehavior());
            }
            return;
        }

        // Stop wandering if player is alive
        if (isWandering)
        {
            isWandering = false;
            if (wanderCoroutine != null)
            {
                StopCoroutine(wanderCoroutine);
            }
        }

        // Always use circular distance for detection range
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Only use rectangular range for attack checks
        bool inAttackRange = false;
        if (useRectangularRange && !isRangedAttacker && !isMagicAttacker)
        {
            // Calculate X and Y distances separately
            float xDistance = Mathf.Abs(transform.position.x - playerTransform.position.x);
            float yDistance = Mathf.Abs((transform.position.y + attackRangeYOffset) - playerTransform.position.y);

            // In attack range if both X and Y are within bounds
            inAttackRange = xDistance <= attackRangeX && yDistance <= attackRangeY;
        }
        else
        {
            inAttackRange = distanceToPlayer <= attackRange;
        }

        if (inAttackRange)
        {
            // Always face the player when in attack range
            FacePlayer();

            if (attackTimer <= 0)
            {
                attackEventTriggered = false; // Reset the trigger flag

                // Check if it's time for a special attack
                if (hasSpecialAttack && attacksSinceSpecial >= attacksUntilSpecial)
                {
                    // Update the current state to OTHER for special attacks
                    currentState = PlayerState.OTHER;
                    spumPrefab.PlayAnimation(PlayerState.OTHER, specialAnimIndex);
                    StartCoroutine(TriggerAttackEvent());

                    // Reset counter
                    attacksSinceSpecial = 0;
                    ResetSpecialAttackCounter();
                }
                else
                {
                    currentState = PlayerState.ATTACK;
                    spumPrefab.PlayAnimation(PlayerState.ATTACK, 0);
                    StartCoroutine(TriggerAttackEvent());
                    attacksSinceSpecial++;
                }

                attackTimer = attackCooldown;
            }
        }
        else if (distanceToPlayer <= detectionRange)
        {
            // In detection range but not attack range - move towards player
            if (currentState != PlayerState.MOVE)
            {
                currentState = PlayerState.MOVE;
                spumPrefab.PlayAnimation(PlayerState.MOVE, 0);
            }

            // Move towards player only if not attacking
            if (currentState != PlayerState.ATTACK)
            {
                MoveTowardsPlayer();
            }
        }
        else
        {
            // Out of range - idle
            if (currentState != PlayerState.IDLE)
            {
                currentState = PlayerState.IDLE;
                spumPrefab.PlayAnimation(PlayerState.IDLE, 0);
            }

            // Even in idle, ranged attackers should face the player if in detection range
            if ((isRangedAttacker || isMagicAttacker) && distanceToPlayer <= detectionRange)
            {
                FacePlayer();
            }
        }

        // If we're in IDLE or ATTACK states, ensure we stop moving
        if ((currentState == PlayerState.IDLE || currentState == PlayerState.ATTACK) && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private IEnumerator WanderBehavior()
    {
        while (isWandering)
        {
            // Walk for 2-4 seconds
            float walkDuration = Random.Range(2f, 4f);
            float walkTimer = 0f;

            // Choose a random direction
            Vector2 randomDirection = Random.insideUnitCircle.normalized;

            // Set walking state
            currentState = PlayerState.MOVE;
            spumPrefab.PlayAnimation(PlayerState.MOVE, 0);

            // Face the movement direction
            if (randomDirection.x > 0)
            {
                spumPrefab.transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (randomDirection.x < 0)
            {
                spumPrefab.transform.localScale = new Vector3(1, 1, 1);
            }

            // Walk in the chosen direction
            while (walkTimer < walkDuration)
            {
                transform.position += (Vector3)randomDirection * moveSpeed * Time.deltaTime;
                walkTimer += Time.deltaTime;
                yield return null;
            }

            // Idle for 1-3 seconds
            float idleDuration = Random.Range(1f, 3f);
            float idleTimer = 0f;

            // Set idle state
            currentState = PlayerState.IDLE;
            spumPrefab.PlayAnimation(PlayerState.IDLE, 0);

            while (idleTimer < idleDuration)
            {
                idleTimer += Time.deltaTime;
                yield return null;
            }
        }
    }

    IEnumerator TriggerAttackEvent()
    {
        bool isSpecialAttack = (currentState == PlayerState.OTHER);
        float baseDelay = isSpecialAttack ? specialAttackAnimationDelay : normalAttackAnimationDelay;
        float projectileOffset = isSpecialAttack ? specialProjectileOffset : normalProjectileOffset;

        AudioClip soundToPlay = isSpecialAttack ? specialAttackSound : normalAttackSound;
        float soundVolume = isSpecialAttack ? specialAttackVolume : normalAttackVolume;
        float soundDelay = isSpecialAttack ? specialAttackSoundDelay : normalAttackSoundDelay;

        attackSoundPlayed = false;

        // Wait for initial sound delay if any
        if (soundDelay > 0)
        {
            yield return new WaitForSeconds(soundDelay);
        }

        // Check if we've been interrupted
        if (isDead || isHurt) yield break;

        // Play the sound
        if (soundToPlay != null)
        {
            PlaySound(soundToPlay, soundVolume);
            attackSoundPlayed = true;
        }

        // Wait for the projectile offset after sound
        if (projectileOffset > 0)
        {
            yield return new WaitForSeconds(projectileOffset);
        }

        // Check if we've been interrupted
        if (isDead || isHurt) yield break;

        // Only proceed with attack if sound has played (or if there was no sound to play)
        if (attackSoundPlayed || soundToPlay == null)
        {
            if (!isDead && !isHurt && !attackEventTriggered &&
                (currentState == PlayerState.ATTACK || currentState == PlayerState.OTHER))
            {
                attackEventTriggered = true;
                OnAttackAnimationEvent();
            }
        }
    }

    void TriggerHurt()
    {
        // Cancel any pending attack sound
        if (currentAttackSoundCoroutine != null)
        {
            StopCoroutine(currentAttackSoundCoroutine);
            currentAttackSoundCoroutine = null;
        }
        attackSoundPlayed = false;  // Reset sound played flag

        // Cancel any current actions
        attackTimer = 0f;

        isHurt = true;
        hurtTimer = 0f;
        currentState = PlayerState.DAMAGED;

        // Reset all triggers first to force interrupt
        foreach (AnimatorControllerParameter param in spumPrefab._anim.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                spumPrefab._anim.ResetTrigger(param.name);
            }
        }

        // Find the right hurt/damaged trigger parameter
        bool foundDamagedParam = false;
        foreach (AnimatorControllerParameter param in spumPrefab._anim.parameters)
        {
            if (param.name.Contains("Damage") || param.name.Contains("damage") ||
                param.name.Contains("Hurt") || param.name.Contains("hurt"))
            {
                // Found a damage-related parameter
                if (param.type == AnimatorControllerParameterType.Trigger)
                {
                    spumPrefab._anim.SetTrigger(param.name);
                    Debug.Log($"Using damage parameter: {param.name}");
                    foundDamagedParam = true;
                    break;
                }
            }
        }

        if (!foundDamagedParam)
        {
            // Fallback - try to play damaged animation directly
            Debug.LogWarning("No damaged parameter found, attempting direct animation play");
            if (spumPrefab.DAMAGED_List.Count > 0)
            {
                spumPrefab._anim.Play(spumPrefab.DAMAGED_List[0].name, 0, 0f);
            }
        }

        // Play hurt sound
        if (hurtSound != null)
        {
            PlaySound(hurtSound, hurtVolume);
        }

    }

    void TriggerDeath()
    {
        // Cancel any pending attack sound
        if (currentAttackSoundCoroutine != null)
        {
            StopCoroutine(currentAttackSoundCoroutine);
            currentAttackSoundCoroutine = null;
        }
        attackSoundPlayed = false;  // Reset sound played flag

        // Set dead state
        isDead = true;
        currentState = PlayerState.DEATH;
        spumPrefab._anim.SetBool("isDeath", true);

        // Disable the capsule collider
        CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = false;
            Debug.Log("Enemy capsule collider disabled on death");
        }
        else
        {
            Debug.LogWarning("No CapsuleCollider2D found on enemy to disable");
        }

        // Zero out all rigidbody physics
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.simulated = false; // This will stop all physics simulation
            Debug.Log("Enemy rigidbody physics disabled on death");
        }
        else
        {
            Debug.LogWarning("No Rigidbody2D found on enemy to disable physics");
        }

        // Reset all triggers first to force interrupt
        foreach (AnimatorControllerParameter param in spumPrefab._anim.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                spumPrefab._anim.ResetTrigger(param.name);
            }
        }

        // Find the right death trigger parameter
        bool foundDeathParam = false;
        foreach (AnimatorControllerParameter param in spumPrefab._anim.parameters)
        {
            if (param.name.Contains("Death") || param.name.Contains("death"))
            {
                // Found a death-related parameter
                if (param.type == AnimatorControllerParameterType.Trigger)
                {
                    spumPrefab._anim.SetTrigger(param.name);
                    Debug.Log($"Using death parameter: {param.name}");
                    foundDeathParam = true;
                    break;
                }
                else if (param.type == AnimatorControllerParameterType.Bool)
                {
                    spumPrefab._anim.SetBool(param.name, true);
                    Debug.Log($"Using death boolean: {param.name}");
                    foundDeathParam = true;
                    break;
                }
            }
        }

        if (!foundDeathParam)
        {
            // Fallback - try to play death animation directly
            Debug.LogWarning("No death parameter found, attempting direct animation play");
            if (spumPrefab.DEATH_List.Count > 0)
            {
                spumPrefab._anim.Play(spumPrefab.DEATH_List[0].name, 0, 0f);
            }
        }

        // Play death sound
        if (deathSound != null)
        {
            PlaySound(deathSound, deathVolume);
        }

        Debug.Log("Enemy death animation triggered");

        // Add at the end:
        DropItems();
    }

    void DropItems()
    {
        if (!dropsItems) return;

        // Drop organs (configurable range)
        int numOrgans = Random.Range(minOrganDrops, maxOrganDrops + 1);
        Dictionary<string, int> organCounts = new Dictionary<string, int>();

        for (int i = 0; i < numOrgans; i++)
        {
            string organType = DropOrgan(organCounts);
            if (organType != null)
            {
                if (!organCounts.ContainsKey(organType))
                    organCounts[organType] = 0;
                organCounts[organType]++;
            }
        }

        // Drop blood vials (configurable range)
        int numBloodVials = Random.Range(minBloodDrops, maxBloodDrops + 1);
        string bloodType = DetermineBloodType();
        for (int i = 0; i < numBloodVials; i++)
        {
            DropBloodVial(bloodType);
        }
    }

    private string DropOrgan(Dictionary<string, int> organCounts)
    {
        if (unlockedItems == null)
        {
            Debug.LogError("UnlockedItems reference is not set in EnemyController!");
            return null;
        }

        // Get list of unlocked organs
        List<string> unlockedOrgans = unlockedItems.GetUnlockedOrgans();
        if (unlockedOrgans == null || unlockedOrgans.Count == 0)
        {
            Debug.LogWarning("No unlocked organs available to drop");
            return null;
        }

        // Define base drop rates
        Dictionary<string, float> baseRates = new Dictionary<string, float>
        {
            { "Eyes", 0.20f },
            { "Finger", 0.20f },
            { "Stomach", 0.15f },
            { "Kidney", 0.15f },
            { "Intestine", 0.15f },
            { "Brain", 0.10f },
            { "Heart", 0.05f }
        };

        // Filter rates for only unlocked organs and respect drop limits
        Dictionary<string, float> availableRates = new Dictionary<string, float>();
        float totalRate = 0f;

        foreach (var unlockedOrgan in unlockedOrgans)
        {
            if (baseRates.ContainsKey(unlockedOrgan))
            {
                // Check if we've hit the limit for this organ type
                int currentCount = organCounts.ContainsKey(unlockedOrgan) ? organCounts[unlockedOrgan] : 0;
                if ((unlockedOrgan == "Eyes" && currentCount < 2) ||
                    (unlockedOrgan == "Finger" && currentCount < 2) ||
                    (unlockedOrgan != "Eyes" && unlockedOrgan != "Finger" && currentCount < 1))
                {
                    availableRates[unlockedOrgan] = baseRates[unlockedOrgan];
                    totalRate += baseRates[unlockedOrgan];
                }
            }
        }

        // If no organs are available to drop (all limits reached), return null
        if (totalRate <= 0) return null;

        // Normalize rates to sum to 1
        Dictionary<string, float> normalizedRates = new Dictionary<string, float>();
        foreach (var kvp in availableRates)
        {
            normalizedRates[kvp.Key] = kvp.Value / totalRate;
        }

        // Select organ based on normalized rates
        float random = Random.value;
        float cumulativeRate = 0f;
        string selectedOrgan = null;

        foreach (var kvp in normalizedRates)
        {
            cumulativeRate += kvp.Value;
            if (random <= cumulativeRate)
            {
                selectedOrgan = kvp.Key;
                break;
            }
        }

        if (selectedOrgan == null) return null;

        // Create the organ GameObject
        GameObject organ = new GameObject("OrganDrop");
        organ.transform.position = transform.position;

        // Add sprite renderer
        SpriteRenderer spriteRenderer = organ.AddComponent<SpriteRenderer>();

        // Add ItemDrop component with organ-specific settings
        ItemDrop itemDrop = organ.AddComponent<ItemDrop>();
        itemDrop.itemType = "Organ";
        itemDrop.itemId = selectedOrgan;
        itemDrop.value = 1;

        // Add Rigidbody2D
        Rigidbody2D rb = organ.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Calculate random drop direction
        float randomAngle = Random.Range(0f, 360f);
        Vector2 dropDirection = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        );

        // Initialize bounce
        itemDrop.InitializeBounce(dropDirection, 0.2f, dropRange);

        return selectedOrgan;
    }

    private string DetermineBloodType()
    {
        float random = Random.value;
        if (random <= 0.70f) return "Red";
        if (random <= 0.95f) return "Blue";
        return "Purple";
    }

    private void DropBloodVial(string bloodType)
    {
        // Create the blood vial drop and set scale immediately
        GameObject bloodVial = new GameObject("BloodVial");
        bloodVial.transform.position = transform.position;
        bloodVial.transform.localScale = new Vector3(2f, 2f, 2f);

        // Add sprite renderer and set the blood vial sprite
        SpriteRenderer spriteRenderer = bloodVial.AddComponent<SpriteRenderer>();
        string spriteName = bloodType.ToLower() + "blood";
        Sprite bloodVialSprite = Resources.Load<Sprite>("BloodPotions/" + spriteName);
        if (bloodVialSprite != null)
        {
            spriteRenderer.sprite = bloodVialSprite;
            spriteRenderer.sortingLayerName = "Item";
            spriteRenderer.sortingOrder = 1;
        }
        else
        {
            Debug.LogError($"Blood vial sprite not found at path: BloodPotions/{spriteName}");
            Destroy(bloodVial);
            return;
        }

        // Add ItemDrop component with blood-specific settings
        ItemDrop itemDrop = bloodVial.AddComponent<ItemDrop>();
        itemDrop.itemId = bloodType;
        itemDrop.itemType = "Blood";
        itemDrop.value = 1;

        // Add Rigidbody2D
        Rigidbody2D rb = bloodVial.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Calculate random drop direction
        float randomAngle = Random.Range(0f, 360f);
        Vector2 dropDirection = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        );

        // Initialize bounce
        itemDrop.InitializeBounce(dropDirection, 0.2f, dropRange);
    }

    public void TakeHit(int damage)
    {
        // Apply damage
        currentHealth -= damage;

        // Check if dead
        if (currentHealth <= 0)
        {
            TriggerDeath();
            return;
        }

        // Play hurt animation
        TriggerHurt();
    }

    void ResetCharacter()
    {
        // Reset state flags
        isDead = false;
        isHurt = false;
        hurtTimer = 0f;
        attackTimer = 0f;
        currentState = PlayerState.IDLE;

        // Reset health
        currentHealth = maxHealth;

        // Re-enable the collider
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
            Debug.Log("Enemy collider re-enabled on reset");
        }
        else
        {
            Debug.LogWarning("No Collider2D found on enemy to re-enable");
        }

        ResetAnimatorState();
        spumPrefab._anim.SetTrigger("isReset");

        attacksSinceSpecial = 0;
        ResetSpecialAttackCounter();

        Debug.Log("Character reset to idle state");
    }

    void MoveTowardsPlayer()
    {
        // Add this check to prevent movement during attack
        if (currentState == PlayerState.ATTACK)
        {
            return; // Don't move while attacking
        }

        if (rb == null)
        {
            // Fallback to original transform movement if rigidbody missing
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
        else
        {
            // Apply velocity so physics engine handles collisions
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }

        // Use the common face player method
        FacePlayer();

        // Set move state if not already set
        if (currentState != PlayerState.MOVE)
        {
            currentState = PlayerState.MOVE;
            spumPrefab.PlayAnimation(PlayerState.MOVE, 0);
        }
    }

    void LaunchProjectile(GameObject projectilePrefab, float speed)
    {
        // Get the current scale to determine if sprite is flipped
        float currentScaleX = spumPrefab.transform.localScale.x;

        Vector3 spawnOffset = new Vector3(
            projectileOffsetX * -currentScaleX,
            projectileOffsetY,
            0
        );

        Vector3 spawnPosition = transform.position + spawnOffset;

        Vector3 direction = (playerTransform.position - spawnPosition).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f);

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, rotation);

        // Check if the projectile has a sprite renderer
        SpriteRenderer spriteRenderer = projectile.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
        }
        else
        {
            Debug.LogError("No SpriteRenderer found on projectile!");
        }

        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(direction, speed);
        }
        else
        {
            Debug.LogWarning("No Projectile script found, trying Rigidbody2D fallback");
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
                Debug.Log($"Rigidbody2D velocity set: {rb.linearVelocity}");
            }
            else
            {
                Debug.LogError("No Rigidbody2D found either!");
            }
        }
    }

    // Add this method to be called by the animation event
    public void OnAttackAnimationEvent()
    {
        bool isSpecialAttack = (currentState == PlayerState.OTHER);

        if (isSpecialAttack)
        {
            if (isRangedAttacker)
            {
                if (specialProjectilePrefab != null)
                {
                    Debug.Log("Launching special ranged projectile");
                    LaunchProjectile(specialProjectilePrefab, specialProjectileSpeed);
                }
                else
                {
                    Debug.LogError("Special projectile prefab is null!");
                }
            }
            else if (isMagicAttacker)
            {
                if (specialProjectilePrefab != null)
                {
                    Debug.Log("Launching special magic projectile");
                    LaunchProjectile(specialProjectilePrefab, specialProjectileSpeed);
                }
                else
                {
                    Debug.LogError("Special projectile prefab is null!");
                }
            }
            else
            {
                // Melee special attack - use the same hitbox with special settings
                if (meleeHitbox != null)
                {
                    meleeHitbox.Activate(true); // Activate with special attack flag
                    Debug.Log("Activated melee hitbox with special attack settings");
                }
            }
        }
        else
        {
            if (isRangedAttacker)
            {
                LaunchProjectile(arrowPrefab, projectileSpeed);
            }
            else if (isMagicAttacker)
            {
                LaunchProjectile(magicOrbPrefab, projectileSpeed);
            }
            else
            {
                // Regular melee attack
                if (meleeHitbox != null)
                {
                    meleeHitbox.Activate(false); // Activate with normal settings
                    Debug.Log("Activated melee hitbox with normal settings");
                }
            }
        }

        // Add this to the OnAttackAnimationEvent() method to handle player damage
        DamagePlayerIfInRange();
    }

    // Add this method to handle sprite flipping
    void FacePlayer()
    {
        if (playerTransform == null) return;

        // Get direction to player
        Vector3 direction = (playerTransform.position - transform.position).normalized;

        // Flip sprite based on direction
        if (direction.x > 0)
        {
            spumPrefab.transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (direction.x < 0)
        {
            spumPrefab.transform.localScale = new Vector3(1, 1, 1);
        }

        // Update melee hitbox whenever we flip the character
        if (meleeHitbox != null)
        {
            meleeHitbox.UpdatePositionAndFlip();
        }
    }

    // Modify the DamagePlayerIfInRange method to use the hitbox:
    private void DamagePlayerIfInRange()
    {
        // For melee attackers, activate the hitbox instead of using circle overlap
        if (!isRangedAttacker && !isMagicAttacker)
        {
            if (meleeHitbox != null)
            {
                // Activate the hitbox
                meleeHitbox.Activate();
            }
            else
            {
                // Fallback to old method if no hitbox assigned
                // Use a circle cast to detect player in attack range
                float attackRadius = useRectangularRange ?
                    Mathf.Max(attackRangeX, attackRangeY) : attackRange;

                Collider2D playerCollider = Physics2D.OverlapCircle(
                    transform.position, attackRadius, playerLayer);

                if (playerCollider != null)
                {
                    // Get player health component
                    PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(attackDamage);

                        // Apply knockback
                        Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();
                        if (playerRb != null)
                        {
                            Vector2 knockbackDir = (playerCollider.transform.position - transform.position).normalized;
                            playerRb.AddForce(knockbackDir * attackKnockback, ForceMode2D.Impulse);
                        }

                        // Flash player red
                        FlashPlayerRed(playerCollider.gameObject);

                        Debug.Log($"Player hit for {attackDamage} damage");
                    }
                }
            }
        }
    }

    private void FlashPlayerRed(GameObject player)
    {
        // Get all sprite renderers on the player
        SpriteRenderer[] renderers = player.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            StartCoroutine(FlashRoutine(renderer));
        }
    }

    private IEnumerator FlashRoutine(SpriteRenderer renderer)
    {
        // Store the original color
        // Color originalColor = renderer.color;

        // Set to red
        renderer.color = Color.red;

        // Wait
        yield return new WaitForSeconds(0.1f);

        // Return to original color
        renderer.color = Color.white;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!doesContactDamage) return;

        HandleContactDamage(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!doesContactDamage) return;

        // Only apply damage at intervals to prevent rapid damage
        if (Time.time - lastContactDamageTime >= contactDamageInterval)
        {
            HandleContactDamage(collision.collider);
        }
    }

    private void HandleContactDamage(Collider2D other)
    {
        // Don't apply damage if the enemy is hurt or dead
        if (isHurt || isDead) return;

        // Only apply to player
        if (other.CompareTag("Player"))
        {
            // Get player health component
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // Check if player is invincible
            if (playerHealth != null && playerHealth.IsInvincible)
            {
                return; // Skip damage if player is invincible
            }

            // Apply damage
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                lastContactDamageTime = Time.time;

                // Apply knockback
                Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    // Direction is from enemy to player
                    Vector2 knockbackDir = (other.transform.position - transform.position).normalized;

                    if (playerRb.bodyType == RigidbodyType2D.Kinematic)
                    {
                        Vector2 targetPosition = (Vector2)other.transform.position +
                                               (knockbackDir * contactKnockbackDistance);
                        StartCoroutine(KinematicKnockback(other.transform, targetPosition));
                    }
                    else
                    {
                        playerRb.AddForce(knockbackDir * contactKnockbackForce, ForceMode2D.Impulse);
                    }
                }

                // Add stun effect
                StunPlayer(other.gameObject, 0.1f);

                // Flash player red
                FlashPlayerRed(other.gameObject);

                Debug.Log($"Player hit by contact damage for {contactDamage} damage");
            }
        }
    }

    // Add this method for kinematic knockback if it's not already in EnemyController
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

    // Add this method for stunning the player if it's not already in EnemyController
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

    // Add these helper methods for sound playing
    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private IEnumerator PlayDelayedSound(AudioClip clip, float volume, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySound(clip, volume);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }
}
