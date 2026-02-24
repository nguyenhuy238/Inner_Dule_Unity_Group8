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
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
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
            // Block ability
            if (characterData.canBlock && blockCooldown <= 0)
            {
                // Handle block input (will be connected to Input System)
            }
            
            // Dash ability
            if (characterData.canDash && dashCooldown <= 0)
            {
                // Handle dash input (will be connected to Input System)
            }
            
            // Berserk mode
            if (characterData.hasBerserkMode && currentHealth <= characterData.maxHealth * 0.3f && !isInBerserkMode)
            {
                EnterBerserkMode();
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
            attackCooldown = 0.5f;
            
            // Check for opponents in range
            Collider2D[] hitOpponents = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, opponentLayer);
            
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
            
            // Perform dash
            Vector2 dashDirection = lastMoveDirection != Vector2.zero ? lastMoveDirection : Vector2.right;
            rb.velocity = dashDirection * characterData.moveSpeed * 3f;
            
            Invoke(nameof(ResetDash), 0.2f);
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
            
            if (isBlocking)
            {
                damage *= 0.2f; // Block reduces 80% damage
            }
            
            currentHealth -= damage;
            invincibilityTimer = invincibilityDuration; // Prevent multi-hit in 1 frame
            
            // Visual feedback for damage
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
