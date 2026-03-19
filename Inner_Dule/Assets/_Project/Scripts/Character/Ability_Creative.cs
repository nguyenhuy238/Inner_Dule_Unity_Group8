using UnityEngine;

namespace InnerDuel.Characters  // ← THÊM NAMESPACE
{
    public class Ability_Creative : BaseCharacterAbility
    {
        // Attack thường: Bắn projectilePrefab
        public override void OnNormalAttack()
        {
            Debug.Log("[Creative] OnNormalAttack called!"); // ← Debug log

            if (characterData.projectilePrefab != null)
            {
                Debug.Log("[Creative] Spawning projectile!");
                controller.StartCoroutine(controller.SpawnProjectileRoutine(
                    0.15f,
                    characterData.normalAttackDamage,
                    1,
                    characterData.projectilePrefab
                ));
            }
            else
            {
                Debug.LogWarning("[Creative] projectilePrefab is NULL!");
            }
        }

        // Skill 1 (Attack1): Bắn arrowSkill1Prefab
        public override void OnSkill1()
        {
            Debug.Log("[Creative] OnSkill1 called!");

            if (characterData.arrowSkill1Prefab != null)
            {
                Debug.Log("[Creative] Spawning Skill1 arrow!");
                controller.StartCoroutine(controller.SpawnProjectileRoutine(
                    0.2f,
                    characterData.attack1Damage,
                    1,
                    characterData.arrowSkill1Prefab
                ));
            }
            else
            {
                Debug.LogWarning("[Creative] arrowSkill1Prefab is NULL!");
            }
        }

        // Skill 2 (Attack2): Bắn arrowSkill2Prefab
        public override void OnSkill2()
        {
            Debug.Log("[Creative] OnSkill2 called!");

            if (characterData.arrowSkill2Prefab != null)
            {
                Debug.Log("[Creative] Spawning Skill2 arrow!");
                controller.StartCoroutine(controller.SpawnProjectileRoutine(
                    0.2f,
                    characterData.attack2Damage,
                    1,
                    characterData.arrowSkill2Prefab
                ));
            }
            else
            {
                Debug.LogWarning("[Creative] arrowSkill2Prefab is NULL!");
            }
        }

        // Skill 3 (Attack3): Bắn arrowSkill3Prefab
        public override void OnSkill3()
        {
            Debug.Log("[Creative] OnSkill3 called!");

            if (characterData.arrowSkill3Prefab != null)
            {
                Debug.Log("[Creative] Spawning Skill3 arrow!");
                controller.StartCoroutine(controller.SpawnProjectileRoutine(
                    0.3f,
                    characterData.attack3Damage,
                    1,
                    characterData.arrowSkill3Prefab
                ));
            }
            else
            {
                Debug.LogWarning("[Creative] arrowSkill3Prefab is NULL!");
            }
        }
    }
}