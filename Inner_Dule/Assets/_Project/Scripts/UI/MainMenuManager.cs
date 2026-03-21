using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using InnerDuel.Core;
using InnerDuel.Audio;
using System.Collections.Generic;

namespace InnerDuel.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        private const string MasterVolumeKey = "MasterVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string SfxVolumeKey = "SFXVolume";
        private const string ResolutionIndexKey = "ResolutionIndex";
        private const string FullscreenKey = "Fullscreen";

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
        public AudioClip menuMusic;

        private Resolution[] resolutions;

        private void Start()
        {
            SetupMenuAudio();
            InitializeOptions();
            SetPanelState(true, false, false, false);
        }

        private void OnDestroy()
        {
            if (masterVolumeSlider) masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
            if (musicVolumeSlider) musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
            if (sfxVolumeSlider) sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
            if (resolutionDropdown) resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
            if (fullscreenToggle) fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
        }

        private void SetupMenuAudio()
        {
            if (AudioManager.Instance != null && clickSound != null)
            {
                AudioManager.Instance.SetUIClickSound(clickSound);
            }

            if (menuMusic == null) menuMusic = hoverSound;

            if (AudioManager.Instance != null && menuMusic != null)
            {
                string sceneName = SceneManager.GetActiveScene().name;
                AudioManager.Instance.RegisterSceneMusic(sceneName, menuMusic);
                AudioManager.Instance.PlaySceneBGM(sceneName, true);
            }
        }

        #region Navigation Logic

        public void PlayGame()
        {
            PlayClickSound();
            SceneManager.LoadScene(GameData.MapSelectScene);
        }

        public void ShowMainMenu()
        {
            SetPanelState(true, false, false, true);
        }

        public void ShowOptions()
        {
            SetPanelState(false, true, false, true);
        }

        public void ShowCredits()
        {
            SetPanelState(false, false, true, true);
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
            ResolveOptionsReferences();

            // Volume initialization
            float defaultMusic = AudioManager.Instance != null ? AudioManager.Instance.musicBaseVolume : 1f;
            float defaultSfx = AudioManager.Instance != null ? AudioManager.Instance.sfxBaseVolume : 1f;

            float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusic);
            float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfx);

            if (masterVolumeSlider)
            {
                masterVolumeSlider.SetValueWithoutNotify(masterVolume);
                masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }
            
            if (musicVolumeSlider)
            {
                musicVolumeSlider.SetValueWithoutNotify(musicVolume);
                musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (sfxVolumeSlider)
            {
                sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
                sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(masterVolume, false);
                AudioManager.Instance.SetMusicVolume(musicVolume, false);
                AudioManager.Instance.SetSFXVolume(sfxVolume, false);
            }

            // Resolution initialization
            if (resolutionDropdown)
            {
                resolutions = Screen.resolutions;
                if (resolutions != null && resolutions.Length > 0)
                {
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
                    int savedResolutionIndex = Mathf.Clamp(
                        PlayerPrefs.GetInt(ResolutionIndexKey, currentResolutionIndex),
                        0,
                        resolutions.Length - 1
                    );
                    resolutionDropdown.SetValueWithoutNotify(savedResolutionIndex);
                    resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
                    resolutionDropdown.onValueChanged.AddListener(SetResolution);
                    SetResolution(savedResolutionIndex);
                    resolutionDropdown.RefreshShownValue();
                }
            }

            // Fullscreen initialization
            bool isFullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
            if (fullscreenToggle)
            {
                fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);
                fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }

            SetFullscreen(isFullscreen);
        }

        public void SetMasterVolume(float volume)
        {
            float clampedVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MasterVolumeKey, clampedVolume);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(clampedVolume);
            }
            Debug.Log($"Master Volume set to: {clampedVolume}");
        }

        public void SetMusicVolume(float volume)
        {
            float clampedVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MusicVolumeKey, clampedVolume);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(clampedVolume);
            }
            Debug.Log($"Music Volume set to: {clampedVolume}");
        }

        public void SetSFXVolume(float volume)
        {
            float clampedVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SfxVolumeKey, clampedVolume);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(clampedVolume);
            }
            Debug.Log($"SFX Volume set to: {clampedVolume}");
        }

        public void SetResolution(int resolutionIndex)
        {
            if (resolutions == null || resolutions.Length == 0)
            {
                resolutions = Screen.resolutions;
                if (resolutions == null || resolutions.Length == 0) return;
            }

            resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1);
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            PlayerPrefs.SetInt(ResolutionIndexKey, resolutionIndex);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        }

        #endregion

        #region Audio Feedback

        public void PlayHoverSound()
        {
            if (hoverSound == null) return;

            // Hover SFX should stay local to MainMenu scene (avoid carrying over via persistent AudioManager).
            if (!uiAudioSource) uiAudioSource = GetComponent<AudioSource>();
            if (!uiAudioSource) return;

            uiAudioSource.PlayOneShot(hoverSound);
        }

        private void PlayClickSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIClick();
            }
            else if (uiAudioSource && clickSound != null)
            {
                uiAudioSource.PlayOneShot(clickSound);
            }
        }

        private void SetPanelState(bool showMain, bool showOptions, bool showCredits, bool playClick)
        {
            if (playClick) PlayClickSound();
            if (mainMenuPanel) mainMenuPanel.SetActive(showMain);
            if (optionsPanel) optionsPanel.SetActive(showOptions);
            if (creditsPanel) creditsPanel.SetActive(showCredits);
        }

        private void ResolveOptionsReferences()
        {
            if (!optionsPanel) return;

            if (!masterVolumeSlider)
            {
                Transform master = optionsPanel.transform.Find("MasterVolumeSlider");
                if (master) masterVolumeSlider = master.GetComponent<Slider>();
            }

            if (!musicVolumeSlider)
            {
                Transform music = optionsPanel.transform.Find("MusicVolumeSlider");
                if (music) musicVolumeSlider = music.GetComponent<Slider>();
            }

            if (!sfxVolumeSlider)
            {
                Transform sfx = optionsPanel.transform.Find("SFXVolumeSlider");
                if (sfx) sfxVolumeSlider = sfx.GetComponent<Slider>();
            }
        }

        #endregion
    }
}
