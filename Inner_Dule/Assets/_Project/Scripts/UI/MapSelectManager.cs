using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using InnerDuel.Core;
using InnerDuel.Audio;

namespace InnerDuel.UI
{
    public class MapSelectManager : MonoBehaviour
    {
        [Header("Map Data")]
        public List<MapData> availableMaps;
        
        [Header("UI References")]
        public TextMeshProUGUI mapNameText;
        public TextMeshProUGUI mapDescriptionText;
        public Image mapPreviewImage;
        public Button confirmButton;
        public Button backButton;

        [Header("Navigation")]
        private int currentMapIndex = 0;

        [Header("Audio")]
        public AudioClip sceneMusic;

        private void Start()
        {
            SetupSceneAudio();
            UpdateUI();
            
            if (confirmButton != null)
                confirmButton.onClick.AddListener(ConfirmSelection);
            
            if (backButton != null)
                backButton.onClick.AddListener(BackToMenu);
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
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Simple navigation
            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                ChangeSelection(-1);
            }
            if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                ChangeSelection(1);
            }
            if (keyboard.fKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
            {
                ConfirmSelection();
            }
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                BackToMenu();
            }
        }

        public void ChangeSelection(int direction)
        {
            if (availableMaps.Count == 0) return;
            
            currentMapIndex = (currentMapIndex + direction + availableMaps.Count) % availableMaps.Count;
            PlayUIClick();
            UpdateUI();
        }

        public void SelectMap(int index)
        {
            if (index >= 0 && index < availableMaps.Count)
            {
                currentMapIndex = index;
                PlayUIClick();
                UpdateUI();
            }
        }

        public void SelectMap(MapData map)
        {
            int index = availableMaps.IndexOf(map);
            if (index != -1)
            {
                currentMapIndex = index;
                PlayUIClick();
                UpdateUI();
            }
        }

        public void SelectMap(string mapName)
        {
            int index = availableMaps.FindIndex(m => m.mapName == mapName);
            if (index != -1)
            {
                currentMapIndex = index;
                PlayUIClick();
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (availableMaps == null || availableMaps.Count == 0) return;

            MapData currentMap = availableMaps[currentMapIndex];

            if (mapNameText != null) mapNameText.text = currentMap.mapName;
            if (mapDescriptionText != null) mapDescriptionText.text = currentMap.description;
            if (mapPreviewImage != null) mapPreviewImage.sprite = currentMap.previewImage;
        }

        public void ConfirmSelection()
        {
            if (availableMaps.Count == 0) return;
            
            PlayUIClick();
            GameData.selectedMap = availableMaps[currentMapIndex];
            SceneManager.LoadScene(GameData.CharacterSelectScene);
        }

        public void BackToMenu()
        {
            PlayUIClick();
            SceneManager.LoadScene(GameData.MainMenuScene);
        }

        private void PlayUIClick()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClick();
            }
        }
    }
}
