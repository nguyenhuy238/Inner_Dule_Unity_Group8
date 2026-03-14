using UnityEngine;
using UnityEngine.InputSystem;
using InnerDuel;
using InnerDuel.Input;

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

        // Input fallback (when PlayerInput callbacks aren't wired)
        private InputManager inputManager;
        
        // Timers
        private float attackCooldown = 0f;
        private float dashCooldown = 0f;
        private float blockCooldown = 0f;
        
        // Special abilities
        private float berserkTimer = 0f;
        private bool isInBerserkMode = false;
        private bool hasBerserkParam = false;

        // Animator parameter presence cache
        private bool hasMoveSpeedParam;
        private bool hasIsAttackingParam;
        private bool hasIsBlockingParam;
        private bool hasIsDeadParam;
        private bool hasIsDeadLowercaseParam;
        private bool hasAttackTrigger;
        private bool hasSkill1Trigger;
        private bool hasSkill2Trigger;
        private bool hasSkill3Trigger;
        private bool hasSkill2NewTrigger;
        private bool hasSkill3NewTrigger;
        
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

            // Optional: scene-level input manager fallback
            inputManager = InputManager.InstanceSafe();
            
            // Auto-add StatusEffectManager if missing
            if (statusEffectManager == null)
            {
                statusEffectManager = gameObject.AddComponent<InnerDuel.Core.StatusEffects.StatusEffectManager>();
            }
            
            if (animator != null)
            {
                // Cache animator parameters to avoid warnings when they don't exist
                foreach (var param in animator.parameters)
                {
                    switch (param.name)
                    {
                        case "MoveSpeed":
                            hasMoveSpeedParam = param.type == AnimatorControllerParameterType.Float;
                            break;
                        case "IsAttacking":
                            hasIsAttackingParam = param.type == AnimatorControllerParameterType.Bool;
                            break;
                        case "IsBlocking":
                            hasIsBlockingParam = param.type == AnimatorControllerParameterType.Bool;
                            break;
                        case "IsDead":
                            hasIsDeadParam = param.type == AnimatorControllerParameterType.Bool;
                            break;
                        case "isDead":
                            hasIsDeadLowercaseParam = param.type == AnimatorControllerParameterType.Bool;
                            break;
                        case "Attack":
                            hasAttackTrigger = param.type == AnimatorControllerParameterType.Trigger;
                            break;
                        case "Skill_1":
                        case "Skill1":
                            hasSkill1Trigger = param.type == AnimatorControllerParameterType.Trigger;
                            break;
                        case "Skill_2":
                        case "Skill2":
                            hasSkill2Trigger = param.type == AnimatorControllerParameterType.Trigger;
                            break;
                        case "Skill_3":
                        case "Skill3":
                            hasSkill3Trigger = param.type == AnimatorControllerParameterType.Trigger;
                            break;
                        case "Skill_2_new":
                            hasSkill2NewTrigger = param.type == AnimatorControllerParameterType.Trigger;
                            break;
                        case "Skill_3_new":
                            hasSkill3NewTrigger = param.type == AnimatorControllerParameterType.Trigger;
                            break;
                        case "IsBerserk":
                            hasBerserkParam = param.type == AnimatorControllerParameterType.Bool;
                            break;
                    }
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
            PollInputFallback();
            HandleMovement();
            HandleAbilities();
            UpdateAnimator();

            foreach (var ability in abilities) ability.OnUpdate();
        }

        private void PollInputFallback()
        {
            // If PlayerInput is properly wired, the OnMove/OnAttack/... callbacks will run and
            // this fallback will simply mirror the same state without breaking anything.
            if (inputManager == null)
            {
                inputManager = InputManager.InstanceSafe();
                if (inputManager == null) return;
            }

            // Movement
            moveInput = inputManager.GetMoveInput(playerID);

            // Combat inputs (mirror existing callback behavior)
            if (statusEffectManager != null && statusEffectManager.HasEffect("Stun")) return;

            if (inputManager.GetButtonDown(playerID, "Attack"))
            {
                if (!isDead && !isAttacking && attackCooldown <= 0) Attack();
            }

            if (inputManager.GetButtonDown(playerID, "Skill1"))
            {
                if (!isDead) TriggerSkill(1);
            }
            if (inputManager.GetButtonDown(playerID, "Skill2"))
            {
                if (!isDead) TriggerSkill(2);
            }
            if (inputManager.GetButtonDown(playerID, "Skill3"))
            {
                if (!isDead) TriggerSkill(3);
            }

            if (characterData != null && characterData.canDash && inputManager.GetButtonDown(playerID, "Dash"))
            {
                if (!isDead && !isDashing && dashCooldown <= 0) Dash();
            }

            if (characterData != null && characterData.canBlock)
            {
                bool blockHeld = inputManager.GetButton(playerID, "Block");
                if (blockHeld)
                {
                    if (!isDead && !isBlocking && blockCooldown <= 0) StartBlocking();
                }
                else
                {
                    if (isBlocking) StopBlocking();
                }
            }
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
            if (animator == null || animator.runtimeAnimatorController == null || rb == null)
                return;

            if (hasMoveSpeedParam)
            {
                animator.SetFloat("MoveSpeed", rb.velocity.magnitude);
            }

            if (hasIsAttackingParam)
            {
                animator.SetBool("IsAttacking", isAttacking);
            }

            if (hasIsBlockingParam)
            {
                animator.SetBool("IsBlocking", isBlocking);
            }

            if (hasIsDeadParam)
            {
                animator.SetBool("IsDead", isDead);
            }
            else if (hasIsDeadLowercaseParam)
            {
                animator.SetBool("isDead", isDead);
            }

            if (hasBerserkParam)
            {
                animator.SetBool("IsBerserk", isInBerserkMode);
            }
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

            if (animator != null && hasAttackTrigger)
            {
                animator.SetTrigger("Attack");
            }
            
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

        private void TriggerSkill(int skillIndex)
        {
            if (animator != null)
            {
                // Support both naming styles and the Spontaneity "*_new" clips
                switch (skillIndex)
                {
                    case 1:
                        if (hasSkill1Trigger) animator.SetTrigger(hasParameterNamed("Skill_1") ? "Skill_1" : "Skill1");
                        else if (hasParameterNamed("Skill_1")) animator.SetTrigger("Skill_1");
                        else if (hasParameterNamed("Skill1")) animator.SetTrigger("Skill1");
                        break;
                    case 2:
                        if (hasSkill2Trigger) animator.SetTrigger(hasParameterNamed("Skill_2") ? "Skill_2" : "Skill2");
                        else if (hasSkill2NewTrigger) animator.SetTrigger("Skill_2_new");
                        else if (hasParameterNamed("Skill_2")) animator.SetTrigger("Skill_2");
                        else if (hasParameterNamed("Skill2")) animator.SetTrigger("Skill2");
                        break;
                    case 3:
                        if (hasSkill3Trigger) animator.SetTrigger(hasParameterNamed("Skill_3") ? "Skill_3" : "Skill3");
                        else if (hasSkill3NewTrigger) animator.SetTrigger("Skill_3_new");
                        else if (hasParameterNamed("Skill_3")) animator.SetTrigger("Skill_3");
                        else if (hasParameterNamed("Skill3")) animator.SetTrigger("Skill3");
                        break;
                }
            }

            // Notify abilities (logic/effects can live there)
            foreach (var ability in abilities)
            {
                switch (skillIndex)
                {
                    case 1: ability.OnSkill1(); break;
                    case 2: ability.OnSkill2(); break;
                    case 3: ability.OnSkill3(); break;
                }
            }
        }

        private bool hasParameterNamed(string paramName)
        {
            if (animator == null) return false;
            foreach (var p in animator.parameters)
            {
                if (p.name == paramName) return true;
            }
            return false;
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
            
            // Set isDead parameter for death animation (support both naming variants)
            if (animator != null)
            {
                if (hasIsDeadParam)
                {
                    animator.SetBool("IsDead", true);
                }
                else if (hasIsDeadLowercaseParam)
                {
                    animator.SetBool("isDead", true);
                }
            }
            
            // Notify game manager
            GameManager.Instance.OnCharacterDied(this);
            foreach (var ability in abilities) ability.OnDie();
        }
        
        public bool IsDead() => isDead;
    }
}
