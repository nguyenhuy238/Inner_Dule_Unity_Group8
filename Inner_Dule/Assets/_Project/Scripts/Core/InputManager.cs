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
        private InputAction attackAction1, attackAction2;
        private InputAction blockAction1, blockAction2;
        private InputAction dashAction1, dashAction2;
        private InputAction skill1Action1, skill1Action2;
        private InputAction skill2Action1, skill2Action2;
        private InputAction skill3Action1, skill3Action2;
        
        public Vector2 MoveInput1 { get; private set; }
        public Vector2 MoveInput2 { get; private set; }
        public bool AttackPressed1 { get; private set; }
        public bool AttackPressed2 { get; private set; }
        public bool BlockPressed1 { get; private set; }
        public bool BlockPressed2 { get; private set; }
        public bool DashPressed1 { get; private set; }
        public bool DashPressed2 { get; private set; }
        public bool Skill1Pressed1 { get; private set; }
        public bool Skill1Pressed2 { get; private set; }
        public bool Skill2Pressed1 { get; private set; }
        public bool Skill2Pressed2 { get; private set; }
        public bool Skill3Pressed1 { get; private set; }
        public bool Skill3Pressed2 { get; private set; }
        
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
            skill1Action1 = inputActions.Player1.FindAction("Skill1");
            skill1Action2 = inputActions.Player2.FindAction("Skill1");
            skill2Action1 = inputActions.Player1.FindAction("Skill2");
            skill2Action2 = inputActions.Player2.FindAction("Skill2");
            skill3Action1 = inputActions.Player1.FindAction("Skill3");
            skill3Action2 = inputActions.Player2.FindAction("Skill3");
            
            // Enable input actions
            moveAction1?.Enable();
            moveAction2?.Enable();
            attackAction1?.Enable();
            attackAction2?.Enable();
            blockAction1?.Enable();
            blockAction2?.Enable();
            dashAction1?.Enable();
            dashAction2?.Enable();
            skill1Action1?.Enable();
            skill1Action2?.Enable();
            skill2Action1?.Enable();
            skill2Action2?.Enable();
            skill3Action1?.Enable();
            skill3Action2?.Enable();
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

            Skill1Pressed1 = skill1Action1?.WasPressedThisFrame() ?? false;
            Skill1Pressed2 = skill1Action2?.WasPressedThisFrame() ?? false;
            Skill2Pressed1 = skill2Action1?.WasPressedThisFrame() ?? false;
            Skill2Pressed2 = skill2Action2?.WasPressedThisFrame() ?? false;
            Skill3Pressed1 = skill3Action1?.WasPressedThisFrame() ?? false;
            Skill3Pressed2 = skill3Action2?.WasPressedThisFrame() ?? false;
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
                        case "Skill1": return Skill1Pressed1;
                        case "Skill2": return Skill2Pressed1;
                        case "Skill3": return Skill3Pressed1;
                        default: return false;
                    }
                case 2:
                    switch (actionName)
                    {
                        case "Attack": return AttackPressed2;
                        case "Dash": return DashPressed2;
                        case "Skill1": return Skill1Pressed2;
                        case "Skill2": return Skill2Pressed2;
                        case "Skill3": return Skill3Pressed2;
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
