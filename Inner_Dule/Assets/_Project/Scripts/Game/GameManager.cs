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
        
        // Death Handling
        private bool deathSequenceStarted = false;
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

            // Prefer using players already placed in the scene
            if (player1 == null || player2 == null)
            {
                RecoverPlayers();
            }

            // Spawn if missing (Factory fallback)
            if (player1 == null || player2 == null)
            {
                 // TODO: Spawn Logic if needed, for now assume scene placement or basic recovery
                 // If you have a character selection screen, you'd pass data here.
                 // For now, let's assume players are placed in the scene or RecoverPlayers found them.
                 Debug.LogWarning("[InnerDuel] Players not assigned in Inspector and Recover failed. Spawning defaults not implemented yet.");
            }

            // Setup players
            if (player1 != null)
            {
                player1.playerID = 1;
                player1.gameObject.layer = LayerMask.NameToLayer("Player1");
                player1.opponentLayer = LayerMask.GetMask("Player2");
            }
            
            if (player2 != null)
            {
                player2.playerID = 2;
                player2.gameObject.layer = LayerMask.NameToLayer("Player2");
                player2.opponentLayer = LayerMask.GetMask("Player1");
            }

            // Link Camera
            if (cameraController != null)
            {
                if (player1 != null && player2 != null)
                {
                    cameraController.SetTargets(player1.transform, player2.transform);
                }
            }

            // Link UI
            if (uiManager != null)
            {
                uiManager.InitializeWithPlayers(player1, player2);
            }

            StartIntro();
        }
        
        private void HandleIntroState()
        {
            stateTimer += Time.deltaTime;
            
            if (uiManager != null) uiManager.ShowIntroText();
            
            if (stateTimer >= introDuration)
            {
                StartGameplay();
            }
        }
        
        private void HandleGameplayState()
        {
            // Check win condition (handled via OnCharacterDied callback mostly, but safety check here)
            if (!deathSequenceStarted)
            {
                if (player1 != null && player1.IsDead()) OnCharacterDied(player1);
                if (player2 != null && player2.IsDead()) OnCharacterDied(player2);
            }
            
            if (deathSequenceStarted)
            {
                deathAnimationTimer += Time.deltaTime;
                if (deathAnimationTimer >= deathAnimationDelay)
                {
                    StartEnding();
                }
            }
            
            if (uiManager != null) uiManager.UpdateGameplayUI();
        }
        
        private void HandleEndingState()
        {
            stateTimer += Time.deltaTime;
            
            if (uiManager != null) uiManager.ShowEndingSequence(winner);
            
            if (stateTimer >= endingDuration)
            {
                ReturnToMenu();
            }
        }
        
        public void StartIntro()
        {
            currentState = GameState.Intro;
            stateTimer = 0f;
            
            if (player1 != null) player1.enabled = false;
            if (player2 != null) player2.enabled = false;
            
            // Initial Positions
            if (player1 != null) player1.transform.position = new Vector3(-5f, 0f, 0f);
            if (player2 != null) player2.transform.position = new Vector3(5f, 0f, 0f);
        }
        
        public void StartGameplay()
        {
            currentState = GameState.Gameplay;
            stateTimer = 0f;
            
            if (player1 != null) player1.enabled = true;
            if (player2 != null) player2.enabled = true;
            
            if (uiManager != null)
            {
                uiManager.HideIntroText();
                uiManager.ShowGameplayUI();
            }
        }
        
        public void OnCharacterDied(InnerCharacterController character)
        {
            if (deathSequenceStarted) return;
            
            deathSequenceStarted = true;
            deathAnimationTimer = 0f;
            
            if (character == player1)
            {
                winner = player2;
                Debug.Log("Player 1 Died! Winner: Player 2");
            }
            else
            {
                winner = player1;
                Debug.Log("Player 2 Died! Winner: Player 1");
            }
        }
        
        public void StartEnding()
        {
            currentState = GameState.Ending;
            stateTimer = 0f;
            
            if (player1 != null) player1.enabled = false;
            if (player2 != null) player2.enabled = false;
            
            if (cameraController != null && winner != null)
            {
                cameraController.StartEndingSequence(winner.transform);
            }
        }
        
        public void ReturnToMenu()
        {
            currentState = GameState.Menu;
            SceneManager.LoadScene(0); // Assuming Menu is index 0
        }
        
        private void RecoverPlayers()
        {
            var controllers = FindObjectsOfType<InnerCharacterController>();
            foreach (var c in controllers)
            {
                if (c.playerID == 1 && player1 == null) player1 = c;
                if (c.playerID == 2 && player2 == null) player2 = c;
            }
            
            // Fallback by position if IDs not set
            if (player1 == null && player2 == null && controllers.Length >= 2)
            {
                // Sort by X
                System.Array.Sort(controllers, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
                player1 = controllers[0];
                player2 = controllers[1];
            }
        }
    }
}
