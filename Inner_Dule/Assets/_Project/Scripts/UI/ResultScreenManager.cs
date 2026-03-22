using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using InnerDuel.Core;
using InnerDuel.Audio;

namespace InnerDuel.UI
{
    public class ResultScreenManager : MonoBehaviour
    {
        [Header("UI Components")]
        public TextMeshProUGUI winnerText;
        public Image winnerPortrait;
        
        [Header("Audio")]
        public AudioClip sceneMusic;

        private void Start()
        {
            SetupSceneAudio();
            if (winnerText)
            {
                winnerText.text = GameData.winnerPlayerID != 0 ? 
                    $"PLAYER {GameData.winnerPlayerID} WINS!" : 
                    "DRAW!";
                
                if (!string.IsNullOrEmpty(GameData.winnerName))
                {
                    winnerText.text += $"\n({GameData.winnerName})";
                }
            }

            if (winnerPortrait)
            {
                Sprite portrait = GameData.winnerPortrait;

                if (portrait == null)
                {
                    if (GameData.winnerPlayerID == 1 && GameData.player1Character != null)
                    {
                        portrait = GameData.player1Character.portrait;
                    }
                    else if (GameData.winnerPlayerID == 2 && GameData.player2Character != null)
                    {
                        portrait = GameData.player2Character.portrait;
                    }
                }

                winnerPortrait.sprite = portrait;
                winnerPortrait.enabled = portrait != null;
            }
        }

        private void SetupSceneAudio()
        {
            if (AudioManager.Instance == null || sceneMusic == null) return;

            string sceneName = SceneManager.GetActiveScene().name;
            AudioManager.Instance.RegisterSceneMusic(sceneName, sceneMusic);
            AudioManager.Instance.PlaySceneBGM(sceneName);
        }

        public void Rematch()
        {
            PlayUIClick();
            SceneManager.LoadScene(GameData.LoadingScene);
        }

        public void CharacterSelect()
        {
            PlayUIClick();
            SceneManager.LoadScene(GameData.CharacterSelectScene);
        }

        public void MainMenu()
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
