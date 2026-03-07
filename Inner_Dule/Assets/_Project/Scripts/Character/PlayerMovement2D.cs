using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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

        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.15f;
        [Tooltip("Optional. If left empty, we will detect by tag 'Ground'.")] public LayerMask groundLayer;

        [Header("Projectile (Player 2 Only)")]
        [Tooltip("Fireball prefab for Player 2's Attack1 and Attack3")]
        public GameObject fireballPrefab;
        public Transform fireballSpawnPoint;
        public float fireballSpeed = 15f;

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

        // State
        private Vector2 moveInput;
        private bool isGrounded;
        private bool jumpQueued;
        private float controlLockUntil;

        // Combat state
        private float currentHealth;
        private bool isDead = false;
        private float invincibilityTimer = 0f;
        private float invincibilityDuration = 0.3f;
        
        // Attack state
        private bool isAttacking = false;
        private float attack1CooldownTimer = 0f;
        private float attack2CooldownTimer = 0f;
        private float attack3CooldownTimer = 0f;
        private const float attack1Cooldown = 1f;  // 1 giây
        private const float attack2Cooldown = 5f;  // 5 giây
        private const float attack3Cooldown = 8f;  // 8 giây

        // Animator parameter presence cache (to tolerate different controllers)
        private bool hasIsRunning;
        private bool hasIsRunningPascal;
        private bool hasIsJumping;
        private bool hasIsJumpingPascal;
        private bool hasGrounded;
        private bool hasIsGrounded;
        private bool hasSpeed;
        private bool hasMoveSpeed;
        private bool hasAttack1;
        private bool hasAttack2;
        private bool hasAttack3;
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

            // Auto-create a ground check if none assigned
            if (groundCheck == null)
            {
                var go = new GameObject("GroundCheck");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0f, -0.5f, 0f);
                groundCheck = go.transform;
            }

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
                else
                {
                    Debug.LogError($"Player{playerID} HealthBar.healthSlider is NULL! Please assign it in Inspector.");
                }
                
                healthBar.SetMaxHealth(maxHealth);
                healthBar.SetHealth(currentHealth);
                Debug.Log($"Player{playerID} HealthBar initialized");
            }
            else
            {
                Debug.LogError($"Player{playerID} HealthBar is NULL! Please assign HealthBar in Inspector.");
            }

            // Cache animator parameter availability to avoid mismatches
            if (animator != null)
            {
                foreach (var p in animator.parameters)
                {
                    switch (p.name)
                    {
                        case "isRunning": hasIsRunning = p.type == AnimatorControllerParameterType.Bool; break;
                        case "IsRunning": hasIsRunningPascal = p.type == AnimatorControllerParameterType.Bool; break;
                        case "isJumping": hasIsJumping = p.type == AnimatorControllerParameterType.Bool; break;
                        case "IsJumping": hasIsJumpingPascal = p.type == AnimatorControllerParameterType.Bool; break;
                        case "Grounded": hasGrounded = p.type == AnimatorControllerParameterType.Bool; break;
                        case "IsGrounded": hasIsGrounded = p.type == AnimatorControllerParameterType.Bool; break;
                        case "Speed": hasSpeed = p.type == AnimatorControllerParameterType.Float; break;
                        case "MoveSpeed": hasMoveSpeed = p.type == AnimatorControllerParameterType.Float; break;
                        case "Attack1": hasAttack1 = p.type == AnimatorControllerParameterType.Trigger; break;
                        case "Attack2": hasAttack2 = p.type == AnimatorControllerParameterType.Trigger; break;
                        case "Attack3": hasAttack3 = p.type == AnimatorControllerParameterType.Trigger; break;
                        case "IsAttacking": hasIsAttacking = p.type == AnimatorControllerParameterType.Bool; break;
                        case "IsActtack": hasIsActtack = p.type == AnimatorControllerParameterType.Bool; break;
                        case "Hit": hasHit = p.type == AnimatorControllerParameterType.Trigger; break;
                        case "hit": hasHitLowercase = p.type == AnimatorControllerParameterType.Trigger; break;
                        case "isDead": hasIsDead = p.type == AnimatorControllerParameterType.Bool; break;
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
                // Player 1: A/D move, Space jump, J/K/L attacks
                if (playerID == 1)
                {
                    if (Keyboard.current.aKey.isPressed) x -= 1f;
                    if (Keyboard.current.dKey.isPressed) x += 1f;

                    // Jump on Space
                    if (Keyboard.current.spaceKey.wasPressedThisFrame)
                    {
                        jumpQueued = true;
                    }

                    // Attacks: J/K/L
                    if (animator != null)
                    {
                        if (Keyboard.current.jKey != null && Keyboard.current.jKey.wasPressedThisFrame)
                        {
                            if (attack1CooldownTimer <= 0)
                            {
                                if (hasAttack1) animator.SetTrigger("Attack1");
                                PerformMeleeAttack(1); // Attack 1
                                attack1CooldownTimer = attack1Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.5f)); // Khóa movement 0.5s
                            }
                            else
                            {
                                Debug.Log($"Player1 Attack1 on cooldown! {attack1CooldownTimer:F1}s remaining");
                            }
                        }
                        if (Keyboard.current.kKey != null && Keyboard.current.kKey.wasPressedThisFrame)
                        {
                            if (attack2CooldownTimer <= 0)
                            {
                                if (hasAttack2) animator.SetTrigger("Attack2");
                                PerformMeleeAttack(2); // Attack 2
                                attack2CooldownTimer = attack2Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.6f)); // Khóa movement 0.6s
                            }
                            else
                            {
                                Debug.Log($"Player1 Attack2 on cooldown! {attack2CooldownTimer:F1}s remaining");
                            }
                        }
                        if (Keyboard.current.lKey != null && Keyboard.current.lKey.wasPressedThisFrame)
                        {
                            if (attack3CooldownTimer <= 0)
                            {
                                if (hasAttack3) animator.SetTrigger("Attack3");
                                PerformMeleeAttack(3); // Attack 3
                                attack3CooldownTimer = attack3Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.8f)); // Khóa movement 0.8s

                                // Apply leap for Attack3: forward + upward impulse
                                // Player1: flipX true = facing left, flipX false = facing right
                                Vector2 leap = new Vector2((sprite != null && sprite.flipX) ? -attack3ForwardImpulse : attack3ForwardImpulse,
                                                           attack3UpImpulse);
                                rb.velocity = new Vector2(rb.velocity.x, 0f);
                                rb.AddForce(leap, ForceMode2D.Impulse);
                                controlLockUntil = Time.time + attack3ControlLock;
                            }
                            else
                            {
                                Debug.Log($"Player1 Attack3 on cooldown! {attack3CooldownTimer:F1}s remaining");
                            }
                        }
                    }
                }
                // Player 2: Arrow Keys (Left/Right) move, Arrow Up jump, 1/2/3 attacks (3 requires jump)
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

                    // Attacks: Numpad 1/2/3
                    if (animator != null)
                    {
                        // Attack1 (Fireball) - Numpad 1
                        if (Keyboard.current.numpad1Key != null && Keyboard.current.numpad1Key.wasPressedThisFrame)
                        {
                            if (attack1CooldownTimer <= 0)
                            {
                                if (hasAttack1) animator.SetTrigger("Attack1");
                                if (hasIsAttacking) animator.SetBool("IsAttacking", true);
                                if (hasIsActtack) animator.SetBool("IsActtack", true);
                                StartCoroutine(ResetAttackBool(0.5f));
                                StartCoroutine(SpawnFireball(0.2f)); // Delay để animation bắt đầu
                                PerformMeleeAttack(1); // Attack 1: melee damage if in range
                                attack1CooldownTimer = attack1Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.5f)); // Khóa movement 0.5s
                            }
                            else
                            {
                                Debug.Log($"Player2 Attack1 on cooldown! {attack1CooldownTimer:F1}s remaining");
                            }
                        }
                        // Attack2 - Numpad 2
                        if (Keyboard.current.numpad2Key != null && Keyboard.current.numpad2Key.wasPressedThisFrame)
                        {
                            if (attack2CooldownTimer <= 0)
                            {
                                Debug.Log($"Player2 Attack2! hasAttack2={hasAttack2} hasIsActtack={hasIsActtack}");
                                if (hasAttack2) animator.SetTrigger("Attack2");
                                if (hasIsAttacking) animator.SetBool("IsAttacking", true);
                                if (hasIsActtack) animator.SetBool("IsActtack", true);
                                StartCoroutine(ResetAttackBool(0.5f));
                                PerformMeleeAttack(2); // Attack 2: deal damage
                                attack2CooldownTimer = attack2Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.6f)); // Khóa movement 0.6s
                            }
                            else
                            {
                                Debug.Log($"Player2 Attack2 on cooldown! {attack2CooldownTimer:F1}s remaining");
                            }
                        }
                        // Attack3 (Jump + Fireball) - Numpad 3
                        if (Keyboard.current.numpad3Key != null && Keyboard.current.numpad3Key.wasPressedThisFrame)
                        {
                            if (attack3CooldownTimer <= 0)
                            {
                                if (hasAttack3) animator.SetTrigger("Attack3");
                                if (hasIsAttacking) animator.SetBool("IsAttacking", true);
                                if (hasIsActtack) animator.SetBool("IsActtack", true);
                                StartCoroutine(ResetAttackBool(0.5f));
                                StartCoroutine(SpawnFireball(0.3f)); // Delay cho animation nhảy
                                PerformMeleeAttack(3); // Attack 3: melee damage if in range
                                attack3CooldownTimer = attack3Cooldown;
                                StartCoroutine(LockMovementDuringAttack(0.8f)); // Khóa movement 0.8s

                                // Apply leap for Attack3: forward + upward impulse
                                // Player2: flipX true = facing right, flipX false = facing left (reversed from Player1)
                                Vector2 leap = new Vector2((sprite != null && sprite.flipX) ? attack3ForwardImpulse : -attack3ForwardImpulse,
                                                           attack3UpImpulse);
                                rb.velocity = new Vector2(rb.velocity.x, 0f);
                                rb.AddForce(leap, ForceMode2D.Impulse);
                                controlLockUntil = Time.time + attack3ControlLock;
                            }
                            else
                            {
                                Debug.Log($"Player2 Attack3 on cooldown! {attack3CooldownTimer:F1}s remaining");
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
            // Robust ground detection: ignore own colliders and triggers
            bool grounded = false;
            Collider2D[] hits;
            if (groundLayer.value != 0)
            {
                hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
            }
            else
            {
                hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
            }
            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h == null) continue;
                if (h.attachedRigidbody == rb) continue; // skip self
                if (h.isTrigger) continue; // ignore triggers
                if (groundLayer.value == 0 && !h.CompareTag("Ground")) continue; // tag filter when no layer mask
                grounded = true;
                break;
            }
            isGrounded = grounded;

            // Horizontal movement
            float targetXVel = moveInput.x * moveSpeed;
            float control = isGrounded ? 1f : airControlMultiplier;

            // Khóa movement khi đang tấn công
            if (isAttacking)
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

            // Jump
            if (jumpQueued && isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            jumpQueued = false;

            // Animator + facing after physics & ground were resolved
            UpdateAnimatorBooleans();
            FlipByDirection(rb.velocity.x);
        }

        private void UpdateAnimatorBooleans()
        {
            if (animator == null) return;
            bool running = Mathf.Abs(rb.velocity.x) > 0.05f && isGrounded;
            bool jumping = !isGrounded;

            if (hasIsRunning) animator.SetBool("isRunning", running);
            if (hasIsRunningPascal) animator.SetBool("IsRunning", running);
            if (hasIsJumping) animator.SetBool("isJumping", jumping);
            if (hasIsJumpingPascal) animator.SetBool("IsJumping", jumping);
            if (hasGrounded) animator.SetBool("Grounded", isGrounded);
            if (hasIsGrounded) animator.SetBool("IsGrounded", isGrounded);
            if (hasSpeed) animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            if (hasMoveSpeed) animator.SetFloat("MoveSpeed", rb.velocity.magnitude);
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
            
            if (fireballPrefab == null || playerID != 2) yield break;

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
            
            // Spawn fireball
            GameObject fireball = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
            
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
                // Try to damage via PlayerMovement2D
                var enemyPlayer = enemy.GetComponent<PlayerMovement2D>();
                if (enemyPlayer != null)
                {
                    enemyPlayer.TakeDamage(attackDamage);
                    Debug.Log($"Player{playerID} Attack{attackNumber} dealt {attackDamage} damage to {enemy.name}");
                }
                
                // Try to damage via InnerCharacterController (if exists)
                var enemyController = enemy.GetComponent<InnerCharacterController>();
                if (enemyController != null)
                {
                    enemyController.TakeDamage(attackDamage);
                    Debug.Log($"Player{playerID} Attack{attackNumber} dealt {attackDamage} damage to {enemy.name}");
                }
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

            currentHealth -= damage;
            invincibilityTimer = invincibilityDuration;

            Debug.Log($"Player{playerID} took {damage} damage! Current HP: {currentHealth}/{maxHealth}");

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
                
                // Set isDead parameter to trigger death animation
                if (hasIsDead)
                {
                    animator.SetBool("isDead", true);
                    Debug.Log($"Player{playerID} set isDead=true for death animation");
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
