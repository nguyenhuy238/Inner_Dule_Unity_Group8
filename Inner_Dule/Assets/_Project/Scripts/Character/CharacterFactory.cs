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
        public GameObject stillnessPrefab; // now Reason
        public GameObject ragePrefab;      // now Emotion

#if UNITY_EDITOR
        private void OnValidate()
        {
            AutoAssignPrefabs();
        }

        private void AutoAssignPrefabs()
        {
            if (disciplinePrefab == null) disciplinePrefab = FindPrefab("Discipline_Character");
            if (spontaneityPrefab == null) spontaneityPrefab = FindPrefab("Spontaneity_Character");
            if (logicPrefab == null) logicPrefab = FindPrefab("Char_Logic") ;
            if (creativityPrefab == null) creativityPrefab = FindPrefab("Char_Creative") ;
        }
        //?? FindPrefab("Player1")
        //    ?? FindPrefab("Player2")
        //Creativity
        private GameObject FindPrefab(string name)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets(name + " t:Prefab");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            return null;
        }
#endif
        
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
                    characterObj.AddComponent<Ability_DisciplineParry>(); // Giữ nguyên
                    break;
                case CharacterType.Spontaneity:
                    characterObj.AddComponent<Ability_SpontaneityDash>(); // Giữ nguyên
                    break;
                case CharacterType.Reason:
                    characterObj.AddComponent<Ability_LogicArcher>(); // Thêm mới cho Logic/Reason
                    break;
                case CharacterType.Creativity:
                    characterObj.AddComponent<Ability_Creative>(); // Thêm mới cho Logic
                    break;
                case CharacterType.Emotion:
                    characterObj.AddComponent<Ability_EmotionAxe>(); // Thêm mới cho Emotion/Axe
                    break;
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
                case CharacterType.Reason: selectedPrefab = stillnessPrefab; break;
                case CharacterType.Emotion: selectedPrefab = ragePrefab; break;
            }
            
            if (selectedPrefab == null)
            {
                Debug.LogError($"[InnerDuel] Prefab for {type} is NOT assigned in CharacterFactory!");

                // Fallback: try to find any assigned prefab to avoid returning null
                selectedPrefab = disciplinePrefab != null ? disciplinePrefab :
                                spontaneityPrefab != null ? spontaneityPrefab :
                                logicPrefab != null ? logicPrefab : 
                                creativityPrefab != null ? creativityPrefab :
                                stillnessPrefab != null ? stillnessPrefab :
                                ragePrefab;
                                 
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
                    data.moveSpeed = 4f;
                    data.defense = 15f;
                    
                    data.jumpForce = 12f;
                    data.airControlMultiplier = 0.5f;
                    
                    data.attackDamage = 20f;
                    data.attackRange = 1.3f;
                    data.attackCooldown = 0.8f;
                    
                    data.attack1Damage = 15f; data.attack1Range = 1.3f; data.attack1Cooldown = 0.5f;
                    data.attack2Damage = 20f; data.attack2Range = 1.4f; data.attack2Cooldown = 1.0f;
                    data.attack3Damage = 25f; data.attack3Range = 1.5f; data.attack3Cooldown = 1.5f;
                    
                    data.mainColor = new Color(0.2f, 0.4f, 1f);
                    data.effectColor = Color.yellow;
                    data.canBlock = true;
                    data.canCounterAttack = true;
                    break;
                    
                case CharacterType.Spontaneity:
                    data.characterName = "Ngẫu Hứng (The Maverick) [Fallback]";
                    data.maxHealth = 100f;
                    data.moveSpeed = 7.5f;
                    data.defense = 2f;
                    
                    data.jumpForce = 14f;
                    data.airControlMultiplier = 0.9f;
                    
                    data.attackDamage = 12f;
                    data.attackRange = 1.1f;
                    data.attackCooldown = 0.35f;
                    
                    data.attack1Damage = 10f; data.attack1Range = 1.1f; data.attack1Cooldown = 0.3f;
                    data.attack2Damage = 12f; data.attack2Range = 1.2f; data.attack2Cooldown = 0.5f;
                    data.attack3Damage = 15f; data.attack3Range = 1.3f; data.attack3Cooldown = 0.8f;
                    
                    data.attack3LeapForce = new Vector2(8f, 5f);
                    
                    data.dashSpeedMultiplier = 4f;
                    data.dashDuration = 0.25f;
                    data.mainColor = Color.white;
                    data.effectColor = Color.cyan;
                    data.canBlock = true;
                    data.canDash = true;
                    break;
                
                case CharacterType.Reason:
                    data.characterName = "Reason (Cung thủ) [Fallback]";
                    data.maxHealth = 110f;
                    data.moveSpeed = 6.5f;
                    data.defense = 4f;
                    data.jumpForce = 13f;
                    data.airControlMultiplier = 0.85f;
                    data.normalAttackDamage = 8f;
                    data.attack1Damage = 12f;
                    data.attack2Damage = 10f; // Mỗi mũi tên trong chùm 3
                    data.attack3Damage = 25f;
                    data.mainColor = new Color(0.4f, 1f, 0.4f); // Greenish
                    data.effectColor = Color.green;
                    data.canDash = true;
                    break;

                case CharacterType.Emotion:
                    data.characterName = "Emotion (Kỵ sĩ cầm rìu) [Fallback]";
                    data.maxHealth = 180f;
                    data.moveSpeed = 4.5f;
                    data.defense = 12f;
                    data.jumpForce = 11f;
                    data.airControlMultiplier = 0.4f;
                    data.normalAttackDamage = 15f;
                    data.attack1Damage = 20f;
                    data.attack2Damage = 25f;
                    data.attack3Damage = 35f;
                    data.mainColor = new Color(1f, 0.4f, 0.4f); // Reddish
                    data.effectColor = Color.red;
                    data.canBlock = true;
                    data.hasBerserkMode = true;
                    data.attack3LeapForce = new Vector2(6f, 4f);
                    break;
                
                default:
                    data.characterName = type.ToString() + " [Fallback]";
                    data.maxHealth = 100f;
                    data.moveSpeed = 5f;
                    data.attackDamage = 10f;
                    
                    data.attack1Damage = 10f; data.attack1Range = 1.2f; data.attack1Cooldown = 0.5f;
                    data.attack2Damage = 12f; data.attack2Range = 1.4f; data.attack2Cooldown = 0.8f;
                    data.attack3Damage = 15f; data.attack3Range = 1.6f; data.attack3Cooldown = 1.2f;
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
            CharacterData[] defaultData = new CharacterData[6];
            for (int i = 0; i < 6; i++)
            {
                defaultData[i] = CreateDefaultCharacterData((CharacterType)i);
            }
            
            return defaultData;
        }
    }
}
