using UnityEngine;
using UnityEngine.InputSystem;
using InnerDuel;

namespace InnerDuel.Characters
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class InnerCharacterController : MonoBehaviour
    {
        [Header("Character Setup")]
        public CharacterData characterData;
        public int playerID = 1; // Player 1 or Player 2
        
        [Header("Combat")]
        public Transform attackPoint;
        public float attackRange = 1f;
        public LayerMask opponentLayer;
        
        [Header("Health")]
        public HealthBar healthBar;
        
        // Components
        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private InnerDuel.Core.StatusEffects.StatusEffectManager statusEffectManager;
        
        // State variables
        private float currentHealth;
        private bool isDead = false;
        private bool isAttacking = false;
        private bool isBlocking = false;
        private bool isDashing = false;
        private bool canMove = true;
        
        // Movement
        private Vector2 moveInput;
        private Vector2 lastMoveDirection;
        
        // Timers
        private float attackCooldown = 0f;
        private float dashCooldown = 0f;
        private float blockCooldown = 0f;
        
        // Special abilities
        private float berserkTimer = 0f;
        private bool isInBerserkMode = false;
        private bool hasBerserkParam = false;
        
        private float invincibilityDuration = 0.2f;
        private float invincibilityTimer = 0f;
        
        // Counters & Windows
        private float lastBlockStartTime = 0f;
        
        // Ability System
        private System.Collections.Generic.List<BaseCharacterAbility> abilities = new System.Collections.Generic.List<BaseCharacterAbility>();
        
        // Properties for Abilities
        public bool IsDashing => isDashing;
        public bool IsBlocking => isBlocking;
        public float LastBlockStartTime => lastBlockStartTime;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            statusEffectManager = GetComponent<InnerDuel.Core.StatusEffects.StatusEffectManager>();
            
            // Auto-add StatusEffectManager if missing
            if (statusEffectManager == null)
            {
                statusEffectManager = gameObject.AddComponent<InnerDuel.Core.StatusEffects.StatusEffectManager>();
            }
            
            if (animator != null)
            {
                // Check if parameters exist to avoid warnings
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    if (param.name == "IsBerserk") hasBerserkParam = true;
                }
            }
            
            // Defensive: Auto-find AttackPoint if missing
            if (attackPoint == null)
            {
                attackPoint = transform.Find("AttackPoint");
                if (attackPoint == null)
                {
                    GameObject ap = new GameObject("AttackPoint");
                    ap.transform.SetParent(this.transform);
                    ap.transform.localPosition = new Vector3(1f, 0f, 0f);
                    attackPoint = ap.transform;
                    Debug.LogWarning($"[InnerDuel] Auto-assigned missing AttackPoint for {gameObject.name}");
                }
            }
            
            if (characterData != null)
            {
                InitializeFromData();
            }
            else
            {
                // Note: If spawned via Factory, data is set immediately after Instantiate,
                // so we don't need to error here, but we should handle it in Start or via setter.
                currentHealth = 100f;
            }
            
            if (healthBar != null)
            {
                healthBar.SetMaxHealth(currentHealth);
            }

            // Initialize Abilities
            abilities.AddRange(GetComponents<BaseCharacterAbility>());
            foreach (var ability in abilities)
            {
                ability.Initialize(this, characterData);
            }
        }

        public void InitializeFromData()
        {
            if (characterData == null) return;
            
            currentHealth = characterData.maxHealth;
            if (healthBar != null) healthBar.SetMaxHealth(currentHealth);
            
            // Refresh and initialize abilities
            abilities.Clear();
            abilities.AddRange(GetComponents<BaseCharacterAbility>());
            foreach (var ability in abilities)
            {
                ability.Initialize(this, characterData);
            }
        }
        
        private void Update()
        {
            if (isDead || characterData == null) return;
            
            UpdateTimers();
            HandleMovement();
            HandleAbilities();
            UpdateAnimator();

            foreach (var ability in abilities) ability.OnUpdate();
        }
        
        private void UpdateTimers()
        {
            if (attackCooldown > 0) attackCooldown -= Time.deltaTime;
            if (dashCooldown > 0) dashCooldown -= Time.deltaTime;
            if (blockCooldown > 0) blockCooldown -= Time.deltaTime;
            if (invincibilityTimer > 0) invincibilityTimer -= Time.deltaTime;
            
            if (berserkTimer > 0)
            {
                berserkTimer -= Time.deltaTime;
                if (berserkTimer <= 0)
                {
                    ExitBerserkMode();
                }
            }
        }
        
        private void HandleMovement()
        {
            if (!canMove || isAttacking || isDashing) return;
            
            // Kiểm tra trạng thái Stun
            if (statusEffectManager != null && statusEffectManager.HasEffect("Stun"))
            {
                rb.velocity = Vector2.zero;
                return;
            }

            if (rb == null) return;
            rb.velocity = moveInput * characterData.moveSpeed;
            
            if (moveInput != Vector2.zero)
            {
                lastMoveDirection = moveInput.normalized;
            }
            
            // Flip sprite based on direction
            if (lastMoveDirection.x != 0)
            {
                spriteRenderer.flipX = lastMoveDirection.x < 0;
            }
        }
        private void HandleAbilities()
        {
            // Logic đặc biệt đã được chuyển sang hệ thống Ability rời (BaseCharacterAbility)
        }
        
        private void UpdateAnimator()
        {
            animator.SetFloat("MoveSpeed", rb.velocity.magnitude);
            animator.SetBool("IsAttacking", isAttacking);
            animator.SetBool("IsBlocking", isBlocking);
            animator.SetBool("IsDead", isDead);
            if (hasBerserkParam) animator.SetBool("IsBerserk", isInBerserkMode);
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }
        
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (statusEffectManager != null && statusEffectManager.HasEffect("Stun")) return;

            if (context.performed && !isDead && !isAttacking && attackCooldown <= 0)
            {
                Attack();
            }
        }
        
        public void OnBlock(InputAction.CallbackContext context)
        {
            if (!characterData.canBlock || isDead) return;
            
            if (context.performed && blockCooldown <= 0)
            {
                StartBlocking();
            }
            else if (context.canceled)
            {
                StopBlocking();
            }
        }
        
        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed && characterData.canDash && !isDead && !isDashing && dashCooldown <= 0)
            {
                Dash();
            }
        }
        
        private void Attack()
        {
            isAttacking = true;
            canMove = false;
            attackCooldown = characterData.attackCooldown; // Dùng thông số từ Data
            
            // Check for opponents in range
            Collider2D[] hitOpponents = Physics2D.OverlapCircleAll(attackPoint.position, characterData.attackRange, opponentLayer);
            
            foreach (Collider2D opponent in hitOpponents)
            {
                var opponentHealth = opponent.GetComponent<InnerCharacterController>();
                if (opponentHealth != null)
                {
                    float damage = isInBerserkMode ? characterData.attackDamage * 1.5f : characterData.attackDamage;
                    opponentHealth.TakeDamage(damage);
                }
            }
            
            foreach (var ability in abilities) ability.OnAttack();
            
            // Reset attack state
            Invoke(nameof(ResetAttack), 0.3f);
        }
        
        private void ResetAttack()
        {
            isAttacking = false;
            canMove = true;
        }
        
        private void StartBlocking()
        {
            isBlocking = true;
            canMove = false;
            blockCooldown = 0.2f;
            lastBlockStartTime = Time.time;
            
            animator.SetBool("IsBlocking", true);
            foreach (var ability in abilities) ability.OnBlockStart();
        }
        
        private void StopBlocking()
        {
            isBlocking = false;
            canMove = true;
            foreach (var ability in abilities) ability.OnBlockEnd();
        }
        
        private void Dash()
        {
            isDashing = true;
            canMove = false;
            dashCooldown = 1f; 
            
            // Perform dash
            Vector2 dashDirection = lastMoveDirection != Vector2.zero ? lastMoveDirection : Vector2.right;
            rb.velocity = dashDirection * characterData.moveSpeed * characterData.dashSpeedMultiplier;
            
            foreach (var ability in abilities) ability.OnDash();

            Invoke(nameof(ResetDash), characterData.dashDuration);
        }
        
        private void ResetDash()
        {
            isDashing = false;
            canMove = true;
            rb.velocity = Vector2.zero;
        }
        
        private void EnterBerserkMode()
        {
            isInBerserkMode = true;
            berserkTimer = 5f;
            spriteRenderer.color = Color.red;
            
            // Increase stats
            characterData.moveSpeed *= 1.5f;
            characterData.attackDamage *= 1.5f;
        }
        
        private void ExitBerserkMode()
        {
            isInBerserkMode = false;
            spriteRenderer.color = Color.white;
            
            // Reset stats
            characterData.moveSpeed /= 1.5f;
            characterData.attackDamage /= 1.5f;
        }
        
        public void TakeDamage(float damage)
        {
            if (isDead || invincibilityTimer > 0) return;
            
            // Tính toán phòng thủ
            float finalDamage = Mathf.Max(1f, damage - (characterData.defense * 0.5f));
            
            if (isBlocking)
            {
                // Thông báo tới các Ability để xử lý logic phản đòn hoặc giảm sát thương thêm
                foreach (var ability in abilities) ability.OnTakeDamage(damage);
                
                // Nếu bị Stun hoặc chết trong khi xử lý Ability (ví dụ phản đòn bị lỗi), thoát
                if (isDead) return;

                finalDamage *= 0.15f; // Block bình thường của Kỷ Luật giảm 85% sát thương
            }
            
            currentHealth -= finalDamage;
            invincibilityTimer = invincibilityDuration;
            
            // Visual feedback
            if (spriteRenderer != null) StartCoroutine(DamageFlash());
            
            if (healthBar != null)
            {
                healthBar.SetHealth(currentHealth);
            }
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        public void TriggerCounterAttack()
        {
            // Reset block
            StopBlocking();
            
            // Thực hiện đòn đánh tức thì
            Attack();
            
            // Hiệu ứng phản đòn
            if (InnerDuel.Effects.ParticleEffectsManager.Instance != null)
                InnerDuel.Effects.ParticleEffectsManager.Instance.PlayEffect("Parry", transform.position, Color.yellow);
        }
        
        private System.Collections.IEnumerator DamageFlash()
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        
        private void Die()
        {
            if (isDead) return; // Already dead
            
            isDead = true;
            canMove = false;
            rb.velocity = Vector2.zero;
            
            // Set isDead bool instead of trigger to prevent loops
            animator.SetBool("isDead", true);
            
            // Notify game manager
            GameManager.Instance.OnCharacterDied(this);
            foreach (var ability in abilities) ability.OnDie();
        }
        
        public bool IsDead() => isDead;
    }
}
