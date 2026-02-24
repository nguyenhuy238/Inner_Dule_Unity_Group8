using UnityEngine;
using InnerDuel.Characters;

namespace InnerDuel.Effects
{
    public class ParticleEffectsManager : MonoBehaviour
    {
        public static ParticleEffectsManager Instance { get; private set; }
        
        [Header("Hit Effects")]
        public ParticleSystem disciplineHitEffect;
        public ParticleSystem spontaneityHitEffect;
        public ParticleSystem logicHitEffect;
        public ParticleSystem creativityHitEffect;
        public ParticleSystem persistenceHitEffect;
        public ParticleSystem surrenderHitEffect;
        public ParticleSystem stillnessHitEffect;
        public ParticleSystem rageHitEffect;
        
        [Header("Block Effects")]
        public ParticleSystem blockEffect;
        public ParticleSystem parryEffect;
        
        [Header("Special Effects")]
        public ParticleSystem dashEffect;
        public ParticleSystem berserkEffect;
        public ParticleSystem harmonyEffect;
        public ParticleSystem lifeStealEffect;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void PlayHitEffect(CharacterType type, Vector3 position)
        {
            ParticleSystem effect = GetHitEffect(type);
            if (effect != null)
            {
                ParticleSystem instance = Instantiate(effect, position, Quaternion.identity);
                instance.Play();
                Destroy(instance.gameObject, instance.main.duration);
            }
        }
        
        public void PlayBlockEffect(Vector3 position)
        {
            if (blockEffect != null)
            {
                ParticleSystem instance = Instantiate(blockEffect, position, Quaternion.identity);
                instance.Play();
                Destroy(instance.gameObject, instance.main.duration);
            }
        }
        
        public void PlayParryEffect(Vector3 position)
        {
            if (parryEffect != null)
            {
                ParticleSystem instance = Instantiate(parryEffect, position, Quaternion.identity);
                instance.Play();
                Destroy(instance.gameObject, instance.main.duration);
            }
        }
        
        public void PlayDashEffect(InnerCharacterController character)
        {
            if (dashEffect != null)
            {
                ParticleSystem instance = Instantiate(dashEffect, character.transform.position, Quaternion.identity);
                instance.transform.SetParent(character.transform);
                instance.Play();
                Destroy(instance.gameObject, 1f);
            }
        }
        
        public void PlayBerserkEffect(InnerCharacterController character)
        {
            if (berserkEffect != null)
            {
                ParticleSystem instance = Instantiate(berserkEffect, character.transform.position, Quaternion.identity);
                instance.transform.SetParent(character.transform);
                instance.Play();
                Destroy(instance.gameObject, 5f);
            }
        }
        
        public void PlayHarmonyEffect(Vector3 position)
        {
            if (harmonyEffect != null)
            {
                ParticleSystem instance = Instantiate(harmonyEffect, position, Quaternion.identity);
                instance.Play();
                Destroy(instance.gameObject, instance.main.duration);
            }
        }
        
        public void PlayLifeStealEffect(Vector3 fromPosition, Vector3 toPosition)
        {
            if (lifeStealEffect != null)
            {
                ParticleSystem instance = Instantiate(lifeStealEffect, fromPosition, Quaternion.identity);
                
                // Create particle movement towards target
                var main = instance.main;
                main.startLifetime = 1f;
                
                Destroy(instance.gameObject, 2f);
            }
        }
        
        private ParticleSystem GetHitEffect(CharacterType type)
        {
            switch (type)
            {
                case CharacterType.Discipline: return disciplineHitEffect;
                case CharacterType.Spontaneity: return spontaneityHitEffect;
                case CharacterType.Logic: return logicHitEffect;
                case CharacterType.Creativity: return creativityHitEffect;
                case CharacterType.Persistence: return persistenceHitEffect;
                case CharacterType.Surrender: return surrenderHitEffect;
                case CharacterType.Stillness: return stillnessHitEffect;
                case CharacterType.Rage: return rageHitEffect;
                default: return null;
            }
        }
        
        public void CreateDefaultEffects()
        {
            // Create default particle effects if none are assigned
            if (disciplineHitEffect == null)
            {
                disciplineHitEffect = CreateDefaultHitEffect(Color.yellow, "DisciplineHit");
            }
            
            if (spontaneityHitEffect == null)
            {
                spontaneityHitEffect = CreateDefaultHitEffect(Color.cyan, "SpontaneityHit");
            }
            
            if (logicHitEffect == null)
            {
                logicHitEffect = CreateDefaultHitEffect(Color.blue, "LogicHit");
            }
            
            if (creativityHitEffect == null)
            {
                creativityHitEffect = CreateDefaultHitEffect(Color.magenta, "CreativityHit");
            }
            
            if (persistenceHitEffect == null)
            {
                persistenceHitEffect = CreateDefaultHitEffect(Color.white, "PersistenceHit");
            }
            
            if (surrenderHitEffect == null)
            {
                surrenderHitEffect = CreateDefaultHitEffect(Color.grey, "SurrenderHit");
            }
            
            if (stillnessHitEffect == null)
            {
                stillnessHitEffect = CreateDefaultHitEffect(Color.gray, "StillnessHit");
            }
            
            if (rageHitEffect == null)
            {
                rageHitEffect = CreateDefaultHitEffect(Color.red, "RageHit");
            }
        }
        
        private ParticleSystem CreateDefaultHitEffect(Color color, string name)
        {
            GameObject effectObj = new GameObject(name);
            effectObj.transform.SetParent(transform);
            
            ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
            
            var main = ps.main;
            main.startColor = color;
            main.startSize = 0.2f;
            main.startSpeed = 3f;
            main.startLifetime = 0.5f;
            main.maxParticles = 20;
            main.duration = 0.5f;
            
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 10)
            });
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;
            
            return ps;
        }
    }
}
