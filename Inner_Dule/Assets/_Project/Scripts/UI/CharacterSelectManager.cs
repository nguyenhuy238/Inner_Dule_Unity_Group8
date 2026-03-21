using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using InnerDuel.Core;
using InnerDuel.Characters;
using InnerDuel.Audio;

namespace InnerDuel.UI
{
    public class CharacterSelectManager : MonoBehaviour
    {
        [Header("Character Data")]
        public List<CharacterData> availableCharacters;
        
        [Header("P1 UI")]
        public TextMeshProUGUI p1SelectionName;
        public Image p1Portrait;
        public TextMeshProUGUI p1Status;
        
        [Header("P2 UI")]
        public TextMeshProUGUI p2SelectionName;
        public Image p2Portrait;
        public TextMeshProUGUI p2Status;

        private int p1Index = 0;
        private int p2Index = 1;

        private bool p1Confirmed = false;
        private bool p2Confirmed = false;

        [Header("Audio")]
        public AudioClip sceneMusic;

        private void Start()
        {
            SetupSceneAudio();
            UpdateUI();
        }

        private void SetupSceneAudio()
        {
            if (AudioManager.Instance == null || sceneMusic == null) return;

            string sceneName = SceneManager.GetActiveScene().name;
            AudioManager.Instance.RegisterSceneMusic(sceneName, sceneMusic);
            AudioManager.Instance.PlaySceneBGM(sceneName);
        }

        private void Update()
        {
            // Using New Input System's Keyboard.current for direct menu navigation
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // P1 Control (WASD for selection, F to confirm, G to cancel)
            if (!p1Confirmed)
            {
                if (keyboard.aKey.wasPressedThisFrame) ChangeSelection(1, -1);
                if (keyboard.dKey.wasPressedThisFrame) ChangeSelection(1, 1);
                if (keyboard.fKey.wasPressedThisFrame) ConfirmSelection(1);
            }
            else if (keyboard.gKey.wasPressedThisFrame) CancelSelection(1);

            // P2 Control (Arrows for selection, Enter to confirm, Backspace to cancel)
            if (!p2Confirmed)
            {
                if (keyboard.leftArrowKey.wasPressedThisFrame) ChangeSelection(2, -1);
                if (keyboard.rightArrowKey.wasPressedThisFrame) ChangeSelection(2, 1);
                if (keyboard.enterKey.wasPressedThisFrame) ConfirmSelection(2);
            }
            else if (keyboard.backspaceKey.wasPressedThisFrame) CancelSelection(2);

            // General Navigation
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                BackToMenu();
            }

            if (p1Confirmed && p2Confirmed)
            {
                StartGame();
            }
        }

        public void ChangeSelection(int playerID, int direction)
        {
            if (playerID == 1)
            {
                p1Index = (p1Index + direction + availableCharacters.Count) % availableCharacters.Count;
            }
            else
            {
                p2Index = (p2Index + direction + availableCharacters.Count) % availableCharacters.Count;
            }
            UpdateUI();
        }

        public void ConfirmSelection(int playerID)
        {
            if (playerID == 1) p1Confirmed = true;
            else p2Confirmed = true;
            UpdateUI();
        }

        public void CancelSelection(int playerID)
        {
            if (playerID == 1) p1Confirmed = false;
            else p2Confirmed = false;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (availableCharacters == null || availableCharacters.Count == 0) return;

            // Update P1 UI
            CharacterData p1Data = availableCharacters[p1Index];
            if (p1SelectionName) p1SelectionName.text = p1Data.characterName;
            if (p1Portrait) p1Portrait.sprite = p1Data.portrait;
            if (p1Status) p1Status.text = p1Confirmed ? "READY" : "SELECTING...";

            // Update P2 UI
            CharacterData p2Data = availableCharacters[p2Index];
            if (p2SelectionName) p2SelectionName.text = p2Data.characterName;
            if (p2Portrait) p2Portrait.sprite = p2Data.portrait;
            if (p2Status) p2Status.text = p2Confirmed ? "READY" : "SELECTING...";
        }

        public void StartGame()
        {
            GameData.player1Character = availableCharacters[p1Index];
            GameData.player2Character = availableCharacters[p2Index];
            SceneManager.LoadScene(GameData.LoadingScene);
        }

        public void BackToMenu()
        {
            SceneManager.LoadScene(GameData.MainMenuScene);
        }
    }
}
