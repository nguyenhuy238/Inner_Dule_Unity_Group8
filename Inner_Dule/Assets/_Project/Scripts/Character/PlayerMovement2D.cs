using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using InnerDuel.Audio;

namespace InnerDuel.Characters
{
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class PlayerMovement2D : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("1 for Player1, 2 for Player2")] public int playerID = 1;

        [Header("Movement Settings")]
        public float moveSpeed = 6f;
        public float jumpForce = 12f;
        public float airControlMultiplier = 0.8f;

        [Header("Attack3 Leap Settings")]
        [Tooltip("Horizontal impulse applied when performing Attack3 (leap forward)")] public float attack3ForwardImpulse = 6f;
        [Tooltip("Vertical impulse applied when performing Attack3 (leap upward)")] public float attack3UpImpulse = 8f;
        [Tooltip("Duration to lock horizontal control so the leap isn't damped (seconds)")] public float attack3ControlLock = 0.2f;

        [HideInInspector]
        public Transform groundCheck;
        [HideInInspector] public float groundCheckRadius = 0.15f;
        [HideInInspector] public LayerMask groundLayer;

        [Header("Projectile (Player 2 Only)")]
        [Tooltip("Fireball prefab for Player 2's Attack1")]
        public GameObject fireballPrefab;
        public Transform fireballSpawnPoint;
        public float fireballSpeed = 15f;
        
        [Header("Skill Projectiles (Player 2 Only)")]
        [Tooltip("Skill1 projectile prefab (Numpad 2)")]
        public GameObject skill1ProjectilePrefab;
        public float skill1ProjectileSpeed = 18f;
        
        [Tooltip("Skill2 projectile prefab (Numpad 3)")]
        public GameObject skill2ProjectilePrefab;
        public float skill2ProjectileSpeed = 20f;
        
        [Tooltip("Skill3 projectile prefab (Numpad 4)")]
        public GameObject skill3ProjectilePrefab;
        public float skill3ProjectileSpeed = 22f;

        [Header("Combat & Health")]
        public float maxHealth = 100f;
        public HealthBar healthBar;
        public LayerMask enemyLayer;

        [Header("Attack 1 Settings")]
        public float attack1Damage = 15f;
        public Transform attack1Point;
        public float attack1Range = 1.5f;

        [Header("Attack 2 Settings")]
        public float attack2Damage = 20f;
        public Transform attack2Point;
        public float attack2Range = 1.8f;

        [Header("Attack 3 Settings")]
        public float attack3Damage = 25f;
        public Transform attack3Point;
        public float attack3Range = 2.0f;

        // Components
        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer sprite;
        private Collider2D col;

        // State
        private Vector2 moveInput;
        private bool isGrounded;
        private bool jumpQueued;
        private float controlLockUntil;
        
        // Jump state
        private int jumpCount = 0;
        private const int maxJumps = 2; // Double jump

        // Combat state
        private float currentHealth;
        private bool isDead = false;
        private float invincibilityTimer = 0f;
        private float invincibilityDuration = 0.3f;
        
        // Defence state
        private bool isDefending = false;
        private const float defenceReduction = 0.2f; // Reduce damage to 20% (1/5) when defending

        [Header("Knockback Settings (Kick)")]
        [Tooltip("Enable knockback when Player2's kick hits an enemy")]
        public bool enableKickKnockback = true;
        [Tooltip("Horizontal impulse for kick knockback")]
        public float kickKnockbackForce = 38f;
        [Tooltip("How long the target loses control during knockback (seconds)")]
        public float kickKnockbackDuration = 0.3f;
        
        private bool isKnockbacked = false;
        
        // Attack state
        private bool isAttacking = false;
        private float attack1CooldownTimer = 0f;
        private float attack2CooldownTimer = 0f;
        private float attack3CooldownTimer = 0f;
        private const float attack1Cooldown = 1f;  // 1 giây
        private const float attack2Cooldown = 5f;  // 5 giây
        private const float attack3Cooldown = 8f;  // 8 giây

        // Animator parameter presence cache (to tolerate different controllers)
        private bool hasIsJumping;
        private bool hasIsJumpingPascal;
        private bool hasGrounded;
        private bool hasIsGrounded;
        private bool hasSpeed;
        private bool hasAttack;      // Attack trigger (new basic attack)
        private bool hasSkill1;      // Skill1 trigger
        private bool hasSkill2;      // Skill2 trigger
        private bool hasSkill3;      // Skill3 trigger
        private bool hasDefence;     // IsDefence bool for defence state
        private bool hasIsAttacking;
        private bool hasIsActtack; // Typo variant used in Player2 controller
        private bool hasHit; // Hit trigger for damage animation (Player1)
        private bool hasHitLowercase; // hit trigger (Player2)
        private bool hasIsDead; // isDead bool for death animation (Player1)
        private bool hasDieTrigger; // die trigger for death animation (Player2)

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            sprite = GetComponent<SpriteRenderer>();

            // GroundCheck transform is no longer required. We detect grounding using collider bounds.
            col = GetComponent<Collider2D>();

            // Auto-create attack points if none assigned
            // Attack points will be updated dynamically based on flip direction
            if (attack1Point == null)
            {
                var ap1 = new GameObject("Attack1Point");
                ap1.transform.SetParent(transform);
                ap1.transform.localPosition = new Vector3(1.0f, 0.5f, 0f); // Phía trước, cao
                attack1Point = ap1.transform;
            }
            if (attack2Point == null)
            {
                var ap2 = new GameObject("Attack2Point");
                ap2.transform.SetParent(transform);
                ap2.transform.localPosition = new Vector3(1.2f, 0f, 0f); // Phía trước, giữa
                attack2Point = ap2.transform;
            }
            if (attack3Point == null)
            {
                var ap3 = new GameObject("Attack3Point");
                ap3.transform.SetParent(transform);
                ap3.transform.localPosition = new Vector3(1.5f, 1.0f, 0f); // Xa hơn, cao hơn
                attack3Point = ap3.transform;
            }

            // Initialize attack points direction based on initial sprite flip
            if (sprite != null)
            {
                UpdateAttackPointsDirection();
            }

            // Initialize health
            currentHealth = maxHealth;
            Debug.Log($"Player{playerID} initializing health: {currentHealth}/{maxHealth}");
            
            if (healthBar != null)
            {
                Debug.Log($"Player{playerID} HealthBar found: {healthBar.gameObject.name}");
                
                if (healthBar.healthSlider != null)
                {
                    Debug.Log($"Player{playerID} HealthBar.healthSlider found");
                }
                
                healthBar.SetMaxHealth(maxHealth);
                healthBar.SetHealth(currentHealth);
                Debug.Log($"Player{playerID} HealthBar initialized");
            }
            // HealthBar will be assigned by UIManager later - no warning needed

            // Cache animator parameter availability to avoid mismatches
            if (animator != null)
            {
                foreach (var p in animator.parameters)
                {
                    switch (p.name)
                    {
                        case "isJumping": hasIsJumping = p.type == AnimatorControllerParameterType.Bool; break;
                        case "IsJumping": hasIsJumpingPascal = p.type == AnimatorControllerParameterType.Bool; break;
                        case "Grounded": hasGrounded = p.type == AnimatorControllerParameterType.Bool; break;
                        case "IsGrounded": hasIsGrounded = p.type == AnimatorControllerParameterType.Bool; break;
                        case "Speed": hasSpeed = p.type == AnimatorControllerParameterType.Float; break;
                        // New attack/skill system
                        case "Attack": hasAttack = p.type == AnimatorControllerParameterType.Trigger; break;
                        case "Skill1": hasSkill1 = p.type == AnimatorControllerParameterType.Trigger; break;
                        case "Skill2": hasSkill2 = p.type == AnimatorControllerParameterType.Trigger; break;
                        case "Skill3": hasSkill3 = p.type == AnimatorControllerParameterType.Trigger; break;
                        // Defence
                        case "IsDefence": hasDefence = p.type == AnimatorControllerParameterType.Bool; break;
                        case "IsDefece": hasDefence = p.type == AnimatorControllerParameterType.Bool; break; // typo variant
                        // Attack states
                        case "IsAttacking": hasIsAttacking = p.type == AnimatorControllerParameterType.Bool; break;
                        case "IsActtack": hasIsActtack = p.type == AnimatorControllerParameterType.Bool; break;
                        // Hit/Damage
                        case "Hit": hasHit = p.type == AnimatorControllerParameterType.Trigger; break;
                        case "hit": hasHitLowercase = p.type == AnimatorControllerParameterType.Trigger; break;
                        // Death
                        case "isDead": hasIsDead = p.type == AnimatorControllerParameterType.Bool; break;
                        case "IsDeath": hasIsDead = p.type == AnimatorControllerParameterType.Bool; break; // variant
                        case "die": hasDieTrigger = p.type == AnimatorControllerParameterType.Trigger; break;
                    }
                }
            }
        }

        private void Update()
        {
            if (isDead) return;

            // Update timers
            if (invincibilityTimer > 0) invincibilityTimer -= Time.deltaTime;
            if (attack1CooldownTimer > 0) attack1CooldownTimer -= Time.deltaTime;
            if (attack2CooldownTimer > 0) attack2CooldownTimer -= Time.deltaTime;
            if (attack3CooldownTimer > 0) attack3CooldownTimer -= Time.deltaTime;

            float x = 0f;

            if (Keyboard.current != null)
            {
                // Player 1: A/D move, W jump, S defend, H attack, J/K/L skills
                if (playerID == 1)
                {
                    if (Keyboard.current.aKey.isPressed) x -= 1f;
                    if (Keyboard.current.dKey.isPressed) x += 1f;

                    // Jump on W
                    if (Keyboard.current.wKey.wasPressedThisFrame)
                    {
                        jumpQueued = true;
                    }

                    // Defence on S (hold)
                    if (Keyboard.current.sKey.isPressed)
                    {
                        isDefending = true;
                        if (animator != null && hasDefence)
                        {
                            // Use the actual parameter name from controller (with typo)
                            animator.SetBool("IsDefece", true);
                        }
                    }
                    else
                    {
                        isDefending = false;
                        if (animator != null && hasDefence)
                        {
                            animator.SetBool("IsDefece", false);
                        }
                    }

                    // Attacks and Skills
                    if (animator != null)
                    {
                        // H = Attack (basic attack)
                        if (Keyboard.current.hKey != null && Keyboard.current.hKey.wasPressedThisFrame)
                        {
                            if (attack1CooldownTimer <= 0)
                            {
                                if (hasAttack) animator.SetTrigger("Attack");
                                AudioManager.Instance?.PlayAttackSound(playerID);
                                PerformMeleeAttack(1); // Attack
                                attack1CooldownTimer = attack1Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.5f));
                                Debug.Log("Player1 Attack (H) triggered");
                            }
                            else
                            {
                                Debug.Log($"Player1 Attack on cooldown! {attack1CooldownTimer:F1}s remaining");
                            }
                        }
                        // J = Skill1
                        if (Keyboard.current.jKey != null && Keyboard.current.jKey.wasPressedThisFrame)
                        {
                            if (attack1CooldownTimer <= 0)
                            {
                                if (hasSkill1) animator.SetTrigger("Skill1");
                                AudioManager.Instance?.PlaySkill1Sound(playerID);
                                PerformMeleeAttack(1); // Skill 1
                                attack1CooldownTimer = attack1Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.5f));
                                Debug.Log("Player1 Skill1 (J) triggered");
                            }
                            else
                            {
                                Debug.Log($"Player1 Skill1 on cooldown! {attack1CooldownTimer:F1}s remaining");
                            }
                        }
                        // K = Skill2
                        if (Keyboard.current.kKey != null && Keyboard.current.kKey.wasPressedThisFrame)
                        {
                            if (attack2CooldownTimer <= 0)
                            {
                                if (hasSkill2) animator.SetTrigger("Skill2");
                                AudioManager.Instance?.PlaySkill2Sound(playerID);
                                PerformMeleeAttack(2); // Skill 2
                                attack2CooldownTimer = attack2Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.6f));
                                Debug.Log("Player1 Skill2 (K) triggered");
                            }
                            else
                            {
                                Debug.Log($"Player1 Skill2 on cooldown! {attack2CooldownTimer:F1}s remaining");
                            }
                        }
                        // L = Skill3 (no leap, just animation)
                        if (Keyboard.current.lKey != null && Keyboard.current.lKey.wasPressedThisFrame)
                        {
                            if (attack3CooldownTimer <= 0)
                            {
                                if (hasSkill3) animator.SetTrigger("Skill3");
                                AudioManager.Instance?.PlaySkill3Sound(playerID);
                                PerformMeleeAttack(3); // Skill 3
                                attack3CooldownTimer = attack3Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.8f));
                                Debug.Log("Player1 Skill3 (L) triggered");
                                // Removed leap effect - just play animation
                            }
                            else
                            {
                                Debug.Log($"Player1 Skill3 on cooldown! {attack3CooldownTimer:F1}s remaining");
                            }
                        }
                    }
                }
                // Player 2: Arrow Keys (Left/Right) move, Up jump, Down defend, 1 attack, 2/3/4 skills
                else if (playerID == 2)
                {
                    if (Keyboard.current.leftArrowKey.isPressed) x -= 1f;
                    if (Keyboard.current.rightArrowKey.isPressed) x += 1f;

                    // Jump on Up Arrow
                    if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                    {
                        jumpQueued = true;
                        Debug.Log($"Player2 Jump Queued! isGrounded={isGrounded}");
                    }

                    // Defence on Down Arrow (hold)
                    if (Keyboard.current.downArrowKey.isPressed)
                    {
                        isDefending = true;
                        if (animator != null && hasDefence)
                        {
                            animator.SetBool("IsDefence", true);
                        }
                    }
                    else
                    {
                        isDefending = false;
                        if (animator != null && hasDefence)
                        {
                            animator.SetBool("IsDefence", false);
                        }
                    }

                    // Attacks and Skills: Numpad 1/2/3/4 = Attack, Skill1/2/3
                    if (animator != null)
                    {
                        // Numpad 1 = Attack (Fireball)
                        if (Keyboard.current.numpad1Key != null && Keyboard.current.numpad1Key.wasPressedThisFrame)
                        {
                            if (attack1CooldownTimer <= 0)
                            {
                                if (hasAttack) animator.SetTrigger("Attack");
                                if (hasIsAttacking) animator.SetBool("IsAttacking", true);
                                if (hasIsActtack) animator.SetBool("IsActtack", true);
                                AudioManager.Instance?.PlayAttackSound(playerID);
                                StartCoroutine(ResetAttackBool(0.5f));
                                StartCoroutine(SpawnFireball(0.2f)); // Delay for animation
                                PerformMeleeAttack(1); // Attack: melee damage if in range
                                attack1CooldownTimer = attack1Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.5f));
                                Debug.Log("Player2 Attack (Numpad 1) triggered + fireball");
                            }
                            else
                            {
                                Debug.Log($"Player2 Attack on cooldown! {attack1CooldownTimer:F1}s remaining");
                            }
                        }
                        // Numpad 2 = Skill1 (with Skill1 projectile)
                        if (Keyboard.current.numpad2Key != null && Keyboard.current.numpad2Key.wasPressedThisFrame)
                        {
                            if (attack1CooldownTimer <= 0)
                            {
                                if (hasSkill1) animator.SetTrigger("Skill1");
                                AudioManager.Instance?.PlaySkill1Sound(playerID);
                                PerformMeleeAttack(1); // Skill 1: deal damage
                                attack1CooldownTimer = attack1Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.5f));
                                StartCoroutine(SpawnSkillProjectileDelayed(skill1ProjectilePrefab, skill1ProjectileSpeed, 0.45f)); // Spawn Skill1 projectile
                                Debug.Log("Player2 Skill1 (Numpad 2) triggered + Skill1 projectile");
                            }
                            else
                            {
                                Debug.Log($"Player2 Skill1 on cooldown! {attack1CooldownTimer:F1}s remaining");
                            }
                        }
                        // Numpad 3 = Skill2 (with Skill2 projectile)
                        if (Keyboard.current.numpad3Key != null && Keyboard.current.numpad3Key.wasPressedThisFrame)
                        {
                            if (attack2CooldownTimer <= 0)
                            {
                                if (hasSkill2) animator.SetTrigger("Skill2");
                                AudioManager.Instance?.PlaySkill2Sound(playerID);
                                PerformMeleeAttack(2); // Skill 2: deal damage
                                attack2CooldownTimer = attack2Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.6f));
                                StartCoroutine(SpawnSkillProjectileDelayed(skill2ProjectilePrefab, skill2ProjectileSpeed, 0.45f)); // Spawn Skill2 projectile
                                Debug.Log("Player2 Skill2 (Numpad 3) triggered + Skill2 projectile");
                            }
                            else
                            {
                                Debug.Log($"Player2 Skill2 on cooldown! {attack2CooldownTimer:F1}s remaining");
                            }
                        }
                        // Numpad 4 = Skill3 (with Skill3 projectile)
                        if (Keyboard.current.numpad4Key != null && Keyboard.current.numpad4Key.wasPressedThisFrame)
                        {
                            if (attack3CooldownTimer <= 0)
                            {
                                if (hasSkill3) animator.SetTrigger("Skill3");
                                AudioManager.Instance?.PlaySkill3Sound(playerID);
                                PerformMeleeAttack(3); // Skill 3: melee damage if in range
                                attack3CooldownTimer = attack3Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.8f));
                                StartCoroutine(SpawnSkillProjectileDelayed(skill3ProjectilePrefab, skill3ProjectileSpeed, 0.5f)); // Spawn Skill3 projectile
                                Debug.Log("Player2 Skill3 (Numpad 4) triggered + Skill3 projectile");
                            }
                            else
                            {
                                Debug.Log($"Player2 Skill3 on cooldown! {attack3CooldownTimer:F1}s remaining");
                            }
                        }
                    }
                }

                moveInput = new Vector2(x, 0f);
                // Defer animator update and flipping to FixedUpdate after physics & ground check
            }
        }

        private void FixedUpdate()
        {
            // Ground detection using collider bounds (no external transform needed)
            bool grounded = false;
            Vector2 checkPos;
            float extraDepth = 0.06f;
            if (col != null)
            {
                checkPos = new Vector2(col.bounds.center.x, col.bounds.min.y - extraDepth);
            }
            else
            {
                checkPos = (Vector2)transform.position + new Vector2(0f, -0.5f);
            }

            // 1) Overlap circle
            Collider2D[] hits;
            hits = Physics2D.OverlapCircleAll(checkPos, groundCheckRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h == null) continue;
                if (h.attachedRigidbody == rb) continue;
                if (h.isTrigger) continue;
                if (!h.CompareTag("Ground")) continue;
                grounded = true;
                break;
            }

            // 2) Short raycast downward as fallback
            if (!grounded)
            {
                float rayLen = Mathf.Max(groundCheckRadius, 0.2f);
                RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, rayLen);
                if (hit.collider != null)
                {
                    if ((hit.rigidbody == null || hit.rigidbody != rb) && !hit.collider.isTrigger && hit.collider.CompareTag("Ground"))
                    {
                        grounded = true;
                    }
                }
            }

            isGrounded = grounded;


            // Horizontal movement
            float targetXVel = moveInput.x * moveSpeed;
            float control = isGrounded ? 1f : airControlMultiplier;

            if (isKnockbacked)
            {
                // During knockback, don't override horizontal velocity with input
                // Let physics settle; we only preserve current Y velocity
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
            }
            // Khóa movement khi đang tấn công
            else if (isAttacking)
            {
                // Dừng di chuyển ngang khi đang attack (trừ Attack3 có leap)
                if (Time.time >= controlLockUntil)
                {
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                }
            }
            // Lock some control just after Attack3 so leap isn't canceled by input
            else if (Time.time < controlLockUntil)
            {
                // Preserve current horizontal momentum; only apply slight damping to feel responsive
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, rb.velocity.x, 1f), rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, targetXVel, control), rb.velocity.y);
            }

            // Jump - Double jump system
            if (jumpQueued)
            {
                // Reset jump count when grounded
                if (isGrounded)
                {
                    jumpCount = 0;
                }
                
                // Allow jump if we haven't used all jumps
                if (jumpCount < maxJumps)
                {
                    rb.velocity = new Vector2(rb.velocity.x, 0f);
                    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                    jumpCount++;
                    AudioManager.Instance?.PlayJumpSound();
                    Debug.Log($"Player{playerID} JUMPING! Jump {jumpCount}/{maxJumps}, jumpForce={jumpForce}");
                }
                else
                {
                    Debug.Log($"Player{playerID} tried to jump but already used all {maxJumps} jumps!");
                }
            }
            jumpQueued = false;

            // Animator + facing after physics & ground were resolved
            UpdateAnimatorBooleans();
            FlipByDirection(rb.velocity.x);

            // Update running SFX based on movement state
            UpdateRunAudio();
        }

        private void UpdateRunAudio()
        {
            // Simple global run loop: play while grounded and moving, stop otherwise
            bool shouldRun = isGrounded && Mathf.Abs(rb.velocity.x) > 0.1f && !isAttacking && !isKnockbacked;
            AudioManager.Instance?.UpdateRunState(playerID, shouldRun);
        }

        private void UpdateAnimatorBooleans()
        {
            if (animator == null) return;
            
            // Jump state follows grounded only (velocity used just for snappier feel)
            bool jumping = !isGrounded || rb.velocity.y > 0.1f;

            // Update Speed (Float) for running animation - used by both players
            if (hasSpeed) animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            
            if (hasIsJumping) animator.SetBool("isJumping", jumping);
            if (hasIsJumpingPascal) animator.SetBool("IsJumping", jumping);
            if (hasGrounded) animator.SetBool("Grounded", isGrounded);
            if (hasIsGrounded) animator.SetBool("IsGrounded", isGrounded);
        }

        private void FlipByDirection(float x)
        {
            if (sprite == null) return;
            if (Mathf.Abs(x) > 0.01f)
            {
                // Player2 sprite faces left by default, so reverse flip logic
                if (playerID == 2)
                {
                    sprite.flipX = x > 0f; // Moving right = flip to true, moving left = flip to false
                }
                else
                {
                    sprite.flipX = x < 0f; // Player1 normal logic: Moving left = flip to true
                }
                
                // Update attack points to face correct direction
                UpdateAttackPointsDirection();
            }
        }
        
        /// <summary>
        /// Update attack points to face the direction the sprite is facing
        /// </summary>
        private void UpdateAttackPointsDirection()
        {
            // Determine if facing left based on flipX
            // For Player2: flipX=false means facing left (default), flipX=true means facing right
            // For Player1: flipX=false means facing right (default), flipX=true means facing left
            bool facingLeft = (playerID == 2) ? !sprite.flipX : sprite.flipX;
            float direction = facingLeft ? -1f : 1f; // Trái = -1 (x âm), Phải = 1 (x dương)
            
            // Update attack point positions
            if (attack1Point != null)
            {
                Vector3 pos = attack1Point.localPosition;
                attack1Point.localPosition = new Vector3(Mathf.Abs(pos.x) * direction, pos.y, pos.z);
            }
            if (attack2Point != null)
            {
                Vector3 pos = attack2Point.localPosition;
                attack2Point.localPosition = new Vector3(Mathf.Abs(pos.x) * direction, pos.y, pos.z);
            }
            if (attack3Point != null)
            {
                Vector3 pos = attack3Point.localPosition;
                attack3Point.localPosition = new Vector3(Mathf.Abs(pos.x) * direction, pos.y, pos.z);
            }
            
            // Update fireball spawn point (Player2 only)
            if (fireballSpawnPoint != null && playerID == 2)
            {
                Vector3 pos = fireballSpawnPoint.localPosition;
                fireballSpawnPoint.localPosition = new Vector3(Mathf.Abs(pos.x) * direction, pos.y, pos.z);
            }
        }

        private IEnumerator ResetAttackBool(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (animator != null)
            {
                if (hasIsAttacking) animator.SetBool("IsAttacking", false);
                if (hasIsActtack) animator.SetBool("IsActtack", false);
            }
        }
        
        /// <summary>
        /// Khóa di chuyển trong thời gian tấn công
        /// </summary>
        private IEnumerator LockMovementDuringAttack(float duration)
        {
            isAttacking = true;
            yield return new WaitForSeconds(duration);
            isAttacking = false;
        }

        private IEnumerator SpawnFireball(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            SpawnSingleFireball();
        }

        private void SpawnSingleFireball()
        {
            if (fireballPrefab == null || playerID != 2) return;

            // Determine direction based on sprite flip
            // For Player2: flipX=false means facing left, flipX=true means facing right
            float direction = (sprite != null && sprite.flipX) ? 1f : -1f;
            
            // Determine spawn position
            Vector3 spawnPos;
            if (fireballSpawnPoint != null)
            {
                spawnPos = fireballSpawnPoint.position;
            }
            else
            {
                // Default offset in front of player
                spawnPos = transform.position + new Vector3(direction * 0.5f, 0.5f, 0f);
            }
            
            // Spawn fireball (set Z to -1 to render in front of players)
            Vector3 spawnPosWithZ = new Vector3(spawnPos.x, spawnPos.y, -1f);
            GameObject fireball = Instantiate(fireballPrefab, spawnPosWithZ, Quaternion.identity);
            
            // Set velocity
            Rigidbody2D rb2d = fireball.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.velocity = new Vector2(direction * fireballSpeed, 0f);
            }
            
            // Flip sprite if needed
            SpriteRenderer fbSprite = fireball.GetComponent<SpriteRenderer>();
            if (fbSprite != null && direction < 0f)
            {
                fbSprite.flipX = true;
            }
        }

        private IEnumerator SpawnFireballsBurst(float initialDelay, int count, float interval)
        {
            if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);
            for (int i = 0; i < count; i++)
            {
                SpawnSingleFireball();
                if (i < count - 1 && interval > 0f)
                {
                    yield return new WaitForSeconds(interval);
                }
            }
        }

        // ========== SKILL PROJECTILE SPAWNING ==========
        
        /// <summary>
        /// Spawn a skill projectile with custom prefab and speed
        /// </summary>
        private void SpawnSkillProjectile(GameObject prefab, float speed)
        {
            if (prefab == null || playerID != 2) return;

            // Determine direction based on sprite flip
            float direction = (sprite != null && sprite.flipX) ? 1f : -1f;

            // Determine spawn position
            Vector3 spawnPos;
            if (fireballSpawnPoint != null)
            {
                spawnPos = fireballSpawnPoint.position;
            }
            else
            {
                spawnPos = transform.position + new Vector3(direction * 0.5f, 0.5f, 0f);
            }

            // Spawn projectile (set Z to -1 to render in front of players)
            Vector3 spawnPosWithZ = new Vector3(spawnPos.x, spawnPos.y, -1f);
            GameObject projectile = Instantiate(prefab, spawnPosWithZ, Quaternion.identity);

            // Set velocity
            Rigidbody2D rb2d = projectile.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.velocity = new Vector2(direction * speed, 0f);
            }

            // Flip sprite if shooting left
            SpriteRenderer projSprite = projectile.GetComponent<SpriteRenderer>();
            if (projSprite != null && direction < 0f)
            {
                projSprite.flipX = true;
            }
        }

        /// <summary>
        /// Spawn a skill projectile with delay (for animation timing)
        /// </summary>
        private IEnumerator SpawnSkillProjectileDelayed(GameObject prefab, float speed, float delay)
        {
            yield return new WaitForSeconds(delay);
            SpawnSkillProjectile(prefab, speed);
        }

        /// <summary>
        /// Spawn multiple skill projectiles in a burst
        /// </summary>
        private IEnumerator SpawnSkillProjectileBurst(GameObject prefab, float speed, float initialDelay, int count, float interval)
        {
            if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);
            
            for (int i = 0; i < count; i++)
            {
                SpawnSkillProjectile(prefab, speed);
                if (i < count - 1 && interval > 0f)
                {
                    yield return new WaitForSeconds(interval);
                }
            }
        }

        /// <summary>
        /// Perform melee attack - check for enemies in range and deal damage
        /// </summary>
        /// <param name="attackNumber">1, 2, or 3 - which attack to use</param>
        private void PerformMeleeAttack(int attackNumber)
        {
            if (enemyLayer.value == 0) return;

            // Select appropriate attack settings
            Transform attackPoint = null;
            float attackRange = 0f;
            float attackDamage = 0f;

            switch (attackNumber)
            {
                case 1:
                    attackPoint = attack1Point;
                    attackRange = attack1Range;
                    attackDamage = attack1Damage;
                    break;
                case 2:
                    attackPoint = attack2Point;
                    attackRange = attack2Range;
                    attackDamage = attack2Damage;
                    break;
                case 3:
                    attackPoint = attack3Point;
                    attackRange = attack3Range;
                    attackDamage = attack3Damage;
                    break;
                default:
                    Debug.LogWarning($"Invalid attack number: {attackNumber}");
                    return;
            }

            if (attackPoint == null)
            {
                Debug.LogWarning($"Attack{attackNumber}Point is null!");
                return;
            }

            // Detect enemies in attack range
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

            foreach (Collider2D enemy in hitEnemies)
            {
                // Compute knockback direction (horizontal away from attacker)
                Vector2 toEnemy = (Vector2)(enemy.transform.position - transform.position);
                float dirX = Mathf.Sign(toEnemy.x == 0f ? (sprite != null && ((playerID==2)? !sprite.flipX : sprite.flipX) ? -1f : 1f) : toEnemy.x);
                Vector2 kickImpulse = new Vector2(dirX * kickKnockbackForce, 0f);

                // Try to damage via PlayerMovement2D
                var enemyPlayer = enemy.GetComponent<PlayerMovement2D>();
                if (enemyPlayer != null)
                {
                    enemyPlayer.TakeDamage(attackDamage);

                    // Apply knockback for Player2's Attack2 (kick)
                    if (playerID == 2 && attackNumber == 2 && enableKickKnockback)
                    {
                        enemyPlayer.ApplyKnockback(kickImpulse, kickKnockbackDuration);
                    }
                    
                    Debug.Log($"Player{playerID} Attack{attackNumber} dealt {attackDamage} damage to {enemy.name}");
                }
                
                // Try to damage via InnerCharacterController (if exists)
                var enemyController = enemy.GetComponent<InnerCharacterController>();
                if (enemyController != null)
                {
                    enemyController.TakeDamage(attackDamage);

                    // Apply knockback to enemies using InnerCharacterController via Rigidbody2D (no code changes there)
                    if (playerID == 2 && attackNumber == 2 && enableKickKnockback)
                    {
                        var enemyRb = enemyController.GetComponent<Rigidbody2D>();
                        if (enemyRb != null)
                        {
                            enemyRb.velocity = new Vector2(0f, enemyRb.velocity.y); // reset X for consistent push
                            enemyRb.AddForce(kickImpulse, ForceMode2D.Impulse);
                        }
                    }
                    
                    Debug.Log($"Player{playerID} Attack{attackNumber} dealt {attackDamage} damage to {enemy.name}");
                }
            }
        }

        /// <summary>
        /// Initialize HealthBar after it's assigned by UIManager
        /// </summary>
        public void InitializeHealthBar()
        {
            if (healthBar != null)
            {
                healthBar.SetMaxHealth(maxHealth);
                healthBar.SetHealth(currentHealth);
                Debug.Log($"Player{playerID} HealthBar initialized by UIManager");
            }
            else
            {
                Debug.LogWarning($"Player{playerID} InitializeHealthBar called but healthBar is still NULL");
            }
        }

        /// <summary>
        /// Take damage from enemy attacks
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (isDead)
            {
                Debug.Log($"Player{playerID} is already dead, ignoring damage");
                return;
            }
            
            if (invincibilityTimer > 0)
            {
                Debug.Log($"Player{playerID} is invincible ({invincibilityTimer:F2}s remaining), ignoring damage");
                return;
            }

            // Apply defence reduction if defending
            float actualDamage = damage;
            if (isDefending)
            {
                actualDamage = damage * defenceReduction; // 20% damage when defending
                Debug.Log($"Player{playerID} is DEFENDING! Reduced damage from {damage} to {actualDamage}");
            }
            
            currentHealth -= actualDamage;
            invincibilityTimer = invincibilityDuration;

            Debug.Log($"Player{playerID} took {actualDamage} damage! Current HP: {currentHealth}/{maxHealth}");

            // Update health bar
            if (healthBar != null)
            {
                Debug.Log($"Player{playerID} updating HealthBar with currentHealth={currentHealth}");
                healthBar.SetHealth(currentHealth);
                
                // Verify the update
                if (healthBar.healthSlider != null)
                {
                    Debug.Log($"HealthBar.healthSlider.value after update: {healthBar.healthSlider.value}");
                }
                else
                {
                    Debug.LogError($"Player{playerID} HealthBar.healthSlider is NULL!");
                }
            }
            else
            {
                Debug.LogError($"Player{playerID} HealthBar is NULL! Health bar will not update.");
            }

            // Trigger hit animation
            if (animator != null)
            {
                if (hasHit)
                {
                    animator.SetTrigger("Hit");
                    Debug.Log($"Player{playerID} playing Hit animation");
                }
                else if (hasHitLowercase)
                {
                    animator.SetTrigger("hit");
                    Debug.Log($"Player{playerID} playing hit animation");
                }
            }

            // Visual feedback
            if (sprite != null)
            {
                StartCoroutine(DamageFlash());
            }

            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Flash sprite red when taking damage
        /// </summary>
        private IEnumerator DamageFlash()
        {
            Color original = sprite.color;
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sprite.color = original;
        }

        public void ApplyKnockback(Vector2 impulse, float duration)
        {
            if (rb == null) return;
            StopCoroutineSafe(knockbackRoutine);
            knockbackRoutine = StartCoroutine(KnockbackRoutine(impulse, duration));
        }

        private Coroutine knockbackRoutine;
        private IEnumerator KnockbackRoutine(Vector2 impulse, float duration)
        {
            isKnockbacked = true;
            // reset horizontal velocity before applying impulse for consistency
            rb.velocity = new Vector2(0f, rb.velocity.y);
            rb.AddForce(impulse, ForceMode2D.Impulse);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                yield return null;
            }
            isKnockbacked = false;
        }

        private void StopCoroutineSafe(Coroutine c)
        {
            if (c != null) StopCoroutine(c);
        }

        /// <summary>
        /// Check if player is dead
        /// </summary>
        public bool IsDead()
        {
            return isDead;
        }
        
        /// <summary>
        /// Handle player death
        /// </summary>
        private void Die()
        {
            if (isDead) return; // Already dead, don't trigger again
            
            isDead = true;
            isKnockbacked = false; // clear state on death
            Debug.Log($"Player{playerID} died!");

            // Disable movement immediately
            rb.velocity = Vector2.zero;

            // Start death animation sequence
            StartCoroutine(PlayDeathAnimation());

            // TODO: Notify GameManager
        }
        
        /// <summary>
        /// Play death animation once and destroy GameObject
        /// </summary>
        private IEnumerator PlayDeathAnimation()
        {
            float deathAnimLength = 2.0f; // default
            
            if (animator != null)
            {
                Debug.Log($"Player{playerID} PlayDeathAnimation started. Current state: {animator.GetCurrentAnimatorStateInfo(0).fullPathHash}");
                
                // Set isDead/IsDeath parameter to trigger death animation
                if (hasIsDead)
                {
                    // Try IsDeath first (what's in the controller), then isDead
                    animator.SetBool("IsDeath", true);
                    Debug.Log($"Player{playerID} set IsDeath=true for death animation");
                }
                
                // Try die trigger (only if isDead bool is not available)
                if (hasDieTrigger && !hasIsDead)
                {
                    animator.SetTrigger("die");
                    Debug.Log($"Player{playerID} triggered die animation");
                }
                
                // Wait for animator to transition to death state
                yield return new WaitForSeconds(0.25f);
                
                // Check current state
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"Player{playerID} after transition - State: {stateInfo.fullPathHash}, IsName 'die': {stateInfo.IsName("die")}, normalizedTime: {stateInfo.normalizedTime}");
                
                // Get the current animation clip length
                AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                
                if (clipInfo.Length > 0)
                {
                    deathAnimLength = clipInfo[0].clip.length;
                    Debug.Log($"Player{playerID} death animation clip: '{clipInfo[0].clip.name}', length: {deathAnimLength}s");
                }
                else
                {
                    // Fallback: use default values
                    if (playerID == 1)
                    {
                        deathAnimLength = 2.2f;
                    }
                    else
                    {
                        deathAnimLength = 1.0f; // Player2 die animation is 1 second
                    }
                    Debug.LogWarning($"Player{playerID} NO CLIP INFO! Using default: {deathAnimLength}s");
                }
            }
            
            Debug.Log($"Player{playerID} waiting {deathAnimLength}s for death animation to complete...");
            
            // Wait for animation to complete
            yield return new WaitForSeconds(deathAnimLength);
            
            Debug.Log($"Player{playerID} death animation finished!");
            
            // For Player2: Destroy GameObject after death animation
            if (playerID == 2)
            {
                Debug.Log($"Player{playerID} destroying GameObject...");
                Destroy(gameObject);
            }
            else
            {
                // For Player1: Just disable
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.isKinematic = true;
                }
                
                this.enabled = false;
                Debug.Log($"Player{playerID} script disabled, GameObject kept alive");
            }
        }
        
        

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw ground check
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }

            // Draw attack ranges
            if (attack1Point != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attack1Point.position, attack1Range);
                Gizmos.DrawLine(transform.position, attack1Point.position);
            }
            if (attack2Point != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(attack2Point.position, attack2Range);
                Gizmos.DrawLine(transform.position, attack2Point.position);
            }
            if (attack3Point != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(attack3Point.position, attack3Range);
                Gizmos.DrawLine(transform.position, attack3Point.position);
            }
        }
#endif
    }
}
