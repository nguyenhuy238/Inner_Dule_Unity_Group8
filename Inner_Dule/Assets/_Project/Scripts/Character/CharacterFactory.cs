using UnityEngine;

namespace InnerDuel.Characters
{
    public class CharacterFactory : MonoBehaviour
    {
        [Header("Character Prefabs")]
        public GameObject disciplinePrefab;
        public GameObject spontaneityPrefab;
        public GameObject logicPrefab;
        public GameObject creativityPrefab;
        public GameObject persistencePrefab;
        public GameObject surrenderPrefab;
        public GameObject stillnessPrefab;
        public GameObject ragePrefab;
        
        [Header("Character Data")]
        public CharacterData[] characterDataArray;
        
        private static CharacterFactory instance;
        
        public static CharacterFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<CharacterFactory>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("CharacterFactory");
                        instance = go.AddComponent<CharacterFactory>();
                    }
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        public GameObject CreateCharacter(CharacterType type, Vector3 position, int playerID)
        {
            GameObject prefab = GetCharacterPrefab(type);
            CharacterData data = GetCharacterData(type);
            
            if (prefab == null)
            {
                // This will already be logged by GetCharacterPrefab
                return null;
            }
            
            if (data == null)
            {
                Debug.LogWarning($"[InnerDuel] Data for {type} not found, using default data.");
                data = CreateDefaultCharacterData(type);
            }
            
            GameObject characterObj = Instantiate(prefab, position, Quaternion.identity);
            characterObj.name = $"{type}_P{playerID}";
            
            // Setup character controller
            InnerDuel.Characters.InnerCharacterController controller = characterObj.GetComponent<InnerDuel.Characters.InnerCharacterController>();
            if (controller != null)
            {
                controller.characterData = data;
                controller.playerID = playerID;
                
                // Set layer based on player ID
                characterObj.layer = playerID == 1 ? LayerMask.NameToLayer("Player1") : LayerMask.NameToLayer("Player2");
            }
            
            return characterObj;
        }
        
        private GameObject GetCharacterPrefab(CharacterType type)
        {
            GameObject selectedPrefab = null;
            
            switch (type)
            {
                case CharacterType.Discipline: selectedPrefab = disciplinePrefab; break;
                case CharacterType.Spontaneity: selectedPrefab = spontaneityPrefab; break;
                case CharacterType.Logic: selectedPrefab = logicPrefab; break;
                case CharacterType.Creativity: selectedPrefab = creativityPrefab; break;
                case CharacterType.Persistence: selectedPrefab = persistencePrefab; break;
                case CharacterType.Surrender: selectedPrefab = surrenderPrefab; break;
                case CharacterType.Stillness: selectedPrefab = stillnessPrefab; break;
                case CharacterType.Rage: selectedPrefab = ragePrefab; break;
            }
            
            if (selectedPrefab == null)
            {
                Debug.LogError($"[InnerDuel] Prefab for {type} is NOT assigned in CharacterFactory!");

                // Fallback: try to find any assigned prefab to avoid returning null
                selectedPrefab = disciplinePrefab ?? spontaneityPrefab ?? logicPrefab ?? 
                                 creativityPrefab ?? persistencePrefab ?? surrenderPrefab ?? 
                                 stillnessPrefab ?? ragePrefab;
                                 
                if (selectedPrefab != null)
                {
                    Debug.LogWarning($"[InnerDuel] Using fallback prefab '{selectedPrefab.name}' for {type} to prevent crash.");
                }
                else
                {
                    Debug.LogError("[InnerDuel] CRITICAL: No prefabs whatsoever assigned in CharacterFactory! Cannot spawn characters.");
                }
            }
            
            return selectedPrefab;
        }
        
        private CharacterData GetCharacterData(CharacterType type)
        {
            if (characterDataArray != null)
            {
                foreach (CharacterData data in characterDataArray)
                {
                    if (data.type == type)
                    {
                        return data;
                    }
                }
            }
            
            // Return default data if not found
            return CreateDefaultCharacterData(type);
        }
        
