using UnityEngine;
using UnityEngine.Audio;
using InnerDuel.Core;

namespace InnerDuel.Audio
{
    public class AudioManager : Singleton<AudioManager>
    {
        
        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioSource voiceSource;
        
        [Header("Audio Clips")]
        public AudioClip[] bgmTracks;
        public AudioClip[] hitSounds;
        public AudioClip[] blockSounds;
        public AudioClip[] dashSounds;
        public AudioClip[] deathSounds;
        public AudioClip[] victorySounds;
        
        [Header("Audio Mixer")]
        public AudioMixer audioMixer;
        
        [Header("Settings")]
        public float musicBaseVolume = 0.5f;
        public float sfxBaseVolume = 0.7f;
        public float voiceBaseVolume = 0.8f;
        
        private float currentHealthRatio = 1f;
        private int currentBGMIndex = 0;
        
        protected override void Awake()
        {
            base.Awake();
        }
        
        private void Start()
        {
            InitializeAudio();
        }
        
        private void InitializeAudio()
        {
            // Create audio sources if not assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
            }
            
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
            }
            
            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.loop = false;
            }
            
            // Set initial volumes
            SetMusicVolume(musicBaseVolume);
            SetSFXVolume(sfxBaseVolume);
            SetVoiceVolume(voiceBaseVolume);
            
            // Start playing background music
            PlayBGM(0);
        }
        
        public void PlayBGM(int trackIndex)
        {
            if (bgmTracks == null || trackIndex >= bgmTracks.Length || trackIndex < 0)
                return;
            
            currentBGMIndex = trackIndex;
            
            if (musicSource != null && musicSource.clip != bgmTracks[trackIndex])
            {
                musicSource.clip = bgmTracks[trackIndex];
                musicSource.Play();
            }
        }
        
        public void PlayRandomHitSound()
        {
            if (hitSounds != null && hitSounds.Length > 0 && sfxSource != null)
            {
                AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
                sfxSource.PlayOneShot(clip);
            }
        }
        
        public void PlayRandomBlockSound()
        {
            if (blockSounds != null && blockSounds.Length > 0 && sfxSource != null)
            {
                AudioClip clip = blockSounds[Random.Range(0, blockSounds.Length)];
                sfxSource.PlayOneShot(clip);
            }
        }
        
        public void PlayRandomDashSound()
        {
            if (dashSounds != null && dashSounds.Length > 0 && sfxSource != null)
            {
                AudioClip clip = dashSounds[Random.Range(0, dashSounds.Length)];
                sfxSource.PlayOneShot(clip);
            }
        }
        
        public void PlayRandomDeathSound()
        {
            if (deathSounds != null && deathSounds.Length > 0 && voiceSource != null)
            {
                AudioClip clip = deathSounds[Random.Range(0, deathSounds.Length)];
                voiceSource.PlayOneShot(clip);
            }
        }
        
        public void PlayRandomVictorySound()
        {
            if (victorySounds != null && victorySounds.Length > 0 && voiceSource != null)
            {
                AudioClip clip = victorySounds[Random.Range(0, victorySounds.Length)];
                voiceSource.PlayOneShot(clip);
            }
        }
        
        public void UpdateBGMIntensity(float healthRatio)
        {
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
        
        public void SetMusicVolume(float volume)
        {
            if (musicSource != null)
            {
                musicSource.volume = volume;
            }
            
            if (audioMixer != null)
            {
                // Clamp volume to avoid log10(0) which returns -Infinity
                float clampedVolume = Mathf.Max(volume, 0.0001f);
                audioMixer.SetFloat("MusicVolume", Mathf.Log10(clampedVolume) * 20);
            }
        }
        
        public void SetSFXVolume(float volume)
        {
            if (sfxSource != null)
            {
                sfxSource.volume = volume;
            }
            
            if (audioMixer != null)
            {
                float clampedVolume = Mathf.Max(volume, 0.0001f);
                audioMixer.SetFloat("SFXVolume", Mathf.Log10(clampedVolume) * 20);
            }
        }
        
        public void SetVoiceVolume(float volume)
        {
            if (voiceSource != null)
            {
                voiceSource.volume = volume;
            }
            
            if (audioMixer != null)
            {
                float clampedVolume = Mathf.Max(volume, 0.0001f);
                audioMixer.SetFloat("VoiceVolume", Mathf.Log10(clampedVolume) * 20);
            }
        }
        
        public void FadeOutMusic(float duration)
        {
            if (musicSource != null)
                StartCoroutine(FadeMusicCoroutine(musicSource.volume, 0f, duration));
        }
        
        public void FadeInMusic(float duration)
        {
            StartCoroutine(FadeMusicCoroutine(0f, musicBaseVolume, duration));
        }
        
        private System.Collections.IEnumerator FadeMusicCoroutine(float startVolume, float endVolume, float duration)
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
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }
        
        public void PlayVoiceLine(AudioClip clip, float volume = 1f)
        {
            if (clip != null && voiceSource != null)
            {
                voiceSource.PlayOneShot(clip, volume);
            }
        }
        
        public void PauseMusic()
        {
            if (musicSource != null)
            {
                musicSource.Pause();
            }
        }
        
        public void ResumeMusic()
        {
            if (musicSource != null)
            {
                musicSource.UnPause();
            }
        }
        
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }
        
        public void StopAllSFX()
        {
            if (sfxSource != null)
            {
                sfxSource.Stop();
            }
            
            if (voiceSource != null)
            {
                voiceSource.Stop();
            }
        }
    }
}
