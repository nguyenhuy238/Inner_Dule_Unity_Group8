using UnityEngine;

namespace InnerDuel.Characters
{
    public enum CharacterType
    {
        // Cặp 1: Kỷ Luật vs Ngẫu Hứng
        Discipline,    // Kỷ Luật - The Warden
        Spontaneity,   // Ngẫu Hứng - The Maverick
        
        // Cặp 2: Lý Trí vs Cảm Xúc
        Logic,         // Lý Trí - The Architect
        Creativity,    // Cảm Xúc - The Muse
        
        // Cặp 3: Kiên Trì vs Từ Bỏ
        Persistence,   // Kiên Trì - The Unbroken
        Surrender,     // Từ Bỏ - The Void
        
        // Cặp 4: Tĩnh Lặng vs Thịnh Nộ
        Stillness,     // Tĩnh Lặng - The Zen
        Rage           // Thịnh Nộ - The Berserker
    }
    
    [System.Serializable]
    public class CharacterData
    {
        public CharacterType type;
        public string characterName;
        public string description;
        public float maxHealth = 100f;
        public float moveSpeed = 5f;
        public float attackDamage = 10f;
        public float defense = 5f;
        public Color mainColor = Color.white;
        public Color effectColor = Color.red;
        
        [Header("Special Abilities")]
        public bool canBlock = false;
        public bool canDash = false;
        public bool canPlaceTraps = false;
        public bool hasLifeSteal = false;
        public bool canCounterAttack = false;
        public bool hasBerserkMode = false;
        public bool hasRageMode = false;
    }
}
