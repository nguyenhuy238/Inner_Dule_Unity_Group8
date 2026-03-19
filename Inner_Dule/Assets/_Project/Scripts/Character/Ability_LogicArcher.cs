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
            // Truyền tham số 3 để bắn 3 mũi tên, dùng prefab mặc định
            controller.StartCoroutine(controller.SpawnProjectileRoutine(0.2f, characterData.attack2Damage, 3, characterData.projectilePrefab));
        }

        // Skill 3: Bắn mũi tên đặc biệt
        public override void OnSkill3()
        {
            controller.StartCoroutine(controller.SpawnProjectileRoutine(0.3f, characterData.attack3Damage, 1, characterData.arrowSkill3Prefab));
        }
    }
}