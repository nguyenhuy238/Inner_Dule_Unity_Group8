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
            
            Debug.Log($"[CharacterFactory] Successfully instantiated {characterObj.name} at {position}.");
            
            // Setup character controller
            InnerDuel.Characters.InnerCharacterController controller = characterObj.GetComponent<InnerDuel.Characters.InnerCharacterController>();
            if (controller != null)
            {
                controller.characterData = data;
                controller.playerID = playerID;
                
                // Add Unique Abilities based on CharacterType
                AddUniqueAbilities(characterObj, type);
                
                // Ensure controller initializes with the new data
                controller.InitializeFromData();
                
                // Set layer based on player ID
                characterObj.layer = playerID == 1 ? LayerMask.NameToLayer("Player1") : LayerMask.NameToLayer("Player2");
            }
            
            return characterObj;
        }

        private void AddUniqueAbilities(GameObject characterObj, CharacterType type)
        {
            switch (type)
            {
                case CharacterType.Discipline:
                    characterObj.AddComponent<Ability_DisciplineParry>();
                    break;
                case CharacterType.Spontaneity:
                    characterObj.AddComponent<Ability_SpontaneityDash>();
                    break;
                // Add more cases here for other characters as their abilities are implemented
            }
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
            Debug.LogWarning($"[InnerDuel] CharacterData SO for {type} is missing! Using runtime default stats. Please create the SO asset in the Editor for persistence.");
            
            CharacterData data = ScriptableObject.CreateInstance<CharacterData>();
            data.type = type;
            
            // Re-implementing the default stats as fallback
            switch (type)
            {
                case CharacterType.Discipline:
                    data.characterName = "Kỷ Luật (The Warden) [Fallback]";
                    data.maxHealth = 150f;
                    data.moveSpeed = 3f;
                    data.defense = 15f;
                    data.attackDamage = 20f;
                    data.attackRange = 1.3f;
                    data.attackCooldown = 0.8f;
                    data.mainColor = new Color(0.2f, 0.4f, 1f);
                    data.effectColor = Color.yellow;
                    data.canBlock = true;
                    data.canCounterAttack = true;
                    break;
                    
                case CharacterType.Spontaneity:
                    data.characterName = "Ngẫu Hứng (The Maverick) [Fallback]";
                    data.maxHealth = 80f;
                    data.moveSpeed = 7.5f;
                    data.defense = 2f;
                    data.attackDamage = 12f;
                    data.attackRange = 1.1f;
                    data.attackCooldown = 0.35f;
                    data.dashSpeedMultiplier = 4f;
                    data.dashDuration = 0.25f;
                    data.mainColor = Color.white;
                    data.effectColor = Color.cyan;
                    data.canDash = true;
                    break;
                
                default:
                    data.characterName = type.ToString() + " [Fallback]";
                    data.maxHealth = 100f;
                    data.moveSpeed = 5f;
                    data.attackDamage = 10f;
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
