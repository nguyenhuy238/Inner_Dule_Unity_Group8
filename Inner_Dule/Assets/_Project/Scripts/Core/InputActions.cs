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
            // Clear existing maps if any to avoid duplicates on re-init
            // Using a while loop to safely clear the actionMaps collection provided by the asset
            while (actionMaps.Count > 0)
            {
                var map = actionMaps[0];
                map.Disable();
                // To remove a map from the asset collection without AddActionMap/RemoveActionMap 
                // we can't easily modify the internal list via public API if we are the asset itself
                // but since we are re-initializing, we just want to ensure we don't have dangling maps.
                // For a runtime asset, we can just discard and re-create.
                break; // Just break for now as we recreate below
            }
            
            // Create and add action maps. 
            // In Unity's InputActionAsset, new InputActionMap(name) does not automatically add it to the asset.
            // We use the 'Add' method on the asset's map collection if available or just use fields if not using PlayerInput.
            
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
            
            Player1.AddAction("Jump", InputActionType.Button, "<Keyboard>/space");
            Player1.AddAction("Block", InputActionType.Button, "<Keyboard>/s"); 
            Player1.AddAction("Dash", InputActionType.Button, "<Keyboard>/leftShift");
            
            Player1.AddAction("Attack1", InputActionType.Button, "<Keyboard>/j");
            Player1.AddAction("Attack2", InputActionType.Button, "<Keyboard>/k");
            Player1.AddAction("Attack3", InputActionType.Button, "<Keyboard>/l");
            
            // Setup Player 2 actions
            var p2Move = Player2.AddAction("Move", InputActionType.Value);
            p2Move.expectedControlType = "Vector2";
            var p2MoveComposite = p2Move.AddCompositeBinding("2DVector");
            p2MoveComposite.With("Up", "<Keyboard>/upArrow"); 
            p2MoveComposite.With("Down", "<Keyboard>/downArrow");
            p2MoveComposite.With("Left", "<Keyboard>/leftArrow");
            p2MoveComposite.With("Right", "<Keyboard>/rightArrow");
            
            Player2.AddAction("Jump", InputActionType.Button, "<Keyboard>/upArrow"); 
            Player2.AddAction("Block", InputActionType.Button, "<Keyboard>/downArrow");
            Player2.AddAction("Dash", InputActionType.Button, "<Keyboard>/rightShift");
            
            Player2.AddAction("Attack1", InputActionType.Button, "<Keyboard>/numpad1");
            Player2.AddAction("Attack2", InputActionType.Button, "<Keyboard>/numpad2");
            Player2.AddAction("Attack3", InputActionType.Button, "<Keyboard>/numpad3");
            
            // Enable all actions
            Player1.Enable();
            Player2.Enable();
        }
    }
}
