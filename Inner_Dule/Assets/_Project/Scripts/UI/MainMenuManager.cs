using UnityEngine;
using UnityEngine.SceneManagement;
using InnerDuel.Core;

namespace InnerDuel.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Menu Panels")]
        public GameObject mainMenuPanel;
        public GameObject optionsPanel;
        public GameObject creditsPanel;

        private void Start()
        {
            ShowMainMenu();
        }

        public void PlayGame()
        {
            SceneManager.LoadScene(GameData.CharacterSelectScene);
        }

        public void ShowMainMenu()
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(true);
            if (optionsPanel) optionsPanel.SetActive(false);
            if (creditsPanel) creditsPanel.SetActive(false);
        }

        public void ShowOptions()
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(false);
            if (optionsPanel) optionsPanel.SetActive(true);
        }

        public void ShowCredits()
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(false);
            if (creditsPanel) creditsPanel.SetActive(true);
        }

        public void QuitGame()
        {
            Debug.Log("Quitting Game...");
            Application.Quit();
        }
    }
}
