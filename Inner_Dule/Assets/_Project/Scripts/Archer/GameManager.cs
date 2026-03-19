using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject winPanel;
    public Text winText; // Nếu dùng TextMeshPro thì đổi thành public TMPro.TMP_Text winText;

    void Start()
    {
        Time.timeScale = 1f; // Ép tốc độ game về bình thường mỗi khi load lại game
        winPanel.SetActive(false);
    }

    public void ShowWinScreen(string winnerName, Color winnerColor)
    {
        winPanel.SetActive(true);
        winText.text = winnerName + " Wins!";
        winText.color = winnerColor;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Phải trả lại thời gian bình thường trước khi load scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}