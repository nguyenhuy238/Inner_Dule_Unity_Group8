using UnityEngine;
using UnityEngine.UI;

namespace InnerDuel.Characters
{
    public class HealthBar : MonoBehaviour
    {
        [Header("UI References")]
        public Slider healthSlider;
        public Slider delayedHealthSlider;
        
        [Header("Settings")]
        public float delayTime = 1f;
        public float fillSpeed = 5f;
        
        private float targetHealth;
        private float currentDelayedHealth;
        
        private void Start()
        {
            if (healthSlider != null)
            {
                healthSlider.value = 1f;
            }
            
            if (delayedHealthSlider != null)
            {
                delayedHealthSlider.value = 1f;
                currentDelayedHealth = 1f;
            }
        }
        
        private void Update()
        {
            // Update delayed health bar for smooth effect
            if (delayedHealthSlider != null && currentDelayedHealth != targetHealth)
            {
                currentDelayedHealth = Mathf.MoveTowards(
                    currentDelayedHealth, 
                    targetHealth, 
                    fillSpeed * Time.deltaTime
                );
                delayedHealthSlider.value = currentDelayedHealth;
            }
        }
        
        public void SetMaxHealth(float maxHealth)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = maxHealth;
            }
            
            if (delayedHealthSlider != null)
            {
                delayedHealthSlider.maxValue = maxHealth;
                delayedHealthSlider.value = maxHealth;
                currentDelayedHealth = maxHealth;
            }
            
            targetHealth = maxHealth;
        }
        
        public void SetHealth(float currentHealth)
        {
            if (healthSlider != null)
            {
                healthSlider.value = currentHealth;
            }
            
            targetHealth = currentHealth;
            
            // Start delayed health reduction
            if (delayedHealthSlider != null)
            {
                Invoke(nameof(StartDelayedHealthReduction), delayTime);
            }
        }
        
        private void StartDelayedHealthReduction()
        {
            // The actual reduction happens in Update for smooth effect
        }
        
        public void ResetHealthBar()
        {
            SetMaxHealth(100f);
        }
        
        /// <summary>
        /// Take damage and update health bar
        /// </summary>
        public void TakeDamage(int damage)
        {
            float currentHealth = healthSlider != null ? healthSlider.value : 0f;
            float newHealth = Mathf.Max(0f, currentHealth - damage);
            SetHealth(newHealth);
        }
    }
}
