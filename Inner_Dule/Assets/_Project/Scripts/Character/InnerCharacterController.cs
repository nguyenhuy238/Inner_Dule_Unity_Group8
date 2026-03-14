using UnityEngine;
using UnityEngine.InputSystem;
using InnerDuel;
using InnerDuel.Input;
using System.Collections;

namespace InnerDuel.Characters
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class InnerCharacterController : MonoBehaviour
    {
        [Header("Character Setup")]
        public CharacterData characterData;
        [Tooltip("1 for Player1, 2 for Player2")] 
        public int playerID = 1;
        
        [Header("Movement Settings (Overrides Data if needed)")]
        public float moveSpeedMultiplier = 1f;
        public float jumpForceMultiplier = 1f;

        [Header("Combat References")]
        public Transform attack1Point;
        public Transform attack2Point;
        public Transform attack3Point;
        public Transform projectileSpawnPoint;
        public LayerMask opponentLayer;
        
        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.2f;
        public LayerMask groundLayer;
        
        [Header("Visuals")]
        public HealthBar healthBar;
        
        // Components
        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private InnerDuel.Core.StatusEffects.StatusEffectManager statusEffectManager;
        private InputManager inputManager;
        
        // State variables
        private float currentHealth;
        private bool isDead = false;
        private bool isGrounded = false;
        private bool isAttacking = false;
        private bool isBlocking = false;
        private bool isDashing = false;
        private bool canMove = true;
        private float controlLockUntil = 0f;
        private float lastBlockStartTime = 0f;

        // Public Properties for Abilities & UI
        public bool IsBlocking => isBlocking;
        public bool IsDashing => isDashing;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => characterData != null ? characterData.maxHealth : 100f;
        public float LastBlockStartTime => lastBlockStartTime;
        
        // Input Buffers
        private Vector2 moveInput;
        private bool jumpQueued;
        
        // Timers
        private float attack1Cooldown = 0f;
        private float attack2Cooldown = 0f;
        private float attack3Cooldown = 0f;
        private float dashCooldown = 0f;
        private float blockCooldown = 0f;
        private float invincibilityTimer = 0f;
        private float invincibilityDuration = 0.2f;
        
        // Abilities list
        private System.Collections.Generic.List<BaseCharacterAbility> abilities = new System.Collections.Generic.List<BaseCharacterAbility>();

        // Animator Parameters Cache
        private int animMoveSpeed;
        private int animIsGrounded;
        private int animVerticalSpeed;
        private int animIsAttacking;
        private int animIsBlocking;
        private int animIsDead;
        private int animHit;
        private int animAttack1;
        private int animAttack2;
        private int animAttack3;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            statusEffectManager = GetComponent<InnerDuel.Core.StatusEffects.StatusEffectManager>();
            
            // Ensure StatusEffectManager
            if (statusEffectManager == null)
                statusEffectManager = gameObject.AddComponent<InnerDuel.Core.StatusEffects.StatusEffectManager>();

            // Auto-setup Transforms if missing
            SetupTransforms();
            
            // Initialize Animation Hashes
            animMoveSpeed = Animator.StringToHash("MoveSpeed");
            animIsGrounded = Animator.StringToHash("IsGrounded");
            animVerticalSpeed = Animator.StringToHash("VerticalSpeed");
            animIsAttacking = Animator.StringToHash("IsAttacking");
            animIsBlocking = Animator.StringToHash("IsBlocking");
            animIsDead = Animator.StringToHash("IsDead");
            animHit = Animator.StringToHash("Hit");
            animAttack1 = Animator.StringToHash("Attack1");
            animAttack2 = Animator.StringToHash("Attack2");
            animAttack3 = Animator.StringToHash("Attack3");

            // Initialize Data
            if (characterData != null)
            {
                InitializeFromData();
            }
            else
            {
                currentHealth = 100f;
            }
        }
        
        private void Start()
        {
            // Initialize Input Manager
            inputManager = InputManager.InstanceSafe();
            
            // Warning for missing ground layer
            if (groundLayer == 0)
            {
                Debug.LogError($"[InnerDuel] {gameObject.name} has no groundLayer set! Jump will NOT work. Please set it in the Inspector.");
            }

            // Initialize HealthBar
            if (healthBar != null)
            {
                healthBar.SetMaxHealth(currentHealth);
                healthBar.SetHealth(currentHealth);
            }
            
            // Collect Abilities
            abilities.AddRange(GetComponents<BaseCharacterAbility>());
            foreach (var ability in abilities)
            {
                ability.Initialize(this, characterData);
            }
        }

        private void SetupTransforms()
        {
            if (groundCheck == null)
            {
                var go = new GameObject("GroundCheck");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0f, -0.6f, 0f);
                groundCheck = go.transform;
            }

            if (attack1Point == null)
            {
                var go = new GameObject("Attack1Point");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0.8f, 0f, 0f);
                attack1Point = go.transform;
            }
            
            if (attack2Point == null)
            {
                var go = new GameObject("Attack2Point");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(1.0f, 0f, 0f);
                attack2Point = go.transform;
            }
            
            if (attack3Point == null)
            {
                var go = new GameObject("Attack3Point");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(1.2f, 0.5f, 0f); // Higher/Further
                attack3Point = go.transform;
            }
            
            if (projectileSpawnPoint == null)
            {
                 var go = new GameObject("ProjectileSpawnPoint");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(1.0f, 0.5f, 0f);
                projectileSpawnPoint = go.transform;
            }
        }

        public void InitializeFromData()
        {
            if (characterData == null) return;
            currentHealth = characterData.maxHealth;
            if (healthBar != null) healthBar.SetMaxHealth(currentHealth);
        }

        private void Update()
        {
            if (isDead) return;

            UpdateTimers();
            HandleInput();
            UpdateAnimator();
            
            // Ability Updates
            foreach (var ability in abilities) ability.OnUpdate();
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            CheckGround();
            HandleMovement();
        }

        private void UpdateTimers()
        {
            if (attack1Cooldown > 0) attack1Cooldown -= Time.deltaTime;
            if (attack2Cooldown > 0) attack2Cooldown -= Time.deltaTime;
            if (attack3Cooldown > 0) attack3Cooldown -= Time.deltaTime;
            if (dashCooldown > 0) dashCooldown -= Time.deltaTime;
            if (blockCooldown > 0) blockCooldown -= Time.deltaTime;
            if (invincibilityTimer > 0) invincibilityTimer -= Time.deltaTime;
        }

        private void HandleInput()
        {
            if (inputManager == null) return;

            // Movement Input
            moveInput = inputManager.GetMoveInput(playerID);

            // Stun Check
            if (statusEffectManager != null && statusEffectManager.HasEffect("Stun"))
            {
                moveInput = Vector2.zero;
                return;
            }

            // Jump
            bool jumpRequested = inputManager.GetButtonDown(playerID, "Jump");
            if (jumpRequested)
            {
                if (isGrounded && canMove && !isAttacking && !isBlocking)
                {
                    jumpQueued = true;
                    Debug.Log($"[InnerDuel] Player {playerID} Jump Queued!");
                }
                else
                {
                    Debug.Log($"[InnerDuel] Player {playerID} Jump Denied. Grounded: {isGrounded}, CanMove: {canMove}, Attacking: {isAttacking}, Blocking: {isBlocking}");
                }
            }

            // Attacks
            if (canMove && !isBlocking && !isDashing) // Can attack while moving (air or ground) usually? Let's say yes but it locks movement
            {
                if (inputManager.GetButtonDown(playerID, "Attack1") && attack1Cooldown <= 0)
                {
                    PerformAttack(1);
                }
                else if (inputManager.GetButtonDown(playerID, "Attack2") && attack2Cooldown <= 0)
                {
                    PerformAttack(2);
                }
                else if (inputManager.GetButtonDown(playerID, "Attack3") && attack3Cooldown <= 0)
                {
                    PerformAttack(3);
                }
            }

            // Dash
            if (characterData != null && characterData.canDash && inputManager.GetButtonDown(playerID, "Dash"))
            {
                if (!isDashing && dashCooldown <= 0 && canMove) PerformDash();
            }

            // Block
            if (characterData != null && characterData.canBlock)
            {
                bool blockHeld = inputManager.GetButton(playerID, "Block");
                if (blockHeld && !isAttacking && !isDashing)
                {
                    if (!isBlocking) StartBlocking();
                }
                else if (!blockHeld && isBlocking)
                {
                    StopBlocking();
                }
            }
        }

        private void CheckGround()
        {
            bool wasGrounded = isGrounded;
            isGrounded = false;
            
            Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
            foreach (var hit in hits)
            {
                // Ensure we don't ground on ourselves, triggers, or our own child hitboxes
                if (hit.gameObject != gameObject && !hit.isTrigger && !hit.transform.IsChildOf(transform))
                {
                    isGrounded = true;
                    break;
                }
            }

            if (isGrounded && !wasGrounded)
            {
                Debug.Log($"[InnerDuel] Player {playerID} Landed.");
            }
        }

        private void HandleMovement()
        {
            // Jump Execution - check this before early returns to ensure it's not missed 
            // if an attack/block starts in the same frame as the jump execution.
            if (jumpQueued)
            {
                float jumpForce = characterData != null ? characterData.jumpForce : 12f;
                jumpForce *= jumpForceMultiplier;
                
                rb.velocity = new Vector2(rb.velocity.x, 0f); // Reset Y velocity for consistent jump height
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                
                jumpQueued = false;
                isGrounded = false;
                
                if (animator != null) animator.SetBool(animIsGrounded, false);
                Debug.Log($"[InnerDuel] Player {playerID} Jump Executed!");
            }

            // Control Lock (e.g. during Leap Attack)
            if (Time.time < controlLockUntil)
            {
                // Just apply gravity/drag, don't override X velocity
                return;
            }

            // Stop movement if Blocking or Attacking (unless aerial drift allowed? usually no in strict 2D fighters)
            // Allow some air control?
            if (!canMove || isAttacking || isBlocking || isDashing)
            {
                if (isGrounded)
                {
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                }
                else
                {
                    // Air drag
                    rb.velocity = new Vector2(rb.velocity.x * 0.95f, rb.velocity.y);
                }
                return;
            }

            // Calculate Speed
            float speed = characterData != null ? characterData.moveSpeed : 5f;
            speed *= moveSpeedMultiplier;
            
            float targetVelocityX = moveInput.x * speed;
            
            // Air Control
            if (!isGrounded && characterData != null)
            {
                // Smoothly interpolate to target, but slower control in air
                 rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, targetVelocityX, characterData.airControlMultiplier), rb.velocity.y);
            }
            else
            {
                // Instant ground movement
                rb.velocity = new Vector2(targetVelocityX, rb.velocity.y);
            }

            // Facing Direction
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                bool faceRight = moveInput.x > 0;
                FlipCharacter(faceRight);
            }
        }

        private void FlipCharacter(bool faceRight)
        {
            // For Player 2, default sprite might be facing Left. 
            // Standard convention: Sprite faces Right by default.
            // If sprite faces Left by default, invert logic.
            // Assuming standard Right-facing sprites:
            
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !faceRight;
            }
            
            // Flip Attack Points
            float direction = faceRight ? 1f : -1f;
            UpdateAttackPoint(attack1Point, direction);
            UpdateAttackPoint(attack2Point, direction);
            UpdateAttackPoint(attack3Point, direction);
            UpdateAttackPoint(projectileSpawnPoint, direction);
        }
        
        private void UpdateAttackPoint(Transform point, float direction)
        {
            if (point == null) return;
            Vector3 pos = point.localPosition;
            point.localPosition = new Vector3(Mathf.Abs(pos.x) * direction, pos.y, pos.z);
        }

        private void PerformAttack(int attackIndex)
        {
            isAttacking = true;
            canMove = false;
            rb.velocity = new Vector2(0, rb.velocity.y); // Stop moving when attacking on ground

            float damage = 10f;
            float range = 1f;
            float cooldown = 0.5f;
            Transform point = attack1Point;
            string triggerName = "Attack1";

            // Load data based on index
            if (characterData != null)
            {
                switch (attackIndex)
                {
                    case 1:
                        damage = characterData.attack1Damage;
                        range = characterData.attack1Range;
                        cooldown = characterData.attack1Cooldown;
                        point = attack1Point;
                        triggerName = "Attack1";
                        break;
                    case 2:
                        damage = characterData.attack2Damage;
                        range = characterData.attack2Range;
                        cooldown = characterData.attack2Cooldown;
                        point = attack2Point;
                        triggerName = "Attack2";
                        break;
                    case 3:
                        damage = characterData.attack3Damage;
                        range = characterData.attack3Range;
                        cooldown = characterData.attack3Cooldown;
                        point = attack3Point;
                        triggerName = "Attack3";
                        break;
                }
            }

            // Set Cooldown
            switch (attackIndex)
            {
                case 1: attack1Cooldown = cooldown; break;
                case 2: attack2Cooldown = cooldown; break;
                case 3: attack3Cooldown = cooldown; break;
            }

            // Animation
            if (animator != null)
            {
                animator.SetTrigger(triggerName);
                animator.SetBool(animIsAttacking, true);
            }

            // Notify abilities
            foreach (var ability in abilities)
            {
                ability.OnAttack();
                if (attackIndex == 1) ability.OnSkill1();
                else if (attackIndex == 2) ability.OnSkill2();
                else if (attackIndex == 3) ability.OnSkill3();
            }
            
            // Special Logic: Leap (Attack 3)
            if (attackIndex == 3 && characterData != null && characterData.attack3LeapForce != Vector2.zero)
            {
                // Leap
                float dir = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;
                Vector2 force = new Vector2(characterData.attack3LeapForce.x * dir, characterData.attack3LeapForce.y);
                rb.AddForce(force, ForceMode2D.Impulse);
                controlLockUntil = Time.time + characterData.attack3ControlLock;
            }

            // Special Logic: Projectile (Player 2 / Ranged)
            // Check if we should spawn projectile. 
            // Simple logic: If prefab exists and it's Attack 1 or 3 (classic shoto/zoning).
            // Or just check playerID for now to match legacy.
            bool isProjectile = (characterData != null && characterData.projectilePrefab != null && (attackIndex == 1 || attackIndex == 3) && playerID == 2);
            
            if (isProjectile)
            {
                StartCoroutine(SpawnProjectileRoutine(0.2f, damage));
            }
            else
            {
                // Melee Hitbox
                StartCoroutine(MeleeAttackRoutine(0.2f, point, range, damage));
            }

            // Reset State
            float recoveryTime = 0.4f; // Default recovery
            StartCoroutine(ResetAttackState(recoveryTime));
        }

        private IEnumerator MeleeAttackRoutine(float delay, Transform point, float range, float damage)
        {
            yield return new WaitForSeconds(delay);
            
            if (point == null) yield break;
            
            Collider2D[] hits = Physics2D.OverlapCircleAll(point.position, range, opponentLayer);
            foreach (var hit in hits)
            {
                // Try InnerCharacterController
                var target = hit.GetComponent<InnerCharacterController>();
                if (target != null && !target.isDead)
                {
                    target.TakeDamage(damage);
                }
            }
        }

        private IEnumerator SpawnProjectileRoutine(float delay, float damage)
        {
            yield return new WaitForSeconds(delay);
            
            if (characterData != null && characterData.projectilePrefab != null)
            {
                GameObject projObj = Instantiate(characterData.projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
                
                // Setup Projectile Component
                var coreProj = projObj.GetComponent<InnerDuel.Core.Projectile>();
                if (coreProj != null)
                {
                    // Direction
                    float dirX = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;
                    Vector2 dir = new Vector2(dirX, 0);
                    coreProj.Initialize(dir, playerID, damage, opponentLayer);
                }
                else
                {
                    // Legacy/Effects projectile fallback
                    // Just set velocity if it has RB
                    var rbProj = projObj.GetComponent<Rigidbody2D>();
                    if (rbProj != null)
                    {
                         float dirX = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;
                         rbProj.velocity = new Vector2(dirX * 15f, 0f);
                    }
                }
            }
        }

        private IEnumerator ResetAttackState(float delay)
        {
            yield return new WaitForSeconds(delay);
            isAttacking = false;
            canMove = true;
            if (animator != null) animator.SetBool(animIsAttacking, false);
        }

        private void PerformDash()
        {
            isDashing = true;
            canMove = false;
            dashCooldown = characterData.dashDuration + 0.5f; // Cooldown > Duration
            
            // Dash Force
            float dir = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;
            float dashSpeed = characterData.moveSpeed * characterData.dashSpeedMultiplier;
            
            rb.velocity = new Vector2(dir * dashSpeed, 0f); // Horizontal Dash
            
            StartCoroutine(ResetDashState(characterData.dashDuration));
        }

        private IEnumerator ResetDashState(float delay)
        {
            yield return new WaitForSeconds(delay);
            isDashing = false;
            canMove = true;
            rb.velocity = Vector2.zero;
        }

        private void StartBlocking()
        {
            isBlocking = true;
            canMove = false;
            rb.velocity = Vector2.zero;
            lastBlockStartTime = Time.time;
            if (animator != null) animator.SetBool(animIsBlocking, true);
            foreach (var ability in abilities) ability.OnBlockStart();
        }

        private void StopBlocking()
        {
            isBlocking = false;
            canMove = true;
            if (animator != null) animator.SetBool(animIsBlocking, false);
            foreach (var ability in abilities) ability.OnBlockEnd();
        }

        public void TakeDamage(float damage)
        {
            if (isDead || invincibilityTimer > 0) return;

            // Notify abilities first (for parry/counter logic)
            foreach (var ability in abilities) ability.OnTakeDamage(damage);

            // Re-check if still alive/valid after ability logic
            if (isDead) return;

            // Block Logic
            if (isBlocking)
            {
                damage *= 0.2f; // Block 80% damage
            }
            
            currentHealth -= damage;
            invincibilityTimer = invincibilityDuration;
            
            if (healthBar != null) healthBar.SetHealth(currentHealth);
            
            // Animation
            if (animator != null) animator.SetTrigger(animHit);
            
            // Death Check
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void TriggerCounterAttack()
        {
            // Reset block
            StopBlocking();
            
            // Perform instant attack (e.g. Attack 1)
            PerformAttack(1);
            
            Debug.Log($"[InnerDuel] {gameObject.name} triggered COUNTER ATTACK!");
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;
            canMove = false;
            rb.velocity = Vector2.zero;
            
            if (animator != null) animator.SetBool(animIsDead, true);
            
            GameManager.Instance.OnCharacterDied(this);
        }

        public bool IsDead() => isDead;

        private void UpdateAnimator()
        {
            if (animator == null) return;

            animator.SetFloat(animMoveSpeed, Mathf.Abs(rb.velocity.x));
            animator.SetFloat(animVerticalSpeed, rb.velocity.y);
            animator.SetBool(animIsGrounded, isGrounded);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
            if (attack1Point != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attack1Point.position, characterData != null ? characterData.attack1Range : 0.5f);
            }
        }
#endif
    }
}
