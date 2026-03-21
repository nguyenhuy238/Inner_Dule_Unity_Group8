using UnityEngine;

namespace InnerDuel.Characters
{
    /// <summary>
    /// Các cặp nhân vật đại diện cho các trạng thái đối lập trong tâm trí.
    /// </summary>
    public enum CharacterType
    {
        // Cặp 1: Kỷ Luật vs Ngẫu Hứng
        Discipline,    // Kỷ Luật - The Warden
        Spontaneity,   // Ngẫu Hứng - The Maverick
        
        // Cặp 2: Lý Trí vs Cảm Xúc
        Logic,         // Lý Trí - The Architect
        Creativity,    // Cảm Xúc - The Muse
        
        // Cặp 4: Tĩnh Lặng vs Thịnh Nộ
        Reason,        // Tĩnh Lặng - The Zen -> Reason (Cung thủ)
        Emotion        // Thịnh Nộ - The Berserker -> Emotion (Kỵ sĩ cầm rìu)
    }
    
    /// <summary>
    /// Chứa tất cả thông số cấu hình của một nhân vật.
    /// Team nên thay đổi ở đây để tạo sự khác biệt giữa các nhân vật.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterData", menuName = "InnerDuel/Character/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [Header("Visual Assets")]
        public RuntimeAnimatorController animatorController;
        public GameObject characterPrefab; // If null, uses default scene object
        public Sprite defaultSprite;
        
        [Header("Identity")]
        public CharacterType type;
        public string characterName;
        public Sprite portrait;
        [TextArea(3, 10)]
        public string description;
        
        [Header("Base Stats")]
        public float maxHealth = 100f;
        public float moveSpeed = 5f;
        public float defense = 5f; // Giảm sát thương nhận vào
        
        [Header("Movement Physics")]
        public float jumpForce = 12f;
        public float airControlMultiplier = 0.8f;

        [Header("Combat Stats")]
        public float normalAttackDamage = 8f;
        public float normalAttackRange = 0.8f;
        public float normalAttackCooldown = 0.35f;
        
        public float attackDamage = 10f; // Base/Default (Legacy/Generic)
        public float attackRange = 1.2f; // Base/Default (Legacy/Generic)
        public float attackCooldown = 0.5f; // Base/Default (Legacy/Generic)
        public float dashSpeedMultiplier = 3f;
        public float dashDuration = 0.2f;

        [Header("Specific Attacks")]
        public float attack1Damage = 10f;
        public float attack1Range = 1.2f;
        public float attack1Cooldown = 0.5f;
        
        public float attack2Damage = 15f;
        public float attack2Range = 1.5f;
        public float attack2Cooldown = 1.0f;
        
        public float attack3Damage = 20f;
        public float attack3Range = 1.8f;
        public float attack3Cooldown = 1.5f;
        
        [Header("Special Attack Properties")]
        public GameObject projectilePrefab; // Dùng cho cả nhân vật cũ và Mũi tên thường/Skill2 của Logic
        public GameObject arrowSkill1Prefab; // Cho nhân vật Logic (S1)
        public GameObject arrowSkill2Prefab; // Cho nhân vật Logic (S2)
        public GameObject arrowSkill3Prefab; // Cho nhân vật Logic (S3)
        public float projectileSpeed = 15f;
        public Vector2 attack3LeapForce = new Vector2(5f, 5f); // For leap attacks
        public float attack3ControlLock = 0.2f;
        
        [Header("New Action Stats")]
        public float skill3DashSpeed = 25f;
        public float skill3DashDuration = 0.5f;
        public float skill3Knockback = 10f;

        [Header("Visuals")]
        public Color mainColor = Color.white;
        public Color effectColor = Color.red;
        
        [Header("Special Abilities Flags")]
        [Tooltip("Có thể đỡ đòn bằng nút Block?")]
        public bool canBlock = false;
        [Tooltip("Có thể lướt bằng nút Dash?")]
        public bool canDash = false;
        [Tooltip("Có thể đặt bẫy?")]
        public bool canPlaceTraps = false;
        [Tooltip("Hồi máu khi gây sát thương?")]
        public bool hasLifeSteal = false;
        [Tooltip("Phản đòn khi bị tấn công?")]
        public bool canCounterAttack = false;
        [Tooltip("Kích hoạt Berserk khi máu thấp?")]
        public bool hasBerserkMode = false;
        [Tooltip("Kích hoạt Rage Mode?")]
        public bool hasRageMode = false;
    }
}
