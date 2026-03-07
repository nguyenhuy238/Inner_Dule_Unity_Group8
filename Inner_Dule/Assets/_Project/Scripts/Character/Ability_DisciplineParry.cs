using UnityEngine;

namespace InnerDuel.Characters
{
    public class Ability_DisciplineParry : BaseCharacterAbility
    {
        private float counterAttackWindow = 0.25f;

        public override void OnTakeDamage(float damage)
        {
            if (controller.IsBlocking && characterData.canCounterAttack && 
                (Time.time - controller.LastBlockStartTime) <= counterAttackWindow)
            {
                Debug.Log($"[InnerDuel] {characterData.characterName} triggered PERFECT PARRY via Ability!");
                
                // Kỷ Luật: Làm choáng kẻ tấn công trong 1.5 giây
                Collider2D[] attackers = Physics2D.OverlapCircleAll(transform.position, characterData.attackRange + 2f, controller.opponentLayer);
                foreach (var attacker in attackers)
                {
                    var stunManager = attacker.GetComponent<InnerDuel.Core.StatusEffects.StatusEffectManager>();
                    if (stunManager != null)
                    {
                        stunManager.ApplyEffect(new InnerDuel.Core.StatusEffects.StunEffect(1.5f));
                    }
                }

                // Thực hiện đòn đánh tức thì
                controller.TriggerCounterAttack();
                
                // Hiệu ứng phản đòn
                if (InnerDuel.Effects.ParticleEffectsManager.Instance != null)
                    InnerDuel.Effects.ParticleEffectsManager.Instance.PlayEffect("Parry", transform.position, Color.yellow);
            }
        }
    }
}
