using UnityEngine;
using InnerDuel.Core.StatusEffects;

namespace InnerDuel.Characters
{
    public class Ability_DisciplineParry : BaseCharacterAbility
    {
        [Header("Parry Settings")]
        [SerializeField] private float parryWindow = 0.25f; // Thời gian để coi là Perfect Parry
        [SerializeField] private float stunDuration = 1.5f;
        [SerializeField] private float parryKnockbackForce = 5f;

        public override void OnTakeDamage(float damage)
        {
            // Kiểm tra xem có đang Block và trong cửa sổ Perfect Parry không
            if (controller.IsBlocking && characterData.canCounterAttack && 
                (Time.time - controller.LastBlockStartTime) <= parryWindow)
            {
                TriggerPerfectParry();
            }
        }

        private void TriggerPerfectParry()
        {
            Debug.Log($"[InnerDuel] {characterData.characterName} triggered PERFECT PARRY!");

            // 1. Gây Stun cho đối thủ trong tầm đánh
            Collider2D[] attackers = Physics2D.OverlapCircleAll(transform.position, characterData.attackRange + 1.5f, controller.opponentLayer);
            foreach (var attacker in attackers)
            {
                // Áp dụng Stun
                var stunManager = attacker.GetComponent<StatusEffectManager>();
                if (stunManager != null)
                {
                    stunManager.ApplyEffect(new StunEffect(stunDuration));
                }

                // Đẩy lùi đối thủ nhẹ (Knockback)
                Rigidbody2D attackerRb = attacker.GetComponent<Rigidbody2D>();
                if (attackerRb != null)
                {
                    Vector2 knockbackDir = (attacker.transform.position - transform.position).normalized;
                    attackerRb.AddForce(knockbackDir * parryKnockbackForce, ForceMode2D.Impulse);
                }
            }

            // 2. Thực hiện phản đòn tự động qua Controller
            controller.TriggerCounterAttack();

            // 3. Hiệu ứng hình ảnh & âm thanh
            if (InnerDuel.Effects.ParticleEffectsManager.Instance != null)
            {
                // Hiệu ứng Parry mặc định của hệ thống
                InnerDuel.Effects.ParticleEffectsManager.Instance.PlayEffect("Parry", transform.position, Color.yellow);
                
                // Hiệu ứng "Hit" tại vị trí đối thủ để nhấn mạnh va chạm
                foreach (var attacker in attackers)
                {
                    InnerDuel.Effects.ParticleEffectsManager.Instance.PlayHitEffect(CharacterType.Discipline, attacker.transform.position);
                }
            }
            
            // Có thể thêm rung màn hình (Screen Shake) ở đây nếu muốn
        }
    }
}
