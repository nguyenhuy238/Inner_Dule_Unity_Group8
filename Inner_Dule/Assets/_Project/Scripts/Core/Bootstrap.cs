using UnityEngine;
using UnityEngine.SceneManagement;
using InnerDuel;
using InnerDuel.Audio;
using InnerDuel.Input;

namespace InnerDuel.Core
{
    /// <summary>
    /// Handles the initialization of all core systems and transitions to the main game.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string nextSceneName = "MainGameScene";
        [SerializeField] private float minSplashDuration = 2f;

        private void Start()
        {
            InitializeManagers();
            StartCoroutine(NavigateToNextScene());
        }

        private void InitializeManagers()
        {
            Debug.Log("[Bootstrap] Initializing core managers...");
            
            // Accessing Instance properties will trigger singleton creation if they don't exist
            var gameManager = GameManager.Instance;
            var audioManager = AudioManager.Instance;
            var inputManager = InputManager.Instance;

            Debug.Log("[Bootstrap] Managers initialized.");
        }

        private System.Collections.IEnumerator NavigateToNextScene()
        {
            // Ensure any splash screens or branding are visible for at least minSplashDuration
            yield return new WaitForSeconds(minSplashDuration);

            Debug.Log($"[Bootstrap] Loading scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
