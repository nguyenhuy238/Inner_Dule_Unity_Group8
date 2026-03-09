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
        Stillness,     // Tĩnh Lặng - The Zen
        Rage           // Thịnh Nộ - The Berserker
    }
    
    /// <summary>
    /// Chứa tất cả thông số cấu hình của một nhân vật.
    /// Team nên thay đổi ở đây để tạo sự khác biệt giữa các nhân vật.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterData", menuName = "InnerDuel/Character/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        public CharacterType type;
        public string characterName;
        [TextArea(3, 10)]
        public string description;
        
        [Header("Base Stats")]
        public float maxHealth = 100f;
        public float moveSpeed = 5f;
        public float defense = 5f; // Giảm sát thương nhận vào
        
        [Header("Combat Stats")]
        public float attackDamage = 10f;
        public float attackRange = 1.2f;
        public float attackCooldown = 0.5f;
        public float dashSpeedMultiplier = 3f;
        public float dashDuration = 0.2f;
        
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
