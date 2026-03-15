using UnityEngine;
using InnerDuel.Characters;
using InnerDuel.Audio;
using System.Collections;

namespace InnerDuel.Effects
{
    /// <summary>
    /// Simple projectile script for fireballs and other projectiles
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        public enum ProjectileType
        {
            Attack,
            Skill1,
            Skill2,
            Skill3
        }
        
        [Header("Projectile Settings")]
        [Tooltip("Type of projectile for sound effects")]
        public ProjectileType projectileType = ProjectileType.Attack;
        
        [Tooltip("Damage dealt to target")]
        public int damage = 10;
        
        [Tooltip("Lifetime before auto-destroy (seconds)")]
        public float lifetime = 5f;
        
        [Tooltip("Destroy on any collision?")]
        public bool destroyOnHit = true;
        
        [Tooltip("Tag of targets that can be damaged (leave empty to damage anything)")]
        public string targetTag = "";
        
        [Header("Explosion Settings")]
        [Tooltip("Play explosion animation before destroying?")]
        public bool playExplosionAnimation = true;
        
        [Tooltip("Time to wait for explosion animation to play before destroying")]
        public float explosionAnimationDuration = 0.6f;

        private Animator animator;
        private Rigidbody2D rb;
        private bool hasExploded = false;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            
            Debug.Log($"[Projectile] {gameObject.name} Awake - Animator: {(animator != null ? animator.runtimeAnimatorController?.name : "NULL")}");
        }

        private void Start()
        {
            // Auto-destroy after lifetime
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasExploded) return; // Already exploded, ignore further collisions
            
            Debug.Log($"[Projectile] OnTriggerEnter2D with {other.gameObject.name}, tag={other.tag}");
            
            // Check if the collider or its parent has the target tag
            bool isValidTarget = false;
            if (string.IsNullOrEmpty(targetTag))
            {
                isValidTarget = true; // No tag filter, accept all
            }
            else if (other.CompareTag(targetTag))
            {
                isValidTarget = true; // Direct match
                Debug.Log($"[Projectile] ✓ Direct tag match: {targetTag}");
            }
            else
            {
                // Check parent hierarchy for tag
                Transform current = other.transform;
                while (current != null)
                {
                    if (current.CompareTag(targetTag))
                    {
                        isValidTarget = true;
                        Debug.Log($"[Projectile] ✓ Parent tag match: {current.name} has tag {targetTag}");
                        break;
                    }
                    current = current.parent;
                }
            }
            
            if (!isValidTarget)
            {
                Debug.Log($"[Projectile] ✗ Tag mismatch: expected '{targetTag}', got '{other.tag}' (ignoring)");
                return;
            }

            // Deal damage and explode
            Debug.Log($"[Projectile] ✓ Valid target hit! Dealing damage and triggering explosion");
            DealDamage(other.gameObject);
            TriggerExplosion();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (hasExploded) return; // Already exploded, ignore further collisions
            
            Debug.Log($"[Projectile] OnCollisionEnter2D with {collision.gameObject.name}, tag={collision.gameObject.tag}");
            
            // Check if the collider or its parent has the target tag
            bool isValidTarget = false;
            if (string.IsNullOrEmpty(targetTag))
            {
                isValidTarget = true; // No tag filter, accept all
            }
            else if (collision.gameObject.CompareTag(targetTag))
            {
                isValidTarget = true; // Direct match
                Debug.Log($"[Projectile] ✓ Direct tag match: {targetTag}");
            }
            else
            {
                // Check parent hierarchy for tag
                Transform current = collision.transform;
                while (current != null)
                {
                    if (current.CompareTag(targetTag))
                    {
                        isValidTarget = true;
                        Debug.Log($"[Projectile] ✓ Parent tag match: {current.name} has tag {targetTag}");
                        break;
                    }
                    current = current.parent;
                }
            }
            
            if (!isValidTarget)
            {
                Debug.Log($"[Projectile] ✗ Tag mismatch: expected '{targetTag}', got '{collision.gameObject.tag}' (ignoring)");
                return;
            }

            // Deal damage and explode
            Debug.Log($"[Projectile] ✓ Valid target hit! Dealing damage and triggering explosion");
            DealDamage(collision.gameObject);
            TriggerExplosion();
        }

        /// <summary>
        /// Deal damage to target
        /// </summary>
        private void DealDamage(GameObject target)
        {
            // Try to deal damage to HealthBar
            var health = target.GetComponent<HealthBar>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"Projectile dealt {damage} damage to {target.name} via HealthBar");
            }

            // Try to deal damage to PlayerMovement2D
            var playerMovement = target.GetComponent<PlayerMovement2D>();
            if (playerMovement != null)
            {
                playerMovement.TakeDamage(damage);
                Debug.Log($"Projectile dealt {damage} damage to {target.name} via PlayerMovement2D");
            }

            // Try to deal damage to InnerCharacterController
            var characterController = target.GetComponent<InnerCharacterController>();
            if (characterController != null)
            {
                characterController.TakeDamage(damage);
                Debug.Log($"Projectile dealt {damage} damage to {target.name} via InnerCharacterController");
            }
        }

        /// <summary>
        /// Play explosion sound based on projectile type
        /// </summary>
        private void PlayExplosionSound()
        {
            switch (projectileType)
            {
                case ProjectileType.Attack:
                    AudioManager.Instance?.PlayExploreAttackSound();
                    break;
                case ProjectileType.Skill1:
                    AudioManager.Instance?.PlayExploreSkill1Sound();
                    break;
                case ProjectileType.Skill2:
                    AudioManager.Instance?.PlayExploreSkill2Sound();
                    break;
                case ProjectileType.Skill3:
                    AudioManager.Instance?.PlayExploreSkill3Sound();
                    break;
            }
        }

        /// <summary>
        /// Trigger explosion animation and destroy
        /// </summary>
        private void TriggerExplosion()
        {
            if (hasExploded) return;
            hasExploded = true;

            Debug.Log($"[Projectile] TriggerExplosion called on {gameObject.name}");
            Debug.Log($"[Projectile] playExplosionAnimation={playExplosionAnimation}, animator={(animator != null ? "EXISTS" : "NULL")}");

            // Stop movement
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false; // Disable physics
                Debug.Log($"[Projectile] Stopped rigidbody movement");
            }

            // Disable collider to prevent multiple hits
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
                Debug.Log($"[Projectile] Disabled collider");
            }

            // Play explosion sound based on projectile type
            PlayExplosionSound();

            // Play explosion animation
            if (playExplosionAnimation && animator != null)
            {
                Debug.Log($"[Projectile] Animator controller: {animator.runtimeAnimatorController?.name}");
                Debug.Log($"[Projectile] Animator parameters count: {animator.parameters.Length}");
                
                // List all parameters
                foreach (var param in animator.parameters)
                {
                    Debug.Log($"[Projectile] - Parameter: {param.name} ({param.type})");
                }
                
                // Try trigger parameter "explore" (for fireball)
                if (HasParameter(animator, "explore"))
                {
                    animator.SetTrigger("explore");
                    Debug.Log($"[Projectile] ✓ Triggered 'explore' animation!");
                    
                    // Check state after a frame
                    StartCoroutine(CheckAnimationState());
                }
                // Try trigger parameter "explode" (alternative)
                else if (HasParameter(animator, "explode"))
                {
                    animator.SetTrigger("explode");
                    Debug.Log($"[Projectile] ✓ Triggered 'explode' animation!");
                }
                else
                {
                    Debug.LogWarning($"[Projectile] ✗ No 'explore' or 'explode' parameter found!");
                }
                
                // Wait for animation to finish before destroying
                if (destroyOnHit)
                {
                    StartCoroutine(DestroyAfterAnimation());
                }
            }
            else
            {
                Debug.LogWarning($"[Projectile] Skipping animation - playExplosionAnimation={playExplosionAnimation}, animator={animator}");
                
                // No animation, destroy immediately
                if (destroyOnHit)
                {
                    Destroy(gameObject);
                }
            }
        }
        
        private IEnumerator CheckAnimationState()
        {
            yield return null; // Wait one frame
            
            if (animator != null)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"[Projectile] Current animation state hash: {stateInfo.fullPathHash}");
                Debug.Log($"[Projectile] Current animation normalized time: {stateInfo.normalizedTime}");
            }
        }

        /// <summary>
        /// Wait for explosion animation to finish, then destroy
        /// </summary>
        private IEnumerator DestroyAfterAnimation()
        {
            yield return new WaitForSeconds(explosionAnimationDuration);
            Destroy(gameObject);
        }

        /// <summary>
        /// Check if animator has a parameter
        /// </summary>
        private bool HasParameter(Animator anim, string paramName)
        {
            if (anim == null) return false;
            
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == paramName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
