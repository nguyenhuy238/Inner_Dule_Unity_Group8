using UnityEngine;
using System.Collections;

namespace InnerDuel.Characters
{
    /// <summary>
    /// Ability đặc biệt cho nhân vật Emotion (Axe).
    /// Lưu ý: Logic Skill 3 hiện đã được tích hợp trực tiếp vào InnerCharacterController 
    /// để đảm bảo độ tin cậy và không phụ thuộc vào việc gắn script này hay không.
    /// </summary>
    public class Ability_EmotionAxe : BaseCharacterAbility
    {
        // Skill 1: Chém ra lửa (Có thể thêm hiệu ứng tại đây)
        public override void OnSkill1()
        {
            if (InnerDuel.Effects.ParticleEffectsManager.Instance != null)
            {
                InnerDuel.Effects.ParticleEffectsManager.Instance.PlayEffect("Hit", controller.attack1Point.position, Color.red);
            }
        }

        // Skill 2: Xoay rìu (Whirlwind)
        public override void OnSkill2()
        {
            controller.StartCoroutine(WhirlwindRoutine());
        }

        public override void OnSkill3()
        {
            // Logic đã có ở InnerCharacterController.AxeDashStabRoutine
        }

        private IEnumerator WhirlwindRoutine()
        {
            float duration = 1.0f;
            float interval = 0.2f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, characterData.attack2Range, controller.opponentLayer);
                foreach (var hit in hits)
                {
                    var target = hit.GetComponent<InnerCharacterController>();
                    if (target != null && !target.IsDead())
                    {
                        target.TakeDamage(characterData.attack2Damage * 0.3f); 
                    }
                }

                if (InnerDuel.Effects.ParticleEffectsManager.Instance != null)
                {
                    InnerDuel.Effects.ParticleEffectsManager.Instance.PlayEffect("Dash", transform.position, Color.red);
                }

                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }
        }
    }
}
