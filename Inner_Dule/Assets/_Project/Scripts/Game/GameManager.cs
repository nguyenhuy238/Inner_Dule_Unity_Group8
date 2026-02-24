using UnityEngine;
using UnityEngine.SceneManagement;
using InnerDuel.Characters;
using InnerDuel.UI;
using InnerDuel.Camera;
using InnerDuel.Core;

namespace InnerDuel
{
    public class GameManager : Singleton<GameManager>
    {
        [Header("Game State")]
        public GameState currentState = GameState.Intro;
        
        [Header("Characters")]
        public InnerCharacterController player1;
        public InnerCharacterController player2;
        
        [Header("UI")]
        public UIManager uiManager;
        public CameraController cameraController;
        
        [Header("Game Settings")]
        public float introDuration = 3f;
        public float endingDuration = 5f;
        
        private float stateTimer = 0f;
        private InnerCharacterController winner;
        
        public enum GameState
        {
            Intro,
            Gameplay,
            Ending,
            Menu
        }
        
        protected override void Awake()
        {
            base.Awake();
            // Additional initialization if needed
        }
        
        private void Start()
        {
            InitializeGame();
        }
        
        private void Update()
        {
            switch (currentState)
            {
                case GameState.Intro:
                    HandleIntroState();
                    break;
                case GameState.Gameplay:
                    HandleGameplayState();
                    break;
                case GameState.Ending:
                    HandleEndingState();
                    break;
            }
        }
        
        private void InitializeGame()
        {
            // Try to recover players if they are missing
            if (player1 == null || player2 == null)
            {
                RecoverPlayers();
            }

            // Still missing? Spawn defaults from Factory
            if (player1 == null)
            {
                Debug.Log("[InnerDuel] Auto-spawning P1: Discipline");
                GameObject p1Obj = CharacterFactory.Instance.CreateCharacter(CharacterType.Discipline, new Vector3(-5f, 0f, 0f), 1);
                if (p1Obj != null) player1 = p1Obj.GetComponent<InnerCharacterController>();
            }

            if (player2 == null)
            {
                Debug.Log("[InnerDuel] Auto-spawning P2: Spontaneity");
                GameObject p2Obj = CharacterFactory.Instance.CreateCharacter(CharacterType.Spontaneity, new Vector3(5f, 0f, 0f), 2);
                if (p2Obj != null) player2 = p2Obj.GetComponent<InnerCharacterController>();
            }

            // Setup character layers and basic settings
            SetCharacterLayers();
            
            // Setup opponent references
            if (player1 != null && player2 != null)
            {
                player1.opponentLayer = LayerMask.GetMask("Player2");
                player2.opponentLayer = LayerMask.GetMask("Player1");
                
                // Link to Camera and UI
                if (cameraController != null) cameraController.SetTargets(player1.transform, player2.transform);
                if (uiManager != null) uiManager.InitializeWithPlayers(player1, player2);
            }
            
            // Start with intro
            StartIntro();
        }
        
        private void SetCharacterLayers()
        {
            if (player1 != null)
            {
                player1.gameObject.layer = LayerMask.NameToLayer("Player1");
                player1.playerID = 1;
            }
            
            if (player2 != null)
            {
                player2.gameObject.layer = LayerMask.NameToLayer("Player2");
                player2.playerID = 2;
            }
        }
        
        private void HandleIntroState()
        {
            stateTimer += Time.deltaTime;
            
            // Show intro text and animations
            if (uiManager != null)
            {
                uiManager.ShowIntroText();
            }
            
            // Transition to gameplay after intro duration
            if (stateTimer >= introDuration)
            {
                StartGameplay();
            }
        }
        
