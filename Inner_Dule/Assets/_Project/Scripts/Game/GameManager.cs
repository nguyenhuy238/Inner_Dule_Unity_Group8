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

        [Header("Map")]
        public Transform mapParent;
        public MapData fallbackMap;
        
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

            // 1. Spawn Map
            SpawnMap();

            // 2. Setup Players
            // Prefer using players already placed in the scene
            if (player1 == null || player2 == null)
            {
                RecoverPlayers();
            }

            // Apply Character Data from Character Select if available
            if (GameData.player1Character != null && player1 != null)
            {
                player1.characterData = GameData.player1Character;
            }
            if (GameData.player2Character != null && player2 != null)
            {
                player2.characterData = GameData.player2Character;
            }

            // Setup players
            if (player1 != null)
            {
                player1.playerID = 1;
                player1.gameObject.layer = LayerMask.NameToLayer("Player1");
                player1.opponentLayer = LayerMask.GetMask("Player2");
                player1.InitializeFromData();
            }
            
            if (player2 != null)
            {
                player2.playerID = 2;
                player2.gameObject.layer = LayerMask.NameToLayer("Player2");
                player2.opponentLayer = LayerMask.GetMask("Player1");
                player2.InitializeFromData();
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

        private void SpawnMap()
        {
            MapData mapToSpawn = GameData.selectedMap != null ? GameData.selectedMap : fallbackMap;

            if (mapToSpawn != null && mapToSpawn.mapPrefab != null)
            {
                Debug.Log($"[InnerDuel] Spawning map: {mapToSpawn.mapName}");
                GameObject mapInstance = Instantiate(mapToSpawn.mapPrefab, Vector3.zero, Quaternion.identity);
                if (mapParent != null) mapInstance.transform.SetParent(mapParent);
            }
            else
            {
                Debug.LogWarning("[InnerDuel] No map selected and no fallback map assigned!");
            }
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
                GameData.winnerPlayerID = 2;
                GameData.winnerName = player2.characterData != null ? player2.characterData.characterName : "P2";
                Debug.Log("Player 1 Died! Winner: Player 2");
            }
            else
            {
                winner = player1;
                GameData.winnerPlayerID = 1;
                GameData.winnerName = player1.characterData != null ? player1.characterData.characterName : "P1";
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
            SceneManager.LoadScene(GameData.ResultScene);
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
