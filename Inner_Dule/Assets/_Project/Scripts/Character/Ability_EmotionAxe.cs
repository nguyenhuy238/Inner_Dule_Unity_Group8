using UnityEngine;
using System.Collections;
using InnerDuel.Core.StatusEffects;

namespace InnerDuel.Characters
{
    public class Ability_EmotionAxe : BaseCharacterAbility
    {
        [Header("Skill Settings")]
        [SerializeField] private float skill1Delay = 0.2f;
        [SerializeField] private float skill2Duration = 1.0f;
        [SerializeField] private float skill2DamageInterval = 0.2f;
        [SerializeField] private float skill3BuffDuration = 5.0f;
        [SerializeField] private float skill3DamageMultiplier = 2.0f;

        // Skill 1: Chém ra lửa (Fire Slash - Melee)
        public override void OnSkill1()
        {
            Debug.Log($"[Ability_EmotionAxe] Skill 1 (Fire Melee Slash) Activated on {gameObject.name}");
            
            // Trigger fire effects at attack point
            if (InnerDuel.Effects.ParticleEffectsManager.Instance != null)
            {
                // Play fire effect at attack1Point
                InnerDuel.Effects.ParticleEffectsManager.Instance.PlayEffect("Hit", controller.attack1Point.position, Color.red);
            }
        }

        // Skill 2: Xoay rìu (Whirlwind)
        public override void OnSkill2()
        {
            Debug.Log($"[Ability_EmotionAxe] Skill 2 (Whirlwind) Activated on {gameObject.name}");
            controller.StartCoroutine(WhirlwindRoutine());
        }

        // Skill 3: Bật nội tại tăng dame (Damage Buff + Freeze Animation)
        public override void OnSkill3()
        {
            Debug.Log($"[Ability_EmotionAxe] Skill 3 (Hold Buff) Activated on {gameObject.name}");
            controller.StartCoroutine(DamageBuffRoutine());
        }

        private IEnumerator WhirlwindRoutine()
        {
            float elapsed = 0f;
            while (elapsed < skill2Duration)
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

                yield return new WaitForSeconds(skill2DamageInterval);
                elapsed += skill2DamageInterval;
            }
        }

        private IEnumerator DamageBuffRoutine()
        {
            // Delay to reach the end of the animation (approx 0.4s)
            yield return new WaitForSeconds(0.4f);

            // Tăng damage multiplier
            float originalMultiplier = controller.damageMultiplier;
            controller.damageMultiplier *= skill3DamageMultiplier;
            
            // Pause animator for 5 seconds (holding the pose)
            controller.PauseAnimator(skill3BuffDuration);
            
            // Visually show buff effect
            if (InnerDuel.Effects.ParticleEffectsManager.Instance != null)
            {
                InnerDuel.Effects.ParticleEffectsManager.Instance.PlayEffect("Parry", transform.position, Color.red);
            }

            // Wait while holding the pose
            yield return new WaitForSeconds(skill3BuffDuration);

            // Restore
            controller.damageMultiplier = originalMultiplier;
        }
    }
}
