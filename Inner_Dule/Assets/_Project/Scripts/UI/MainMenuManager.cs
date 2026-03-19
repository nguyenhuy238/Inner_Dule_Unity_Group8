using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using InnerDuel.Core;
using System.Collections.Generic;

namespace InnerDuel.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Menu Panels")]
        public GameObject mainMenuPanel;
        public GameObject optionsPanel;
        public GameObject creditsPanel;

        [Header("Options UI Elements")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public TMP_Dropdown resolutionDropdown;
        public Toggle fullscreenToggle;

        [Header("Audio Settings")]
        public AudioSource uiAudioSource;
        public AudioClip hoverSound;
        public AudioClip clickSound;

        private Resolution[] resolutions;

        private void Start()
        {
            InitializeOptions();
            ShowMainMenu();
        }

        #region Navigation Logic

        public void PlayGame()
        {
            PlayClickSound();
            SceneManager.LoadScene(GameData.MapSelectScene);
        }

        public void ShowMainMenu()
        {
            PlayClickSound();
            if (mainMenuPanel) mainMenuPanel.SetActive(true);
            if (optionsPanel) optionsPanel.SetActive(false);
            if (creditsPanel) creditsPanel.SetActive(false);
        }

        public void ShowOptions()
        {
            PlayClickSound();
            if (mainMenuPanel) mainMenuPanel.SetActive(false);
            if (optionsPanel) optionsPanel.SetActive(true);
        }

        public void ShowCredits()
        {
            PlayClickSound();
            if (mainMenuPanel) mainMenuPanel.SetActive(false);
            if (creditsPanel) creditsPanel.SetActive(true);
        }

        public void QuitGame()
        {
            PlayClickSound();
            Debug.Log("Quitting Game...");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        #endregion

        #region Options Logic

        private void InitializeOptions()
        {
            // Volume initialization
            if (masterVolumeSlider)
            {
                masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }
            
            if (musicVolumeSlider)
            {
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (sfxVolumeSlider)
            {
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            }

            // Resolution initialization
            if (resolutionDropdown)
            {
                resolutions = Screen.resolutions;
                resolutionDropdown.ClearOptions();

                List<string> options = new List<string>();
                int currentResolutionIndex = 0;

                for (int i = 0; i < resolutions.Length; i++)
                {
                    string option = resolutions[i].width + " x " + resolutions[i].height;
                    options.Add(option);

                    if (resolutions[i].width == Screen.currentResolution.width &&
                        resolutions[i].height == Screen.currentResolution.height)
                    {
                        currentResolutionIndex = i;
                    }
                }

                resolutionDropdown.AddOptions(options);
                resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
                resolutionDropdown.RefreshShownValue();
                resolutionDropdown.onValueChanged.AddListener(SetResolution);
            }

            // Fullscreen initialization
            if (fullscreenToggle)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }
        }

        public void SetMasterVolume(float volume)
        {
            PlayerPrefs.SetFloat("MasterVolume", volume);
            // Add actual audio mixer logic here if available
            Debug.Log($"Master Volume set to: {volume}");
        }

        public void SetMusicVolume(float volume)
        {
            PlayerPrefs.SetFloat("MusicVolume", volume);
            Debug.Log($"Music Volume set to: {volume}");
        }

        public void SetSFXVolume(float volume)
        {
            PlayerPrefs.SetFloat("SFXVolume", volume);
            Debug.Log($"SFX Volume set to: {volume}");
        }

        public void SetResolution(int resolutionIndex)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        }

        #endregion

        #region Audio Feedback

        public void PlayHoverSound()
        {
            if (uiAudioSource && hoverSound)
            {
                uiAudioSource.PlayOneShot(hoverSound);
            }
        }

        private void PlayClickSound()
        {
            if (uiAudioSource && clickSound)
            {
                uiAudioSource.PlayOneShot(clickSound);
            }
        }

        #endregion
    }
}
