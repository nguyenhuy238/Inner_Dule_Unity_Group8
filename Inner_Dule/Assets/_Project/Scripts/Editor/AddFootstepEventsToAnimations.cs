using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace InnerDuel.Editor
{
    /// <summary>
    /// Tool to automatically add footstep animation events to Run animations
    /// Menu: Tools/Add Footstep Events to Run Animations
    /// </summary>
    public class AddFootstepEventsToAnimations : EditorWindow
    {
        [MenuItem("Tools/Add Footstep Events to Run Animations")]
        public static void AddFootstepEvents()
        {
            // Find all Run.anim files
            string[] guids = AssetDatabase.FindAssets("Run t:AnimationClip", new[] { "Assets/_Project/Art/Animations" });
            
            int modifiedCount = 0;
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                
                if (clip == null) continue;
                
                // Get animation length
                float length = clip.length;
                float frameRate = clip.frameRate;
                int totalFrames = Mathf.FloorToInt(length * frameRate);
                
                Debug.Log($"Processing: {path} (Length: {length}s, Frames: {totalFrames})");
                
                // Calculate footstep timing (typically 2-4 steps per run cycle)
                // Adjust these values based on your animations
                List<float> footstepTimes = new List<float>();
                
                if (totalFrames >= 10)
                {
                    // Add footstep events at 25%, 50%, 75% of animation
                    footstepTimes.Add(length * 0.25f);
                    footstepTimes.Add(length * 0.5f);
                    footstepTimes.Add(length * 0.75f);
                }
                else
                {
                    // Short animation - just 2 events
                    footstepTimes.Add(length * 0.33f);
                    footstepTimes.Add(length * 0.66f);
                }
                
                // Get existing events
                AnimationEvent[] existingEvents = AnimationUtility.GetAnimationEvents(clip);
                List<AnimationEvent> newEvents = new List<AnimationEvent>(existingEvents);
                
                // Check if footstep events already exist
                bool hasFootsteps = false;
                foreach (var evt in existingEvents)
                {
                    if (evt.functionName == "AE_PlayFootstepSfx")
                    {
                        hasFootsteps = true;
                        break;
                    }
                }
                
                if (hasFootsteps)
                {
                    Debug.Log($"  -> Skipped (already has footstep events)");
                    continue;
                }
                
                // Add new footstep events
                foreach (float time in footstepTimes)
                {
                    AnimationEvent evt = new AnimationEvent
                    {
                        time = time,
                        functionName = "AE_PlayFootstepSfx"
                    };
                    newEvents.Add(evt);
                }
                
                // Apply events
                AnimationUtility.SetAnimationEvents(clip, newEvents.ToArray());
                EditorUtility.SetDirty(clip);
                modifiedCount++;
                
                Debug.Log($"  -> Added {footstepTimes.Count} footstep events");
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog(
                "Footstep Events Added", 
                $"Successfully added footstep events to {modifiedCount} Run animations!", 
                "OK"
            );
        }
    }
}
