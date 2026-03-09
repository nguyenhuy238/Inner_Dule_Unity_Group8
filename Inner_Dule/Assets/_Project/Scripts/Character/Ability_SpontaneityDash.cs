using UnityEngine;

namespace InnerDuel.Characters
{
    public class Ability_SpontaneityDash : BaseCharacterAbility
    {
        [Header("Maverick Dash Settings")]
        [SerializeField] private float dashDamageMultiplier = 0.7f;
        [SerializeField] private float dashHitRadius = 1.2f;
        
        private System.Collections.Generic.HashSet<int> hitOpponentsThisDash = new System.Collections.Generic.HashSet<int>();

        public override void OnDash()
        {
            // Reset danh sách đối thủ đã trúng trong lần lướt này
            hitOpponentsThisDash.Clear();
        }

        public override void OnUpdate()
        {
            // Ngẫu hứng (Spontaneity): Gây sát thương khi lướt qua đối thủ
            if (controller.IsDashing)
            {
                CheckDashCollision();
            }
        }

        private void CheckDashCollision()
        {
            // Quét các đối thủ trong tầm lướt
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, dashHitRadius, controller.opponentLayer);
            
            foreach (Collider2D col in colliders)
            {
                int id = col.gameObject.GetInstanceID();
                
                // Chỉ gây sát thương 1 lần cho mỗi đối thủ trong một lần lướt
                if (!hitOpponentsThisDash.Contains(id))
                {
                    var opponent = col.GetComponent<InnerCharacterController>();
                    if (opponent != null)
                    {
                        ApplyDashDamage(opponent);
                        hitOpponentsThisDash.Add(id);
                    }
                }
            }
        }

        private void ApplyDashDamage(InnerCharacterController opponent)
        {
            float damage = characterData.attackDamage * dashDamageMultiplier;
            opponent.TakeDamage(damage);
            
            Debug.Log($"[InnerDuel] {characterData.characterName} dashed through {opponent.gameObject.name} for {damage} damage!");

            // Hiệu ứng đặc biệt
            if (InnerDuel.Effects.ParticleEffectsManager.Instance != null)
            {
                // Hiệu ứng lướt (Dash) tại vị trí va chạm
                InnerDuel.Effects.ParticleEffectsManager.Instance.PlayEffect("Dash", opponent.transform.position, characterData.effectColor);
                
                // Hiệu ứng trúng đòn (Hit)
                InnerDuel.Effects.ParticleEffectsManager.Instance.PlayHitEffect(characterData.type, opponent.transform.position);
            }
        }
    }
}
