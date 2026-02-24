using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using InnerDuel.Core;

namespace InnerDuel.Input
{
    public class InputManager : Singleton<InputManager>
    {
        
        private PlayerInput playerInput;
        private InputAction moveAction1, moveAction2;
        private InputAction attackAction1, attackAction2;
        private InputAction blockAction1, blockAction2;
        private InputAction dashAction1, dashAction2;
        
        public Vector2 MoveInput1 { get; private set; }
        public Vector2 MoveInput2 { get; private set; }
        public bool AttackPressed1 { get; private set; }
        public bool AttackPressed2 { get; private set; }
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
            attackAction1 = inputActions.Player1.FindAction("Attack");
            attackAction2 = inputActions.Player2.FindAction("Attack");
            blockAction1 = inputActions.Player1.FindAction("Block");
            blockAction2 = inputActions.Player2.FindAction("Block");
            dashAction1 = inputActions.Player1.FindAction("Dash");
            dashAction2 = inputActions.Player2.FindAction("Dash");
            
            // Enable input actions
            moveAction1?.Enable();
            moveAction2?.Enable();
            attackAction1?.Enable();
            attackAction2?.Enable();
            blockAction1?.Enable();
            blockAction2?.Enable();
            dashAction1?.Enable();
            dashAction2?.Enable();
        }
        
        private void Update()
        {
            // Read input values
            MoveInput1 = moveAction1?.ReadValue<Vector2>() ?? Vector2.zero;
            MoveInput2 = moveAction2?.ReadValue<Vector2>() ?? Vector2.zero;
            
            AttackPressed1 = attackAction1?.WasPressedThisFrame() ?? false;
            AttackPressed2 = attackAction2?.WasPressedThisFrame() ?? false;
            
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
                        case "Attack": return AttackPressed1;
                        case "Dash": return DashPressed1;
                        default: return false;
                    }
                case 2:
                    switch (actionName)
                    {
                        case "Attack": return AttackPressed2;
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
