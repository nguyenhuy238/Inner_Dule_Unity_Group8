using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using InnerDuel.Core;
using InnerDuel.Audio;

namespace InnerDuel.UI
{
    public class LoadingSceneManager : MonoBehaviour
    {
        [Header("UI Components")]
        public Image progressBar;
        public TextMeshProUGUI progressText;
        public TextMeshProUGUI loadingTip;

        [Header("Audio")]
        public AudioClip sceneMusic;
        
        public string[] tips = {
            "Tip: Block to reduce damage from heavy attacks.",
            "Tip: Each character has unique special abilities.",
            "Tip: Watch your opponent's cooldowns!",
            "Tip: Use your dash to escape tricky situations."
        };

        private void Start()
        {
            SetupSceneAudio();
            if (loadingTip) loadingTip.text = tips[Random.Range(0, tips.Length)];
            StartCoroutine(LoadSceneAsync());
        }

        private void SetupSceneAudio()
        {
            if (AudioManager.Instance == null || sceneMusic == null) return;

            string sceneName = SceneManager.GetActiveScene().name;
            AudioManager.Instance.RegisterSceneMusic(sceneName, sceneMusic);
            AudioManager.Instance.PlaySceneBGM(sceneName);
        }

        IEnumerator LoadSceneAsync()
        {
            yield return new WaitForSeconds(1f); // Brief delay for visual effect

            AsyncOperation operation = SceneManager.LoadSceneAsync(GameData.MainGameScene);
            
            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                if (progressBar) progressBar.fillAmount = progress;
                if (progressText) progressText.text = (progress * 100f).ToString("F0") + "%";
                
                yield return null;
            }
        }
    }
}
