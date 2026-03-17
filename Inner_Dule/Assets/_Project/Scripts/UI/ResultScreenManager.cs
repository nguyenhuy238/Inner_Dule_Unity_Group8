using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using InnerDuel.Core;

namespace InnerDuel.UI
{
    public class ResultScreenManager : MonoBehaviour
    {
        [Header("UI Components")]
        public TextMeshProUGUI winnerText;
        public Image winnerPortrait;
        
        private void Start()
        {
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
        }

        public void Rematch()
        {
            SceneManager.LoadScene(GameData.LoadingScene);
        }

        public void CharacterSelect()
        {
            SceneManager.LoadScene(GameData.CharacterSelectScene);
        }

        public void MainMenu()
        {
            SceneManager.LoadScene(GameData.MainMenuScene);
        }
    }
}