        private void HandleGameplayState()
        {
            // Check for references
            if (player1 == null || player2 == null)
            {
                if (!RecoverPlayers())
                {
                    Debug.LogWarning("[InnerDuel] Players not found in Gameplay state. No players discovered in scene.");
                    return;
                }
            }

            // Check for win conditions
            if (player1.IsDead())
            {
                winner = player2;
                StartEnding();
            }
            else if (player2.IsDead())
            {
                winner = player1;
                StartEnding();
            }
            
            // Update UI
            if (uiManager != null)
            {
                uiManager.UpdateGameplayUI();
            }
        }
        
        private void HandleEndingState()
        {
            stateTimer += Time.deltaTime;
            
            // Show ending sequence
            if (uiManager != null)
            {
                uiManager.ShowEndingSequence(winner);
            }
            
            // Return to menu after ending duration
            if (stateTimer >= endingDuration)
            {
                ReturnToMenu();
            }
        }
        
        public void StartIntro()
        {
            currentState = GameState.Intro;
            stateTimer = 0f;
            
            // Disable character controls during intro
            if (player1 != null) player1.enabled = false;
            if (player2 != null) player2.enabled = false;
            
            // Position characters for intro
            PositionCharactersForIntro();
        }
        
        public void StartGameplay()
        {
            currentState = GameState.Gameplay;
            stateTimer = 0f;
            
            // Enable character controls
            if (player1 != null) player1.enabled = true;
            if (player2 != null) player2.enabled = true;
            
            // Link players to Camera and UI automatically
            if (cameraController != null && player1 != null && player2 != null)
            {
                cameraController.SetTargets(player1.transform, player2.transform);
            }

            if (uiManager != null)
            {
                uiManager.HideIntroText();
                uiManager.ShowGameplayUI();
                uiManager.InitializeWithPlayers(player1, player2);
            }
        }
        
        public void StartEnding()
        {
            currentState = GameState.Ending;
            stateTimer = 0f;
            
            // Disable character controls
            if (player1 != null) player1.enabled = false;
            if (player2 != null) player2.enabled = false;
            
            // Start ending sequence
            if (cameraController != null)
            {
                cameraController.StartEndingSequence(winner.transform);
            }
        }
        
        public void ReturnToMenu()
        {
            currentState = GameState.Menu;
            
            // Load menu scene or restart
            SceneManager.LoadScene(0);
        }
        
        private void PositionCharactersForIntro()
        {
            if (player1 != null && player2 != null)
            {
                // Position characters on opposite sides
                player1.transform.position = new Vector3(-5f, 0f, 0f);
                player2.transform.position = new Vector3(5f, 0f, 0f);
                
                // Make them face each other
                var player1Sprite = player1.GetComponent<SpriteRenderer>();
                var player2Sprite = player2.GetComponent<SpriteRenderer>();
                
                if (player1Sprite != null) player1Sprite.flipX = false;
                if (player2Sprite != null) player2Sprite.flipX = true;
            }
        }
        
        public void OnCharacterDied(InnerCharacterController character)
        {
            // This method is called by InnerCharacterController when a character dies
            // The actual handling is done in HandleGameplayState
        }
        
        public void RestartGame()
        {
            // Reload current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        public void PauseGame()
        {
            Time.timeScale = 0f;
        }
        
        public void ResumeGame()
        {
            Time.timeScale = 1f;
        }

        private bool RecoverPlayers()
        {
            var controllers = FindObjectsOfType<InnerCharacterController>();
            bool foundAny = false;

            foreach (var controller in controllers)
            {
                if (controller.playerID == 1 && player1 == null)
                {
                    player1 = controller;
                    foundAny = true;
                }
                else if (controller.playerID == 2 && player2 == null)
                {
                    player2 = controller;
                    foundAny = true;
                }
            }

            if (foundAny)
            {
                Debug.Log($"[InnerDuel] Recovered players: P1: {player1 != null}, P2: {player2 != null}");
                
                // Re-initialize if both found
                if (player1 != null && player2 != null)
                {
                    // InitializeGame(); // Avoid recursion, InitializeGame will handle the rest
                }
            }

            return player1 != null && player2 != null;
        }
    }
}
