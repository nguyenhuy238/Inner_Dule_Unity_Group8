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
        private float counterAttackWindow = 0.25f; // Thời gian để kích hoạt phản đòn
        private bool hasDashedThroughOpponent = false; // Ngăn multi-hit trong 1 lần dash
        
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
                currentHealth = characterData.maxHealth;
            }
            else
            {
                Debug.LogError($"[InnerDuel] CharacterData missing on {gameObject.name}!");
                currentHealth = 100f;
            }
            
            if (healthBar != null)
            {
                healthBar.SetMaxHealth(currentHealth);
            }
        }
        
        private void Update()
        {
            if (isDead) return;
            
            UpdateTimers();
            HandleMovement();
            HandleAbilities();
            UpdateAnimator();
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
            // --- TEAM HOOK: Thêm logic kỹ năng đặc biệt của nhân vật tại đây ---
            HandleUniqueAbilities();
            // ----------------------------------------------------------------
        }
        
        private void HandleUniqueAbilities()
        {
            // Spontaneity: Dash Damage Logic
            if (characterData.type == CharacterType.Spontaneity && isDashing && !hasDashedThroughOpponent)
            {
                Collider2D[] hitOpponents = Physics2D.OverlapCircleAll(transform.position, 1f, opponentLayer);
                foreach (Collider2D opponent in hitOpponents)
                {
                    var opponentController = opponent.GetComponent<InnerCharacterController>();
                    if (opponentController != null)
                    {
                        opponentController.TakeDamage(characterData.attackDamage * 0.5f); // Dash deals 50% damage
                        hasDashedThroughOpponent = true;
                        
                        // Effect
                        if (InnerDuel.Effects.ParticleEffectsManager.Instance != null)
                            InnerDuel.Effects.ParticleEffectsManager.Instance.PlayEffect("Dash", transform.position, characterData.effectColor);
                    }
                }
            }
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
        }
        
        private void StopBlocking()
        {
            isBlocking = false;
            canMove = true;
        }
        
        private void Dash()
        {
            isDashing = true;
            canMove = false;
            dashCooldown = 1f; 
            hasDashedThroughOpponent = false; // Reset flash damage flag
            
            // Perform dash
            Vector2 dashDirection = lastMoveDirection != Vector2.zero ? lastMoveDirection : Vector2.right;
            rb.velocity = dashDirection * characterData.moveSpeed * characterData.dashSpeedMultiplier;
            
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
                // Kiểm tra Counter-attack cho Kỷ Luật (Discipline)
                if (characterData.canCounterAttack && (Time.time - lastBlockStartTime) <= counterAttackWindow)
                {
                    Debug.Log($"[InnerDuel] {characterData.characterName} triggered COUNTER-ATTACK!");
                    TriggerCounterAttack();
                    return; // Không nhận sát thương nếu phản đòn thành công (Perfect Block)
                }
                
                finalDamage *= 0.2f; // Block bình thường giảm 80% sát thương
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
        
        private void TriggerCounterAttack()
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
            isDead = true;
            canMove = false;
            rb.velocity = Vector2.zero;
            
            // Trigger death animation
            animator.SetTrigger("Die");
            
            // Notify game manager
            GameManager.Instance.OnCharacterDied(this);
        }
        
        public bool IsDead() => isDead;
    }
}
