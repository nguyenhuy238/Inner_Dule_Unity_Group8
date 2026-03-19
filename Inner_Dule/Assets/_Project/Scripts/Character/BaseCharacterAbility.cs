using UnityEngine;

namespace InnerDuel.Characters
{
    public abstract class BaseCharacterAbility : MonoBehaviour
    {
        protected InnerCharacterController controller;
        protected CharacterData characterData;

        public virtual void Initialize(InnerCharacterController controller, CharacterData data)
        {
            this.controller = controller;
            this.characterData = data;
        }

        public virtual void OnUpdate() { }
        public virtual void OnAttack() { }
        public virtual void OnNormalAttack() { }
        public virtual void OnDash() { }
        public virtual void OnBlockStart() { }
        public virtual void OnBlockEnd() { }
        public virtual void OnSkill1() { }
        public virtual void OnSkill2() { }
        public virtual void OnSkill3() { }
        public virtual void OnTakeDamage(float damage) { }
        public virtual void OnDie() { }
    }
}
