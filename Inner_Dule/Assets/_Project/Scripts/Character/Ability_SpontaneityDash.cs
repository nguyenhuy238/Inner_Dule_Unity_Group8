using UnityEngine;

namespace InnerDuel.Characters
{
    public class Ability_SpontaneityDash : BaseCharacterAbility
    {
        private bool hasDashedThroughOpponent = false;

        public override void OnDash()
        {
            hasDashedThroughOpponent = false;
        }

        public override void OnUpdate()
        {
            // Spontaneity: Dash Damage Logic - Gây sát thương khi lướt qua đối thủ
            if (characterData.type == CharacterType.Spontaneity && controller.IsDashing && !hasDashedThroughOpponent)
            {
                // Tăng tầm quét khi lướt một chút để dễ trúng hơn
                Collider2D[] hitOpponents = Physics2D.OverlapCircleAll(transform.position, 1.2f, controller.opponentLayer);
                foreach (Collider2D opponent in hitOpponents)
                {
                    var opponentController = opponent.GetComponent<InnerCharacterController>();
                    if (opponentController != null)
                    {
                        // Ngẫu hứng gây sát thương lướt bằng 60% sát thương cơ bản
                        float dashDamage = characterData.attackDamage * 0.6f;
                        opponentController.TakeDamage(dashDamage);
                        hasDashedThroughOpponent = true;
                        
                        // Hiệu ứng đặc biệt khi lướt trúng
                        if (InnerDuel.Effects.ParticleEffectsManager.Instance != null)
                        {
                            InnerDuel.Effects.ParticleEffectsManager.Instance.PlayEffect("Dash", opponent.transform.position, characterData.effectColor);
                            InnerDuel.Effects.ParticleEffectsManager.Instance.PlayHitEffect(characterData.type, opponent.transform.position);
                        }
                    }
                }
            }
        }
    }
}
