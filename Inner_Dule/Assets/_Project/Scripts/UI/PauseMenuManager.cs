using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using InnerDuel.Core;
using InnerDuel.Audio;

namespace InnerDuel.UI
{
    public class PauseMenuManager : MonoBehaviour
    {
        public GameObject pauseMenuPanel;
        public static bool isPaused = false;

        private void Update()
        {
            // Using New Input System's Keyboard.current for direct menu toggle
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (isPaused) Resume();
                else Pause();
            }
        }

        public void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;
            if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
            if (AudioManager.Instance != null) AudioManager.Instance.PauseMusic();
        }

        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;
            if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.ResumeMusic();
        }

        public void RestartMatch()
        {
            Time.timeScale = 1f;
            isPaused = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void QuitToMainMenu()
        {
            Time.timeScale = 1f;
            isPaused = false;
            SceneManager.LoadScene(GameData.MainMenuScene);
        }
    }
}
