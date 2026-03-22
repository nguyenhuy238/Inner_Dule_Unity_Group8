using UnityEngine;
using UnityEditor;
using InnerDuel.Characters;

namespace InnerDuel.Editor
{
    /// <summary>
    /// Tool to automatically add CharacterAnimationAudioEvents to all character prefabs
    /// Menu: Tools/Add Audio Events Component to Character Prefabs
    /// </summary>
    public class AddAudioEventsComponentToPrefabs : EditorWindow
    {
        [MenuItem("Tools/Add Audio Events Component to Character Prefabs")]
        public static void AddAudioEventsComponent()
        {
            // Find all character prefabs
            string[] prefabPaths = new string[]
            {
                "Assets/_Project/Prefabs/Prefabs/Characters/BaseCharacter.prefab",
                "Assets/_Project/Prefabs/Prefabs/Characters/Char_Creativity.prefab",
                "Assets/_Project/Prefabs/Prefabs/Characters/Char_Emotion.prefab",
                "Assets/_Project/Prefabs/Prefabs/Characters/Char_Logic.prefab",
                "Assets/_Project/Prefabs/Prefabs/Characters/Char_Reason.prefab",
                "Assets/_Project/Prefabs/Prefabs/Characters/Discipline_Character.prefab",
                "Assets/_Project/Prefabs/Prefabs/Characters/Spontaneity_Character.prefab"
            };
            
            int modifiedCount = 0;
            
            foreach (string path in prefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab == null)
                {
                    Debug.LogWarning($"Prefab not found: {path}");
                    continue;
                }
                
                // Open prefab for editing
                string assetPath = AssetDatabase.GetAssetPath(prefab);
                GameObject prefabInstance = PrefabUtility.LoadPrefabContents(assetPath);
                
                // Check if component already exists
                var existingComponent = prefabInstance.GetComponent<CharacterAnimationAudioEvents>();
                
                if (existingComponent != null)
                {
                    Debug.Log($"  -> {prefab.name}: Already has CharacterAnimationAudioEvents");
                    PrefabUtility.UnloadPrefabContents(prefabInstance);
                    continue;
                }
                
                // Add component
                var audioEvents = prefabInstance.AddComponent<CharacterAnimationAudioEvents>();
                
                // Try to auto-assign controller
                var controller = prefabInstance.GetComponent<InnerCharacterController>();
                if (controller != null)
                {
                    // Use reflection to set the private field
                    var field = typeof(CharacterAnimationAudioEvents).GetField("controller", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(audioEvents, controller);
                    }
                }
                
                // Save prefab
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath);
                PrefabUtility.UnloadPrefabContents(prefabInstance);
                
                modifiedCount++;
                Debug.Log($"  -> {prefab.name}: Added CharacterAnimationAudioEvents component");
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog(
                "Component Added", 
                $"Successfully added CharacterAnimationAudioEvents to {modifiedCount} character prefabs!\n\n" +
                "Now run: Tools → Add Footstep Events to Run Animations", 
                "OK"
            );
        }
    }
}