        private CharacterData CreateDefaultCharacterData(CharacterType type)
        {
            CharacterData data = new CharacterData();
            data.type = type;
            
            switch (type)
            {
                case CharacterType.Discipline:
                    data.characterName = "Kỷ Luật";
                    data.description = "The Warden - Chỉnh chu, nghiêm túc. Có khả năng chặn đòn và phản công mạnh mẽ.";
                    data.maxHealth = 130f;
                    data.moveSpeed = 2.5f;
                    data.defense = 10f;
                    data.attackDamage = 15f;
                    data.attackRange = 1.3f;
                    data.attackCooldown = 0.7f;
                    data.mainColor = Color.blue;
                    data.effectColor = Color.yellow;
                    data.canBlock = true;
                    data.canCounterAttack = true;
                    break;
                    
                case CharacterType.Spontaneity:
                    data.characterName = "Ngẫu Hứng";
                    data.description = "The Maverick - Tự do, linh hoạt. Tốc độ cao và khả năng lướt biến ảo.";
                    data.maxHealth = 85f;
                    data.moveSpeed = 8f;
                    data.defense = 3f;
                    data.attackDamage = 8f;
                    data.attackRange = 1.1f;
                    data.attackCooldown = 0.4f;
                    data.mainColor = Color.white;
                    data.effectColor = Color.cyan;
                    data.canDash = true;
                    break;
                    
                case CharacterType.Logic:
                    data.characterName = "Lý Trí";
                    data.description = "The Architect - Tính toán, tầm xa. Có thể đặt bẫy khống chế đối thủ.";
                    data.maxHealth = 100f;
                    data.moveSpeed = 4f;
                    data.defense = 5f;
                    data.attackDamage = 12f;
                    data.attackRange = 2.5f;
                    data.attackCooldown = 0.8f;
                    data.mainColor = Color.green;
                    data.effectColor = Color.blue;
                    data.canPlaceTraps = true;
                    break;
                    
                case CharacterType.Creativity:
                    data.characterName = "Sáng Tạo";
                    data.description = "The Muse - Ngẫu hứng, khó đoán. Tầm đánh và sát thương thay đổi liên tục.";
                    data.maxHealth = 90f;
                    data.moveSpeed = 6f;
                    data.defense = 4f;
                    data.attackDamage = 10f;
                    data.attackRange = 1.5f;
                    data.attackCooldown = 0.5f;
                    data.mainColor = Color.magenta;
                    data.effectColor = Color.yellow;
                    break;
                    
                case CharacterType.Persistence:
                    data.characterName = "Kiên Trì";
                    data.description = "The Unbroken - Bền bỉ, lầm lì. Càng mất nhiều máu càng trở nên nguy hiểm.";
                    data.maxHealth = 110f;
                    data.moveSpeed = 4f;
                    data.defense = 7f;
                    data.attackDamage = 10f;
                    data.attackRange = 1.2f;
                    data.attackCooldown = 0.6f;
                    data.mainColor = Color.gray;
                    data.effectColor = Color.white;
                    break;
                    
                case CharacterType.Surrender:
                    data.characterName = "Từ Bỏ";
                    data.description = "The Void - Hư vô, đáng sợ. Hút sinh lực và làm suy yếu ý chí đối thủ.";
                    data.maxHealth = 85f;
                    data.moveSpeed = 5f;
                    data.defense = 4f;
                    data.attackDamage = 7f;
                    data.attackRange = 1.4f;
                    data.attackCooldown = 0.5f;
                    data.mainColor = Color.black;
                    data.effectColor = Color.grey;
                    data.hasLifeSteal = true;
                    break;
                    
                case CharacterType.Stillness:
                    data.characterName = "Tĩnh Lặng";
                    data.description = "The Zen - Điềm tĩnh, sâu sắc. Phản đòn mạnh mẽ khi đối thủ sơ hở.";
                    data.maxHealth = 95f;
                    data.moveSpeed = 3f;
                    data.defense = 6f;
                    data.attackDamage = 14f;
                    data.attackRange = 1.2f;
                    data.attackCooldown = 0.7f;
                    data.mainColor = Color.gray;
                    data.effectColor = Color.white;
                    data.canCounterAttack = true;
                    break;
                    
                case CharacterType.Rage:
                    data.characterName = "Thịnh Nộ";
                    data.description = "The Berserker - Cuồng bạo, khát máu. Sức mạnh bộc phát khi rơi vào trạng thái nguy kịch.";
                    data.maxHealth = 100f;
                    data.moveSpeed = 6f;
                    data.defense = 3f;
                    data.attackDamage = 12f;
                    data.attackRange = 1.1f;
                    data.attackCooldown = 0.4f;
                    data.mainColor = Color.red;
                    data.effectColor = Color.yellow;
                    data.hasBerserkMode = true;
                    break;
            }
            
            return data;
        }
        
        public CharacterData[] GetAllCharacterData()
        {
            if (characterDataArray != null && characterDataArray.Length > 0)
            {
                return characterDataArray;
            }
            
            // Create default data array if none exists
            CharacterData[] defaultData = new CharacterData[8];
            for (int i = 0; i < 8; i++)
            {
                defaultData[i] = CreateDefaultCharacterData((CharacterType)i);
            }
            
            return defaultData;
        }
    }
}
