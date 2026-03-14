using UnityEngine;
using UnityEngine.InputSystem;

namespace InnerDuel.Input
{
    [CreateAssetMenu(fileName = "InputActions", menuName = "InnerDuel/Input Actions")]
    public class InputActions : InputActionAsset
    {
        public InputActionMap Player1;
        public InputActionMap Player2;
        
        public void Initialize()
        {
            // Create action maps
            Player1 = new InputActionMap("Player1");
            Player2 = new InputActionMap("Player2");
            
            // Setup Player 1 actions
            var p1Move = Player1.AddAction("Move", InputActionType.Value);
            p1Move.expectedControlType = "Vector2";
            var p1MoveComposite = p1Move.AddCompositeBinding("2DVector");
            p1MoveComposite.With("Up", "<Keyboard>/w");
            p1MoveComposite.With("Down", "<Keyboard>/s");
            p1MoveComposite.With("Left", "<Keyboard>/a");
            p1MoveComposite.With("Right", "<Keyboard>/d");
            Player1.AddAction("Attack", InputActionType.Button, "<Keyboard>/space");
            Player1.AddAction("Block", InputActionType.Button, "<Keyboard>/leftShift");
            Player1.AddAction("Dash", InputActionType.Button, "<Keyboard>/leftCtrl");
            Player1.AddAction("Skill1", InputActionType.Button, "<Keyboard>/j");
            Player1.AddAction("Skill2", InputActionType.Button, "<Keyboard>/k");
            Player1.AddAction("Skill3", InputActionType.Button, "<Keyboard>/l");
            
            // Setup Player 2 actions
            var p2Move = Player2.AddAction("Move", InputActionType.Value);
            p2Move.expectedControlType = "Vector2";
            var p2MoveComposite = p2Move.AddCompositeBinding("2DVector");
            p2MoveComposite.With("Up", "<Keyboard>/upArrow");
            p2MoveComposite.With("Down", "<Keyboard>/downArrow");
            p2MoveComposite.With("Left", "<Keyboard>/leftArrow");
            p2MoveComposite.With("Right", "<Keyboard>/rightArrow");
            Player2.AddAction("Attack", InputActionType.Button, "<Keyboard>/rightShift");
            Player2.AddAction("Block", InputActionType.Button, "<Keyboard>/rightCtrl");
            Player2.AddAction("Dash", InputActionType.Button, "<Keyboard>/enter");
            Player2.AddAction("Skill1", InputActionType.Button, "<Keyboard>/numpad1");
            Player2.AddAction("Skill2", InputActionType.Button, "<Keyboard>/numpad2");
            Player2.AddAction("Skill3", InputActionType.Button, "<Keyboard>/numpad3");
            
            // Enable all actions
            Player1.Enable();
            Player2.Enable();
        }
    }
}
