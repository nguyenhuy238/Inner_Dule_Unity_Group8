using System.Collections.Generic;
using UnityEngine;
using InnerDuel.Characters;

namespace InnerDuel.Core.StatusEffects
{
    [RequireComponent(typeof(InnerCharacterController))]
    public class StatusEffectManager : MonoBehaviour
    {
        private List<StatusEffect> activeEffects = new List<StatusEffect>();
        private InnerCharacterController controller;

        private void Awake()
        {
            controller = GetComponent<InnerCharacterController>();
        }

        public void ApplyEffect(StatusEffect effect)
        {
            activeEffects.Add(effect);
            effect.OnApply(controller);
        }

        private void Update()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                activeEffects[i].OnUpdate(controller, Time.deltaTime);

                if (activeEffects[i].IsFinished)
                {
                    activeEffects[i].OnRemove(controller);
                    activeEffects.RemoveAt(i);
                }
            }
        }

        public bool HasEffect(string effectName)
        {
            return activeEffects.Exists(e => e.Name == effectName);
        }
    }
}
