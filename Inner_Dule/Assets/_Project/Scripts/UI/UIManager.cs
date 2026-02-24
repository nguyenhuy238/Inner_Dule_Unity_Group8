using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InnerDuel;
using InnerDuel.Characters;

namespace InnerDuel.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Intro UI")]
        public GameObject introPanel;
        public TextMeshProUGUI introText;
        public string[] introQuotes = {
            "Hôm nay là một ngày dài... tâm trí tôi không thể yên định.",
            "Cuộc chiến nội tâm không phải để tiêu diệt, mà để tìm thấy sự cân bằng.",
            "Giữa hai thái cực, đâu là con đường dẫn đến hòa hợp?"
        };
        
        [Header("Gameplay UI")]
        public GameObject gameplayPanel;
        public HealthBar player1HealthBar;
        public HealthBar player2HealthBar;
        public TextMeshProUGUI player1Name;
        public TextMeshProUGUI player2Name;
        
        [Header("Ending UI")]
        public GameObject endingPanel;
        public TextMeshProUGUI endingText;
        public TextMeshProUGUI harmonyText;
        public string[] harmonyQuotes = {
            "Sự cân bằng không phải là đích đến, đó là một hành trình.",
            "Chiến thắng bản thân là chiến thắng hiển hách nhất.",
            "Hòa hợp không phải là sự giống nhau, mà là sự bổ sung cho nhau.",
            "Từ đối đầu đến thấu hiểu, đó chính là sự trưởng thành."
        };
        
        [Header("Effects")]
        public Animator screenFadeAnimator;
        public ParticleSystem harmonyParticles;
        
        private InnerCharacterController player1;
        private InnerCharacterController player2;
        
        private void Start()
        {
            InitializeUI();
            HideAllPanels();
        }
        
        public void InitializeWithPlayers(InnerCharacterController p1, InnerCharacterController p2)
        {
            player1 = p1;
            player2 = p2;
            
            // Setup character names
            if (player1 != null && player1Name != null)
            {
                player1Name.text = player1.characterData.characterName;
            }
            
            if (player2 != null && player2Name != null)
            {
                player2Name.text = player2.characterData.characterName;
            }
            
            // Setup health bars
            if (player1 != null && player1HealthBar != null)
            {
                player1HealthBar.SetMaxHealth(player1.characterData.maxHealth);
                player1.healthBar = player1HealthBar;
            }
            
            if (player2 != null && player2HealthBar != null)
            {
                player2HealthBar.SetMaxHealth(player2.characterData.maxHealth);
                player2.healthBar = player2HealthBar;
            }
        }
        
        private void InitializeUI()
        {
            // Initial setup - will be reinforced by InitializeWithPlayers call
        }
        
        private void HideAllPanels()
        {
            if (introPanel != null) introPanel.SetActive(false);
            if (gameplayPanel != null) gameplayPanel.SetActive(false);
            if (endingPanel != null) endingPanel.SetActive(false);
        }
        
        public void ShowIntroText()
        {
            HideAllPanels();
            
            if (introPanel != null)
            {
                introPanel.SetActive(true);
                
                if (introText != null)
                {
                    string randomQuote = introQuotes[Random.Range(0, introQuotes.Length)];
                    introText.text = randomQuote;
                    
                    // Start typewriter effect
                    StartCoroutine(TypewriterEffect(introText, randomQuote));
                }
            }
        }
        
        public void HideIntroText()
        {
            if (introPanel != null)
            {
                introPanel.SetActive(false);
            }
        }
        
        public void ShowGameplayUI()
        {
            HideAllPanels();
            
            if (gameplayPanel != null)
            {
                gameplayPanel.SetActive(true);
            }
        }
        
        public void UpdateGameplayUI()
        {
            // Update health bars are handled by HealthBar components
            // Additional UI updates can be added here
        }
        
        public void ShowEndingSequence(InnerCharacterController winner)
        {
            HideAllPanels();
            
            if (endingPanel != null)
            {
                endingPanel.SetActive(true);
                
                if (endingText != null)
                {
                    endingText.text = "HARMONY ACHIEVED";
                }
                
                if (harmonyText != null)
                {
                    string randomQuote = harmonyQuotes[Random.Range(0, harmonyQuotes.Length)];
                    harmonyText.text = randomQuote;
                    
                    // Show quote after a delay
                    Invoke(nameof(ShowHarmonyQuote), 2f);
                }
            }
            
            // Play screen fade effect
            if (screenFadeAnimator != null)
            {
                screenFadeAnimator.SetTrigger("FadeToBlack");
            }
            
            // Play harmony particles
            if (harmonyParticles != null)
            {
                harmonyParticles.Play();
            }
        }
        
        private void ShowHarmonyQuote()
        {
            if (harmonyText != null)
            {
                // Fade in the harmony quote
                CanvasGroup canvasGroup = harmonyText.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = harmonyText.gameObject.AddComponent<CanvasGroup>();
                }
                
                StartCoroutine(FadeText(canvasGroup, 0f, 1f, 1f));
            }
        }
        
        private System.Collections.IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string text)
        {
            textComponent.text = "";
            
            foreach (char c in text)
            {
                textComponent.text += c;
                yield return new WaitForSeconds(0.05f);
            }
        }
        
        private System.Collections.IEnumerator FadeText(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            canvasGroup.alpha = endAlpha;
        }
        
        public void ShowPauseMenu()
        {
            // Implementation for pause menu
        }
        
        public void HidePauseMenu()
        {
            // Implementation for hiding pause menu
        }
    }
}
