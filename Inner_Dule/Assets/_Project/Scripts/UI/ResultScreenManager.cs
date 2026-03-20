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
