using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthSlider;
    public Animator animator;
    private GameManager gameManager;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthSlider != null) healthSlider.value = maxHealth;
        gameManager = FindObjectOfType<GameManager>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (animator.GetBool("isDefending"))
        {
            damage /= 4; // Giảm sát thương khi thủ
            Debug.Log(gameObject.name + " đang đỡ đòn!");
        }

        currentHealth -= damage;
        if (healthSlider != null) healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            animator.SetTrigger("Hurt");
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetBool("isDead", true);
        GetComponent<PlayerMovement>().enabled = false;

        string winnerName = (gameObject.name == "CamXuc") ? "LyTri " : "CamXuc";

        var cam = FindObjectOfType<InnerDuel.Camera.CameraController>();
        if (cam != null && cam.virtualCamera != null)
        {
            GameObject winnerObj = GameObject.Find(winnerName);
            if (winnerObj != null)
            {
                // 1. NGẮT hoàn toàn Target Group để hết bị nghiêng/lệch
                cam.virtualCamera.LookAt = null;

                // 2. Ép Camera chỉ bám theo người thắng
                cam.virtualCamera.Follow = winnerObj.transform;

                // 3. QUAN TRỌNG: Đổi Transposer thành Framing Transposer bằng code lúc thắng
                // để nó có thể thực hiện lệnh Zoom (Orthographic Size)
                var component = cam.virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
                if (component != null)
                {
                    // Tắt bám đuổi kiểu cũ để lệnh Zoom của CameraController có tác dụng
                    cam.virtualCamera.m_Lens.OrthographicSize = 5f;
                }

                cam.StartEndingSequence(winnerObj.transform);
            }
        }

        if (gameManager != null)
        {
            gameManager.ShowWinScreen(winnerName, (winnerName == "LyTri ") ? Color.magenta : Color.cyan);
        }
    }
}