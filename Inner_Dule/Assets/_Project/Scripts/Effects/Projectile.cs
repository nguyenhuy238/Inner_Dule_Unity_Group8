using UnityEngine;
using InnerDuel.Characters;

namespace InnerDuel.Effects
{
    /// <summary>
    /// Simple projectile script for fireballs and other projectiles
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [Tooltip("Damage dealt to target")]
        public int damage = 10;
        
        [Tooltip("Lifetime before auto-destroy (seconds)")]
        public float lifetime = 5f;
        
        [Tooltip("Destroy on any collision?")]
        public bool destroyOnHit = true;
        
        [Tooltip("Tag of targets that can be damaged (leave empty to damage anything)")]
        public string targetTag = "";

        private void Start()
        {
            // Auto-destroy after lifetime
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore if tag filter is set and doesn't match
            if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
            {
                return;
            }

            // Damage the controller only
            var characterController = other.GetComponent<InnerCharacterController>();
            if (characterController != null)
            {
                characterController.TakeDamage(damage);
            }

            // Destroy projectile on hit
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Ignore if tag filter is set and doesn't match
            if (!string.IsNullOrEmpty(targetTag) && !collision.gameObject.CompareTag(targetTag))
            {
                return;
            }

            // Try to deal damage to InnerCharacterController
            var characterController = collision.gameObject.GetComponent<InnerCharacterController>();
            if (characterController != null)
            {
                characterController.TakeDamage(damage);
            }

            // Destroy projectile on hit
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}
