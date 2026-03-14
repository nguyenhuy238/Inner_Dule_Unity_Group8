using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using InnerDuel.Core;

namespace InnerDuel.Input
{
    public class InputManager : Singleton<InputManager>
    {
        public static InputManager InstanceSafe()
        {
            // Return existing instance if present in the scene; avoid auto-creating one
            return (InputManager)Object.FindObjectOfType(typeof(InputManager));
        }
        
        private PlayerInput playerInput;
        private InputAction moveAction1, moveAction2;
        private InputAction jumpAction1, jumpAction2;
        private InputAction attack1Action1, attack1Action2;
        private InputAction attack2Action1, attack2Action2;
        private InputAction attack3Action1, attack3Action2;
        private InputAction blockAction1, blockAction2;
        private InputAction dashAction1, dashAction2;
        
        public Vector2 MoveInput1 { get; private set; }
        public Vector2 MoveInput2 { get; private set; }
        public bool JumpPressed1 { get; private set; }
        public bool JumpPressed2 { get; private set; }
        public bool Attack1Pressed1 { get; private set; }
        public bool Attack1Pressed2 { get; private set; }
        public bool Attack2Pressed1 { get; private set; }
        public bool Attack2Pressed2 { get; private set; }
        public bool Attack3Pressed1 { get; private set; }
        public bool Attack3Pressed2 { get; private set; }
        public bool BlockPressed1 { get; private set; }
        public bool BlockPressed2 { get; private set; }
        public bool DashPressed1 { get; private set; }
        public bool DashPressed2 { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
            InitializeInput();
        }
        
        private void InitializeInput()
        {
            playerInput = GetComponent<PlayerInput>();
            
            // Create InputActions instance correctly for ScriptableObjects
            var inputActions = ScriptableObject.CreateInstance<InputActions>();
            inputActions.Initialize();
            
            // Get input actions from the created instance
            moveAction1 = inputActions.Player1.FindAction("Move");
            moveAction2 = inputActions.Player2.FindAction("Move");
            
            jumpAction1 = inputActions.Player1.FindAction("Jump");
            jumpAction2 = inputActions.Player2.FindAction("Jump");
            
            blockAction1 = inputActions.Player1.FindAction("Block");
            blockAction2 = inputActions.Player2.FindAction("Block");
            
            dashAction1 = inputActions.Player1.FindAction("Dash");
            dashAction2 = inputActions.Player2.FindAction("Dash");
            
            attack1Action1 = inputActions.Player1.FindAction("Attack1");
            attack1Action2 = inputActions.Player2.FindAction("Attack1");
            
            attack2Action1 = inputActions.Player1.FindAction("Attack2");
            attack2Action2 = inputActions.Player2.FindAction("Attack2");
            
            attack3Action1 = inputActions.Player1.FindAction("Attack3");
            attack3Action2 = inputActions.Player2.FindAction("Attack3");
            
            // Enable input actions
            moveAction1?.Enable();
            moveAction2?.Enable();
            jumpAction1?.Enable();
            jumpAction2?.Enable();
            blockAction1?.Enable();
            blockAction2?.Enable();
            dashAction1?.Enable();
            dashAction2?.Enable();
            attack1Action1?.Enable();
            attack1Action2?.Enable();
            attack2Action1?.Enable();
            attack2Action2?.Enable();
            attack3Action1?.Enable();
            attack3Action2?.Enable();
        }
        
        private void Update()
        {
            // Read input values
            MoveInput1 = moveAction1?.ReadValue<Vector2>() ?? Vector2.zero;
            MoveInput2 = moveAction2?.ReadValue<Vector2>() ?? Vector2.zero;
            
            JumpPressed1 = jumpAction1?.WasPressedThisFrame() ?? false;
            JumpPressed2 = jumpAction2?.WasPressedThisFrame() ?? false;
            
            Attack1Pressed1 = attack1Action1?.WasPressedThisFrame() ?? false;
            Attack1Pressed2 = attack1Action2?.WasPressedThisFrame() ?? false;
            
            Attack2Pressed1 = attack2Action1?.WasPressedThisFrame() ?? false;
            Attack2Pressed2 = attack2Action2?.WasPressedThisFrame() ?? false;

            Attack3Pressed1 = attack3Action1?.WasPressedThisFrame() ?? false;
            Attack3Pressed2 = attack3Action2?.WasPressedThisFrame() ?? false;
            
            BlockPressed1 = blockAction1?.IsPressed() ?? false;
            BlockPressed2 = blockAction2?.IsPressed() ?? false;
            
            DashPressed1 = dashAction1?.WasPressedThisFrame() ?? false;
            DashPressed2 = dashAction2?.WasPressedThisFrame() ?? false;
        }
        
        public bool GetButtonDown(int playerID, string actionName)
        {
            switch (playerID)
            {
                case 1:
                    switch (actionName)
                    {
                        case "Jump": return JumpPressed1;
                        case "Attack1": return Attack1Pressed1;
                        case "Attack2": return Attack2Pressed1;
                        case "Attack3": return Attack3Pressed1;
                        case "Dash": return DashPressed1;
                        default: return false;
                    }
                case 2:
                    switch (actionName)
                    {
                        case "Jump": return JumpPressed2;
                        case "Attack1": return Attack1Pressed2;
                        case "Attack2": return Attack2Pressed2;
                        case "Attack3": return Attack3Pressed2;
                        case "Dash": return DashPressed2;
                        default: return false;
                    }
                default:
                    return false;
            }
        }
        
        public bool GetButton(int playerID, string actionName)
        {
            switch (playerID)
            {
                case 1:
                    switch (actionName)
                    {
                        case "Block": return BlockPressed1;
                        default: return false;
                    }
                case 2:
                    switch (actionName)
                    {
                        case "Block": return BlockPressed2;
                        default: return false;
                    }
                default:
                    return false;
            }
        }
        
        public Vector2 GetMoveInput(int playerID)
        {
            return playerID == 1 ? MoveInput1 : MoveInput2;
        }
    }
}
