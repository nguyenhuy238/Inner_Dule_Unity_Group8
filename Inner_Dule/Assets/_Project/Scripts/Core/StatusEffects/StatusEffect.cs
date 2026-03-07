using UnityEngine;

namespace InnerDuel.Core.StatusEffects
{
    /// <summary>
    /// Base class cho tất cả hiệu ứng trạng thái (Buff/Debuff).
    /// Team có thể tạo các file mới kế thừa từ class này.
    /// </summary>
    public abstract class StatusEffect
    {
        public string Name { get; protected set; }
        public float Duration { get; protected set; }
        public float TimeRemaining { get; protected set; }
        public bool IsFinished => TimeRemaining <= 0;

        public StatusEffect(string name, float duration)
        {
            Name = name;
            Duration = duration;
            TimeRemaining = duration;
        }

        public virtual void OnApply(Characters.InnerCharacterController target) 
        {
            Debug.Log($"[StatusEffect] Applied {Name} to {target.gameObject.name}");
        }

        public virtual void OnUpdate(Characters.InnerCharacterController target, float deltaTime)
        {
            TimeRemaining -= deltaTime;
        }

        public virtual void OnRemove(Characters.InnerCharacterController target)
        {
            Debug.Log($"[StatusEffect] Removed {Name} from {target.gameObject.name}");
        }
    }

    /// <summary>
    /// Ví dụ: Hiệu ứng làm chậm.
    /// </summary>
    public class SlowEffect : StatusEffect
    {
        private float slowAmount; // 0.5f nghĩa là giảm 50% tốc độ
        private float originalSpeed;

        public SlowEffect(float duration, float slowAmount) : base("Slow", duration)
        {
            this.slowAmount = slowAmount;
        }

        public override void OnApply(Characters.InnerCharacterController target)
        {
            base.OnApply(target);
            originalSpeed = target.characterData.moveSpeed;
            target.characterData.moveSpeed *= (1f - slowAmount);
        }

        public override void OnRemove(Characters.InnerCharacterController target)
        {
            target.characterData.moveSpeed = originalSpeed;
            base.OnRemove(target);
        }
    }

    /// <summary>
    /// Ví dụ: Hiệu ứng choáng.
    /// </summary>
    public class StunEffect : StatusEffect
    {
        public StunEffect(float duration) : base("Stun", duration) { }

        public override void OnApply(Characters.InnerCharacterController target)
        {
            base.OnApply(target);
            // Team can implement 'canInput' flag in InnerCharacterController
            // For now we just stop the velocity
            Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
        }
    }
}
