using UnityEngine;
using InnerDuel.Characters;

namespace InnerDuel.Core
{
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Stats")]
        public float speed = 10f;
        public float damage = 10f;
        public float lifetime = 3f;
        
        [Header("Effects")]
        public GameObject hitEffectPrefab;
        
        private Vector2 direction;
        private int shooterID;
        private LayerMask targetLayer;
        private bool isInitialized = false;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Vector2 dir, int playerID, float dmg, LayerMask target)
        {
            direction = dir.normalized;
            shooterID = playerID;
            damage = dmg;
            targetLayer = target;
            isInitialized = true;
            
            // Set velocity if RB exists, otherwise Translate in Update
            if (rb != null)
            {
                rb.velocity = direction * speed;
            }
            
            // Rotate to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (!isInitialized) return;
            
            if (rb == null)
            {
                transform.Translate(direction * speed * Time.deltaTime, Space.World);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isInitialized) return;
            
            // Check Layer Match
            if (((1 << collision.gameObject.layer) & targetLayer) != 0)
            {
                var target = collision.GetComponent<InnerCharacterController>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                }
                
                OnHit();
            }
            // Wall/Ground check
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) 
            {
                OnHit();
            }
        }

        private void OnHit()
        {
            
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                
                if (rb != null) rb.velocity = Vector2.zero;
                isInitialized = false;

               
                Collider2D collider = GetComponent<Collider2D>();
                if (collider != null) collider.enabled = false;

                
                animator.SetTrigger("Explore");

                
                Destroy(gameObject, 0.3f);
            }
            else
            {
               
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
            }
        }
    }
}
