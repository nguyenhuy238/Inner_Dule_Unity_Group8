using UnityEngine;
using System.Collections;

namespace InnerDuel.Characters
{
    public class Ability_SpontaneityDash : BaseCharacterAbility
    {
        [Header("Maverick Dash Settings")]
        [SerializeField] private float dashDamageMultiplier = 0.7f;
        [SerializeField] private float dashHitRadius = 1.2f;

        [Header("Energy Orb Shot Settings")]
        [SerializeField] private float normalShotDelay = 0.12f;
        [SerializeField] private float skill1ShotDelay = 0.16f;
        [SerializeField] private float skill2ShotDelay = 0.2f;
        [SerializeField] private int skill2OrbCount = 3;

        [Header("Skill 3 (Melee Explosion)")]
        [SerializeField] private float skill3ExplosionDelay = 0.22f;
        [SerializeField] private float skill3ExplosionRadiusMultiplier = 0.75f;
        [SerializeField] private float skill3MinExplosionRadius = 1.0f;
        
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

        public override void OnNormalAttack()
        {
            SpawnOrb(normalShotDelay, characterData.normalAttackDamage, 1, characterData.projectilePrefab, null);
        }

        public override void OnSkill1()
        {
            SpawnOrb(skill1ShotDelay, characterData.attack1Damage, 1, characterData.arrowSkill1Prefab, characterData.projectilePrefab);
        }

        public override void OnSkill2()
        {
            SpawnOrb(skill2ShotDelay, characterData.attack2Damage, Mathf.Max(1, skill2OrbCount), characterData.arrowSkill2Prefab, characterData.projectilePrefab);
        }

        public override void OnSkill3()
        {
            // Skill 3 là nổ cận chiến: KHÔNG bắn orb
            controller.StartCoroutine(Skill3ExplosionRoutine());
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

        private void SpawnOrb(float delay, float damage, int orbCount, GameObject preferredPrefab, GameObject fallbackPrefab)
        {
            GameObject prefabToUse = preferredPrefab != null ? preferredPrefab : fallbackPrefab;
            if (prefabToUse == null)
            {
                Debug.LogWarning($"[Spontaneity] Missing projectile prefab on {characterData.characterName}.");
                return;
            }

            controller.StartCoroutine(controller.SpawnProjectileRoutine(delay, damage, orbCount, prefabToUse));
        }

        private IEnumerator Skill3ExplosionRoutine()
        {
            yield return new WaitForSeconds(skill3ExplosionDelay);

            if (controller == null || characterData == null || controller.IsDead()) yield break;

            Vector3 center = controller.attack3Point != null ? controller.attack3Point.position : transform.position;
            float radius = Mathf.Max(skill3MinExplosionRadius, characterData.attack3Range * skill3ExplosionRadiusMultiplier);
            float damage = characterData.attack3Damage;

            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, controller.opponentLayer);
            foreach (var hit in hits)
            {
                var target = hit.GetComponent<InnerCharacterController>();
                if (target != null && !target.IsDead())
                {
                    target.TakeDamage(damage);

                    if (InnerDuel.Effects.ParticleEffectsManager.Instance != null)
                    {
                        InnerDuel.Effects.ParticleEffectsManager.Instance.PlayHitEffect(characterData.type, target.transform.position);
                    }
                }
            }

            if (InnerDuel.Effects.ParticleEffectsManager.Instance != null)
            {
                InnerDuel.Effects.ParticleEffectsManager.Instance.PlayEffect("Parry", center, characterData.effectColor);
            }
        }
    }
}
