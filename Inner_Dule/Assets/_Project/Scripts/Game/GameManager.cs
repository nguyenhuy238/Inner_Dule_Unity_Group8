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
        
        // Support for PlayerMovement2D
        private PlayerMovement2D playerMovement1;
        private PlayerMovement2D playerMovement2;
        private bool deathAnimationPlaying = false;
        private float deathAnimationTimer = 0f;
        private float deathAnimationDelay = 3f; // Wait 3 seconds for death animation
        
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
            Debug.Log("[InnerDuel] InitializeGame started.");

            // Dùng dữ liệu từ màn hình chọn tướng nếu có
            CharacterType p1Type = InnerDuel.UI.SelectionData.P1_Type;
            CharacterType p2Type = InnerDuel.UI.SelectionData.P2_Type;

            // Kiểm tra xem có player nào "đi lạc" trong scene không trước khi spawn
            if (player1 != null || player2 != null)
            {
                Debug.LogWarning($"[InnerDuel] Pre-existing players found in scene before spawning: P1: {player1 != null}, P2: {player2 != null}. These will be replaced by newly spawned characters.");
            }

            // Spawn Player 1
            Debug.Log($"[InnerDuel] Spawning P1: {p1Type}");
            GameObject p1Obj = CharacterFactory.Instance.CreateCharacter(p1Type, new Vector3(-5f, 0f, 0f), 1);
            if (p1Obj != null) 
            {
                player1 = p1Obj.GetComponent<InnerCharacterController>();
                playerMovement1 = p1Obj.GetComponent<PlayerMovement2D>();
                Debug.Log($"[InnerDuel] P1 spawned successfully. InnerController: {player1 != null}, PlayerMovement: {playerMovement1 != null}");
            }
            else 
            {
                Debug.LogError("[InnerDuel] Failed to spawn P1 via Factory!");
            }

            // Spawn Player 2
            Debug.Log($"[InnerDuel] Spawning P2: {p2Type}");
            p2Obj = CharacterFactory.Instance.CreateCharacter(p2Type, new Vector3(5f, 0f, 0f), 2);
            if (p2Obj != null)
            {
                player2 = p2Obj.GetComponent<InnerCharacterController>();
                playerMovement2 = p2Obj.GetComponent<PlayerMovement2D>();
                Debug.Log($"[InnerDuel] P2 spawned successfully. InnerController: {player2 != null}, PlayerMovement: {playerMovement2 != null}");
            }
            else
            {
                Debug.LogError("[InnerDuel] Failed to spawn P2 via Factory!");
            }

            // Setup character layers and basic settings
            SetCharacterLayers();
            
            // Setup opponent references
            if (player1 != null && player2 != null)
            {
                player1.opponentLayer = LayerMask.GetMask("Player2");
                player2.opponentLayer = LayerMask.GetMask("Player1");
            }
            
            // Get player GameObjects for camera (works with both controller types)
            if (p1Obj == null) p1Obj = player1?.gameObject;
            if (p2Obj == null) p2Obj = player2?.gameObject;
            
            // Fallback: find by PlayerMovement2D if InnerCharacterController not available
            if (p1Obj == null || p2Obj == null)
            {
                var allPlayers = GameObject.FindObjectsOfType<PlayerMovement2D>();
                foreach (var p in allPlayers)
                {
                    if (p.playerID == 1 && p1Obj == null) p1Obj = p.gameObject;
                    if (p.playerID == 2 && p2Obj == null) p2Obj = p.gameObject;
                }
            }
            
            // Link to Camera
            if (cameraController != null && p1Obj != null && p2Obj != null)
            {
                cameraController.SetTargets(p1Obj.transform, p2Obj.transform);
                Debug.Log($"[GameManager] Camera targets set: {p1Obj.name}, {p2Obj.name}");
            }
            else if (cameraController != null)
            {
                Debug.LogWarning("[GameManager] Camera could not be set - missing player objects");
            }
            
            // Link to UI (always call even if player1/player2 are null - UIManager will find PlayerMovement2D)
            if (uiManager != null)
            {
                uiManager.InitializeWithPlayers(player1, player2);
                Debug.Log("[GameManager] UIManager.InitializeWithPlayers() called");
            }
            else
            {
                Debug.LogError("[GameManager] UIManager is NULL! Cannot initialize health bars!");
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
            // Try to find PlayerMovement2D if not found yet
            if (playerMovement1 == null)
            {
                var players = GameObject.FindObjectsOfType<PlayerMovement2D>();
                foreach (var p in players)
                {
                    if (p.playerID == 1)
                    {
                        playerMovement1 = p;
                    }
                    else if (p.playerID == 2)
                    {
                        playerMovement2 = p;
                    }
                }
            }
            
            // Check for references - only warn if BOTH types are missing
            if (player1 == null && player2 == null && playerMovement1 == null && playerMovement2 == null)
            {
                // Try to recover players silently
                RecoverPlayers();
                // No warning - players might be using PlayerMovement2D
            }

            // Check for win conditions (support both types)
            bool player1Dead = false;
            bool player2Dead = false;
            
            // Check InnerCharacterController first
            if (player1 != null && player1.IsDead())
            {
                player1Dead = true;
            }
            else if (playerMovement1 != null && playerMovement1.IsDead())
            {
                player1Dead = true;
            }
            
            if (player2 != null && player2.IsDead())
            {
                player2Dead = true;
            }
            else if (playerMovement2 != null && playerMovement2.IsDead())
            {
                player2Dead = true;
            }
            
            // Handle death animation delay
            if ((player1Dead || player2Dead) && !deathAnimationPlaying)
            {
                deathAnimationPlaying = true;
                deathAnimationTimer = 0f;
                Debug.Log($"[GameManager] Player died! Waiting {deathAnimationDelay}s for death animation...");
            }
            
            if (deathAnimationPlaying)
            {
                deathAnimationTimer += Time.deltaTime;
                
                if (deathAnimationTimer >= deathAnimationDelay)
                {
                    // Death animation finished, determine winner
                    if (player1Dead)
                    {
                        winner = player2;
                        Debug.Log("[GameManager] Player 2 wins!");
                        StartEnding();
                    }
                    else if (player2Dead)
                    {
                        winner = player1;
                        Debug.Log("[GameManager] Player 1 wins!");
                        StartEnding();
                    }
                    
                    deathAnimationPlaying = false;
                }
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
            
            // Start ending sequence safely, even if winner is null or uses PlayerMovement2D
            if (cameraController != null)
            {
                Transform focus = null;
                
                // Prefer the explicit winner if available
                if (winner != null)
                {
                    focus = winner.transform;
                }
                else
                {
                    // Try to determine the surviving character
                    if (player1 != null && !player1.IsDead()) focus = player1.transform;
                    else if (player2 != null && !player2.IsDead()) focus = player2.transform;

                    // Fallback to PlayerMovement2D references if InnerCharacterController is not used
                    if (focus == null)
                    {
                        if (playerMovement1 != null && !playerMovement1.IsDead()) focus = playerMovement1.transform;
                        else if (playerMovement2 != null && !playerMovement2.IsDead()) focus = playerMovement2.transform;
                    }

                    // Last resort: use any available transform (even if dead), to avoid null
                    if (focus == null)
                    {
                        if (player1 != null) focus = player1.transform;
                        else if (player2 != null) focus = player2.transform;
                        else if (playerMovement1 != null) focus = playerMovement1.transform;
                        else if (playerMovement2 != null) focus = playerMovement2.transform;
                    }
                }

                cameraController.StartEndingSequence(focus);
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
                Debug.Log($"[InnerDuel] Diagnostic: Found potential player object '{controller.gameObject.name}' with PlayerID: {controller.playerID}");
                
                if (controller.playerID == 1 && player1 == null)
                {
                    player1 = controller;
                    foundAny = true;
                    Debug.Log($"[InnerDuel] Recovered Player 1 from object: {controller.gameObject.name}");
                }
                else if (controller.playerID == 2 && player2 == null)
                {
                    player2 = controller;
                    foundAny = true;
                    Debug.Log($"[InnerDuel] Recovered Player 2 from object: {controller.gameObject.name}");
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
