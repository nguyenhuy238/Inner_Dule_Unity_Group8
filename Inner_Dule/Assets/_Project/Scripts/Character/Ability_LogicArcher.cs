using UnityEngine;

namespace InnerDuel.Characters
{
    public class Ability_LogicArcher : BaseCharacterAbility
    {
        // Attack thường: Bắn 1 mũi tên thường
        public override void OnNormalAttack()
        {
            controller.StartCoroutine(controller.SpawnProjectileRoutine(0.15f, characterData.normalAttackDamage, 1, characterData.projectilePrefab));
        }

        // Skill 1: Bắn 1 mũi tên năng lượng
        public override void OnSkill1()
        {
            controller.StartCoroutine(controller.SpawnProjectileRoutine(0.2f, characterData.attack1Damage, 1, characterData.arrowSkill1Prefab));
        }

        // Skill 2: Bắn 3 mũi tên
        public override void OnSkill2()
        {
            Debug.Log($"[Ability_LogicArcher] Skill 2 Activated on {gameObject.name}");
            // Truyền tham số 3 để bắn 3 mũi tên, dùng prefab mặc định
            controller.StartCoroutine(controller.SpawnProjectileRoutine(0.2f, characterData.attack2Damage, 3, characterData.projectilePrefab));
        }

        // Skill 3: Bắn mũi tên đặc biệt
        public override void OnSkill3()
        {
            Debug.Log($"[Ability_LogicArcher] Skill 3 Activated on {gameObject.name}");
            // Fallback: Nếu không có ArrowSkill3Prefab thì dùng mũi tên thường
            GameObject skill3Prefab = characterData.arrowSkill3Prefab != null ? characterData.arrowSkill3Prefab : characterData.projectilePrefab;
            
            if (skill3Prefab == null)
            {
                Debug.LogError($"[Ability_LogicArcher] CRITICAL: No prefabs (S3 or regular) found for {characterData.type}!");
            }

            controller.StartCoroutine(controller.SpawnProjectileRoutine(0.3f, characterData.attack3Damage, 1, skill3Prefab));
        }
    }
}