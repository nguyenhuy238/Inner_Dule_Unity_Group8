using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using InnerDuel.Core;
using InnerDuel.Characters;

namespace InnerDuel.Audio
{
    [Serializable]
    public class SceneBgmProfile
    {
        public string sceneName;
        public AudioClip bgmClip;
        [Range(0f, 2f)] public float volumeScale = 1f;
        public bool loop = true;
    }

    [Serializable]
    public class CharacterActionSfxProfile
    {
        public CharacterType characterType;
        public CharacterAudioAction action;
        public AudioClip[] clips;
        [Range(0f, 2f)] public float volumeScale = 1f;
        [Range(0.5f, 1.5f)] public float pitchMin = 0.95f;
        [Range(0.5f, 1.5f)] public float pitchMax = 1.05f;
    }

    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioSource voiceSource;

        [Header("Scene Music")]
        [Tooltip("BGM theo Scene name. Scene load xong sẽ tự phát theo danh sách này.")]
        public SceneBgmProfile[] sceneBgmProfiles;

        [Header("Character Action SFX")]
        [Tooltip("SFX riêng cho từng nhân vật + hành động (normal, skill1-3, dash, block...).")]
        public CharacterActionSfxProfile[] characterActionSfxProfiles;

        [Header("Audio Clips")]
        public AudioClip[] bgmTracks;
        public AudioClip[] hitSounds;
        public AudioClip[] blockSounds;
        public AudioClip[] dashSounds;
        public AudioClip[] deathSounds;
        public AudioClip[] victorySounds;

        [Header("UI SFX")]
        [Tooltip("Clip click mặc định cho UI Button/phím chọn menu.")]
        public AudioClip uiClickSound;
        [Range(0f, 2f)] public float uiClickVolume = 1f;
        [Range(0.5f, 1.5f)] public float uiClickPitchMin = 0.98f;
        [Range(0.5f, 1.5f)] public float uiClickPitchMax = 1.02f;
        
        [Header("Audio Mixer")]
        public AudioMixer audioMixer;
        
        [Header("Settings")]
        public float musicBaseVolume = 0.5f;
        public float sfxBaseVolume = 0.7f;
        public float voiceBaseVolume = 0.8f;

        private const string MasterVolumeKey = "MasterVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string SfxVolumeKey = "SFXVolume";
        private const string VoiceVolumeKey = "VoiceVolume";

        private readonly Dictionary<string, SceneBgmProfile> sceneBgmLookup = new Dictionary<string, SceneBgmProfile>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SceneBgmProfile> runtimeSceneBgmLookup = new Dictionary<string, SceneBgmProfile>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<CharacterAudioAction, List<CharacterActionSfxProfile>> characterActionLookup = new Dictionary<CharacterAudioAction, List<CharacterActionSfxProfile>>();

        private float currentHealthRatio = 1f;
        private int currentBGMIndex = 0;
        private float masterVolume = 1f;
        private bool initialized = false;
        private Coroutine musicFadeCoroutine;

        protected override void Awake()
        {
            base.Awake();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            base.OnDestroy();
        }

        private void Start()
        {
            InitializeAudio();
            RebuildLookups();

            // Ensure active scene gets the proper BGM right from startup.
            PlaySceneBGM(SceneManager.GetActiveScene().name, true);
        }

        private void InitializeAudio()
        {
            if (initialized) return;
            initialized = true;

            EnsureAudioSources();
            ApplySavedVolumes();
        }

