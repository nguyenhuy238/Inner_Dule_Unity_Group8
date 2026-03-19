using UnityEngine;

namespace InnerDuel.Characters
{
    public class Ability_Counter : BaseCharacterAbility
    {
        [Header("Counter Settings")]
        public float parryWindow = 0.2f;
        
        public override void OnTakeDamage(float damage)
        {
            if (controller == null || characterData == null) return;
            
            if (!characterData.canCounterAttack) return;
            
            if (controller.IsBlocking)
            {
                // Check for Perfect Block (Parry)
                if (Time.time - controller.LastBlockStartTime <= parryWindow)
                {
                    Debug.Log("PERFECT BLOCK! Counter Triggered!");
                    controller.TriggerCounterAttack();
                    // Heal back the damage? Or prevent it?
                    // Since OnTakeDamage is called before health reduction in the controller (in my previous thought),
                    // wait, let's check controller code again.
                    // Controller: foreach ability OnTakeDamage... then currentHealth -= damage.
                    // We can't cancel damage easily without changing return type.
                    // But we can heal it back or grant invincibility.
                    
                    // Hack: Add health back immediately to negate damage, or set invincibility.
                }
            }
        }
    }
}
