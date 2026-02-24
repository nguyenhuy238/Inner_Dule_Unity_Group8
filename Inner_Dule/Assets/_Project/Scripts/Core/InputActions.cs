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
            Player1.AddAction("Move", InputActionType.Value, "<Keyboard>/wasd");
            Player1.AddAction("Attack", InputActionType.Button, "<Keyboard>/space");
            Player1.AddAction("Block", InputActionType.Button, "<Keyboard>/leftShift");
            Player1.AddAction("Dash", InputActionType.Button, "<Keyboard>/leftCtrl");
            
            // Setup Player 2 actions
            Player2.AddAction("Move", InputActionType.Value, "<Keyboard>/arrows");
            Player2.AddAction("Attack", InputActionType.Button, "<Keyboard>/rightShift");
            Player2.AddAction("Block", InputActionType.Button, "<Keyboard>/rightCtrl");
            Player2.AddAction("Dash", InputActionType.Button, "<Keyboard>/enter");
            
            // Enable all actions
            Player1.Enable();
            Player2.Enable();
        }
    }
}
