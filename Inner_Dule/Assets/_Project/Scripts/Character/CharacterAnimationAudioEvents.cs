using UnityEngine;
using InnerDuel.Audio;

namespace InnerDuel.Characters
{
    [DisallowMultipleComponent]
    public class CharacterAnimationAudioEvents : MonoBehaviour
    {
        [SerializeField] private InnerCharacterController controller;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<InnerCharacterController>();
            }
        }

        // Animation Event entry points
        public void AE_PlayFootstepSfx() => Play(CharacterAudioAction.Footstep, 0.75f);
        public void AE_PlayJumpSfx() => Play(CharacterAudioAction.Jump, 0.9f);
        public void AE_PlayLandSfx() => Play(CharacterAudioAction.Land, 0.9f);
        public void AE_PlayNormalAttackSfx() => Play(CharacterAudioAction.NormalAttack);
        public void AE_PlaySkill1Sfx() => Play(CharacterAudioAction.Skill1);
        public void AE_PlaySkill2Sfx() => Play(CharacterAudioAction.Skill2);
        public void AE_PlaySkill3Sfx() => Play(CharacterAudioAction.Skill3);
        public void AE_PlayDashSfx() => Play(CharacterAudioAction.DashStart);
        public void AE_PlayBlockSfx() => Play(CharacterAudioAction.BlockImpact, 0.9f);
        public void AE_PlayHurtSfx() => Play(CharacterAudioAction.Hurt);
        public void AE_PlayDeathSfx() => Play(CharacterAudioAction.Death);

        private void Play(CharacterAudioAction action, float volumeMultiplier = 1f)
        {
            if (controller == null || controller.characterData == null) return;
            if (AudioManager.Instance == null) return;

            AudioManager.Instance.PlayCharacterActionSfx(controller.characterData.type, action, volumeMultiplier);
        }
    }
}