        private void EnsureAudioSources()
        {
            // Create audio sources if not assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;

            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
            }
            voiceSource.loop = false;
            voiceSource.playOnAwake = false;
            voiceSource.spatialBlend = 0f;
        }

        private void ApplySavedVolumes()
        {
            SetMasterVolume(PlayerPrefs.GetFloat(MasterVolumeKey, 1f), false);
            SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumeKey, musicBaseVolume), false);
            SetSFXVolume(PlayerPrefs.GetFloat(SfxVolumeKey, sfxBaseVolume), false);
            SetVoiceVolume(PlayerPrefs.GetFloat(VoiceVolumeKey, voiceBaseVolume), false);
        }

        public void RebuildLookups()
        {
            sceneBgmLookup.Clear();
            if (sceneBgmProfiles != null)
            {
                for (int i = 0; i < sceneBgmProfiles.Length; i++)
                {
                    SceneBgmProfile profile = sceneBgmProfiles[i];
                    if (profile == null || string.IsNullOrWhiteSpace(profile.sceneName) || profile.bgmClip == null)
                    {
                        continue;
                    }

                    sceneBgmLookup[profile.sceneName.Trim()] = profile;
                }
            }

            characterActionLookup.Clear();
            if (characterActionSfxProfiles == null) return;

            for (int i = 0; i < characterActionSfxProfiles.Length; i++)
            {
                CharacterActionSfxProfile profile = characterActionSfxProfiles[i];
                if (profile == null || profile.clips == null || profile.clips.Length == 0) continue;

                if (!characterActionLookup.TryGetValue(profile.action, out List<CharacterActionSfxProfile> actionProfiles))
                {
                    actionProfiles = new List<CharacterActionSfxProfile>();
                    characterActionLookup[profile.action] = actionProfiles;
                }

                actionProfiles.Add(profile);
            }
        }

        public void RegisterSceneMusic(string sceneName, AudioClip clip, float volumeScale = 1f, bool loop = true)
        {
            if (string.IsNullOrWhiteSpace(sceneName) || clip == null) return;

            runtimeSceneBgmLookup[sceneName.Trim()] = new SceneBgmProfile
            {
                sceneName = sceneName.Trim(),
                bgmClip = clip,
                volumeScale = Mathf.Clamp(volumeScale, 0f, 2f),
                loop = loop
            };
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == GameData.MainGameScene)
            {
                AudioClip selectedMapClip = GameData.selectedMap != null ? GameData.selectedMap.mapBgmClip : null;
                bool mapBgmAlreadyPlaying =
                    selectedMapClip != null &&
                    musicSource != null &&
                    musicSource.isPlaying &&
                    musicSource.clip == selectedMapClip;

                if (mapBgmAlreadyPlaying)
                {
                    return;
                }

                // MainGameScene uses map-specific BGM from GameManager.
                // Stop current scene/system BGM here to prevent overlap during transition.
                StopMusic();
                StopAllSFX();
                return;
            }

            PlaySceneBGM(scene.name);
        }

        public void PlaySceneBGM(string sceneName, bool immediate = false)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;

            EnsureAudioReady();

            SceneBgmProfile profile = null;
            if (!runtimeSceneBgmLookup.TryGetValue(sceneName, out profile))
            {
                sceneBgmLookup.TryGetValue(sceneName, out profile);
            }

            if (profile != null && profile.bgmClip != null)
            {
                PlayBGMClip(profile.bgmClip, profile.volumeScale, profile.loop, immediate);
            }
        }

        public void PlayBGMClip(AudioClip clip, float volumeScale = 1f, bool loop = true, bool immediate = false, float fadeDuration = 0.25f)
        {
            if (clip == null) return;
            EnsureAudioReady();

            float targetVolume = Mathf.Clamp01(musicBaseVolume * Mathf.Clamp(volumeScale, 0f, 2f));

            if (immediate || musicSource.clip == null || !musicSource.isPlaying)
            {
                if (musicFadeCoroutine != null)
                {
                    StopCoroutine(musicFadeCoroutine);
                    musicFadeCoroutine = null;
                }

                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.volume = targetVolume;
                musicSource.Play();
                return;
            }

            if (musicSource.clip == clip)
            {
                musicSource.loop = loop;
                musicSource.volume = targetVolume;
                return;
            }

            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }

            musicFadeCoroutine = StartCoroutine(CrossfadeMusicCoroutine(clip, targetVolume, loop, fadeDuration));
        }

        private IEnumerator CrossfadeMusicCoroutine(AudioClip nextClip, float targetVolume, bool loop, float fadeDuration)
        {
            if (musicSource == null)
            {
                yield break;
            }

            float safeDuration = Mathf.Max(0.01f, fadeDuration);
            float startVolume = musicSource.volume;
            float timer = 0f;

            while (timer < safeDuration)
            {
                timer += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / safeDuration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.clip = nextClip;
            musicSource.loop = loop;
            musicSource.Play();

            timer = 0f;
            while (timer < safeDuration)
            {
                timer += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(0f, targetVolume, timer / safeDuration);
                yield return null;
            }

            musicSource.volume = targetVolume;
            musicFadeCoroutine = null;
        }

        public void PlayBGM(int trackIndex)
        {
            EnsureAudioReady();

            if (bgmTracks == null || trackIndex >= bgmTracks.Length || trackIndex < 0)
                return;

            currentBGMIndex = trackIndex;

            PlayBGMClip(bgmTracks[trackIndex], 1f, true);
        }

        public void PlayRandomHitSound()
        {
            PlayClipOnSfxSource(GetRandomClip(hitSounds), 1f, 0.95f, 1.05f);
        }

        public void PlayRandomBlockSound()
        {
            PlayClipOnSfxSource(GetRandomClip(blockSounds), 1f, 0.95f, 1.05f);
        }

        public void PlayRandomDashSound()
        {
            PlayClipOnSfxSource(GetRandomClip(dashSounds), 1f, 0.95f, 1.08f);
        }

        public void PlayRandomDeathSound()
        {
            EnsureAudioReady();
            AudioClip clip = GetRandomClip(deathSounds);
            if (clip != null && voiceSource != null)
            {
                voiceSource.PlayOneShot(clip);
            }
        }

        public void PlayRandomVictorySound()
        {
            EnsureAudioReady();
            AudioClip clip = GetRandomClip(victorySounds);
            if (clip != null && voiceSource != null)
            {
                voiceSource.PlayOneShot(clip);
            }
        }

        public void PlayCharacterActionSfx(CharacterType characterType, CharacterAudioAction action, float volumeMultiplier = 1f)
        {
            EnsureAudioReady();

            if (characterActionLookup.TryGetValue(action, out List<CharacterActionSfxProfile> profiles))
            {
                for (int i = 0; i < profiles.Count; i++)
                {
                    CharacterActionSfxProfile profile = profiles[i];
                    if (profile.characterType != characterType) continue;

                    AudioClip clip = GetRandomClip(profile.clips);
                    if (clip == null) break;

                    float finalVolume = Mathf.Clamp01(profile.volumeScale * Mathf.Max(0f, volumeMultiplier));
                    float pitchMin = Mathf.Min(profile.pitchMin, profile.pitchMax);
                    float pitchMax = Mathf.Max(profile.pitchMin, profile.pitchMax);
                    PlayClipOnSfxSource(clip, finalVolume, pitchMin, pitchMax);
                    return;
                }
            }

            PlayLegacyFallbackAction(action, volumeMultiplier);
        }

        private void PlayLegacyFallbackAction(CharacterAudioAction action, float volumeMultiplier)
        {
            float finalVolume = Mathf.Clamp01(Mathf.Max(0f, volumeMultiplier));
            switch (action)
            {
                case CharacterAudioAction.DashStart:
                    PlayClipOnSfxSource(GetRandomClip(dashSounds), finalVolume, 0.95f, 1.08f);
                    break;
                case CharacterAudioAction.BlockStart:
                case CharacterAudioAction.BlockImpact:
                case CharacterAudioAction.Parry:
                    PlayClipOnSfxSource(GetRandomClip(blockSounds), finalVolume, 0.95f, 1.05f);
                    break;
                case CharacterAudioAction.Death:
                    PlayRandomDeathSound();
                    break;
                case CharacterAudioAction.Victory:
                    PlayRandomVictorySound();
                    break;
                case CharacterAudioAction.Hurt:
                case CharacterAudioAction.NormalAttack:
                case CharacterAudioAction.Skill1:
                case CharacterAudioAction.Skill2:
                case CharacterAudioAction.Skill3:
                case CharacterAudioAction.Jump:
                case CharacterAudioAction.Land:
                case CharacterAudioAction.Footstep:
                    PlayClipOnSfxSource(GetRandomClip(hitSounds), finalVolume, 0.95f, 1.05f);
                    break;
            }
        }

        public void UpdateBGMIntensity(float healthRatio)
        {
            EnsureAudioReady();
            currentHealthRatio = healthRatio;

            // Change BGM based on health ratio
            if (healthRatio > 0.7f)
            {
                PlayBGM(0); // Normal BGM
            }
            else if (healthRatio > 0.3f)
            {
                PlayBGM(1); // Medium intensity BGM
            }
            else
            {
                PlayBGM(2); // High intensity BGM
            }
            
            // Adjust music pitch based on intensity
            if (musicSource != null)
            {
                float targetPitch = Mathf.Lerp(0.8f, 1.2f, 1f - healthRatio);
                musicSource.pitch = Mathf.Lerp(musicSource.pitch, targetPitch, Time.deltaTime * 2f);
            }
        }

        public void SetMasterVolume(float volume, bool savePref = true)
        {
            masterVolume = Mathf.Clamp01(volume);
            AudioListener.volume = masterVolume;
            if (savePref)
            {
                PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
            }
        }

        public void SetMusicVolume(float volume, bool savePref = true)
        {
            EnsureAudioReady();
            float clampedVolume = Mathf.Clamp01(volume);
            musicBaseVolume = clampedVolume;

            if (musicSource != null)
            {
                musicSource.volume = clampedVolume;
            }

            if (audioMixer != null)
            {
                // Clamp volume to avoid log10(0) which returns -Infinity
                float safe = Mathf.Max(clampedVolume, 0.0001f);
                audioMixer.SetFloat("MusicVolume", Mathf.Log10(safe) * 20);
            }

            if (savePref)
            {
                PlayerPrefs.SetFloat(MusicVolumeKey, clampedVolume);
            }
        }

        public void SetSFXVolume(float volume, bool savePref = true)
        {
            EnsureAudioReady();
            float clampedVolume = Mathf.Clamp01(volume);
            sfxBaseVolume = clampedVolume;

            if (sfxSource != null)
            {
                sfxSource.volume = clampedVolume;
            }

            if (audioMixer != null)
            {
                float safe = Mathf.Max(clampedVolume, 0.0001f);
                audioMixer.SetFloat("SFXVolume", Mathf.Log10(safe) * 20);
            }

            if (savePref)
            {
                PlayerPrefs.SetFloat(SfxVolumeKey, clampedVolume);
            }
        }

        public void SetVoiceVolume(float volume, bool savePref = true)
        {
            EnsureAudioReady();
            float clampedVolume = Mathf.Clamp01(volume);
            voiceBaseVolume = clampedVolume;

            if (voiceSource != null)
            {
                voiceSource.volume = clampedVolume;
            }

            if (audioMixer != null)
            {
                float safe = Mathf.Max(clampedVolume, 0.0001f);
                audioMixer.SetFloat("VoiceVolume", Mathf.Log10(safe) * 20);
            }

            if (savePref)
            {
                PlayerPrefs.SetFloat(VoiceVolumeKey, clampedVolume);
            }
        }

        public void FadeOutMusic(float duration)
        {
            EnsureAudioReady();
            if (musicSource != null)
                StartCoroutine(FadeMusicCoroutine(musicSource.volume, 0f, duration));
        }

        public void FadeInMusic(float duration)
        {
            EnsureAudioReady();
            StartCoroutine(FadeMusicCoroutine(0f, musicBaseVolume, duration));
        }

        private IEnumerator FadeMusicCoroutine(float startVolume, float endVolume, float duration)
        {
            if (musicSource == null) yield break;

            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                if (musicSource == null) yield break;
                float currentVolume = Mathf.Lerp(startVolume, endVolume, elapsedTime / duration);
                musicSource.volume = currentVolume;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (musicSource != null) musicSource.volume = endVolume;
        }

        public void PlaySoundEffect(AudioClip clip, float volume = 1f)
        {
            PlayClipOnSfxSource(clip, Mathf.Clamp01(volume), 1f, 1f);
        }

        public void SetUIClickSound(AudioClip clip, bool overrideExisting = false)
        {
            if (clip == null) return;
            if (uiClickSound == null || overrideExisting)
            {
                uiClickSound = clip;
            }
        }

        public void PlayUIClick(float volumeMultiplier = 1f)
        {
            AudioClip clip = uiClickSound;
            if (clip == null)
            {
                clip = GetRandomClip(hitSounds) ?? GetRandomClip(blockSounds) ?? GetRandomClip(dashSounds);
            }

            if (clip == null) return;

            float pitchMin = Mathf.Min(uiClickPitchMin, uiClickPitchMax);
            float pitchMax = Mathf.Max(uiClickPitchMin, uiClickPitchMax);
            float finalVolume = Mathf.Clamp01(uiClickVolume * Mathf.Max(0f, volumeMultiplier));

            PlayClipOnSfxSource(clip, finalVolume, pitchMin, pitchMax);
        }

        public void PlayVoiceLine(AudioClip clip, float volume = 1f)
        {
            EnsureAudioReady();
            if (clip != null && voiceSource != null)
            {
                voiceSource.PlayOneShot(clip, volume);
            }
        }

        public void PauseMusic()
        {
            EnsureAudioReady();
            if (musicSource != null)
            {
                musicSource.Pause();
            }
        }

        public void ResumeMusic()
        {
            EnsureAudioReady();
            if (musicSource != null)
            {
                musicSource.UnPause();
            }
        }

        public void StopMusic()
        {
            EnsureAudioReady();

            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
                musicFadeCoroutine = null;
            }

            if (musicSource != null)
            {
                musicSource.Stop();
                musicSource.clip = null;
                musicSource.pitch = 1f;
            }
        }

        public void StopAllSFX()
        {
            EnsureAudioReady();
            if (sfxSource != null)
            {
                sfxSource.Stop();
            }
            
            if (voiceSource != null)
            {
                voiceSource.Stop();
            }
        }

        private void EnsureAudioReady()
        {
            if (!initialized)
            {
                InitializeAudio();
                RebuildLookups();
            }
            else
            {
                EnsureAudioSources();
            }
        }

        private AudioClip GetRandomClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return null;

            int randomIndex = UnityEngine.Random.Range(0, clips.Length);
            return clips[randomIndex];
        }

        private void PlayClipOnSfxSource(AudioClip clip, float volume, float pitchMin, float pitchMax)
        {
            EnsureAudioReady();
            if (clip == null || sfxSource == null) return;

            sfxSource.pitch = UnityEngine.Random.Range(pitchMin, pitchMax);
            sfxSource.PlayOneShot(clip, volume);
            sfxSource.pitch = 1f;
        }
    }
}
