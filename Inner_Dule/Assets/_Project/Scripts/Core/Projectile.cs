using UnityEngine;

namespace InnerDuel.Core
{
    /// <summary>
    /// Script dùng chung cho tất cả các loại đạn (Projectiles).
    /// Team có thể tùy chỉnh tốc độ, sát thương và hiệu ứng khi va chạm.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Stats")]
        public float speed = 10f;
        public float damage = 10f;
        public float lifetime = 3f;
        public LayerMask targetLayer;
        
        [Header("Effects")]
        public Color projectileColor = Color.white;
        public GameObject hitEffectPrefab;
        
        private Vector2 direction;
        private int shooterID;
        private bool isInitialized = false;

        public void Initialize(Vector2 dir, int playerID, float dmg, LayerMask target)
        {
            direction = dir.normalized;
            shooterID = playerID;
            damage = dmg;
            targetLayer = target;
            isInitialized = true;
            
            // Tự động xoay theo hướng bay
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (!isInitialized) return;
            
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Kiểm tra xem có trúng mục tiêu không
            if (((1 << collision.gameObject.layer) & targetLayer) != 0)
            {
                var target = collision.GetComponent<Characters.InnerCharacterController>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                    
                    // Trigger hiệu ứng trúng đòn
                    if (InnerDuel.Effects.ParticleEffectsManager.Instance != null)
                    {
                        InnerDuel.Effects.ParticleEffectsManager.Instance.PlayEffect("Hit", transform.position, projectileColor);
                    }
                }
                
                OnHit();
            }
            // Nếu trúng tường hoặc chướng ngại vật
            else if (collision.CompareTag("Obstacle"))
            {
                OnHit();
            }
        }

        private void OnHit()
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
    }
}
