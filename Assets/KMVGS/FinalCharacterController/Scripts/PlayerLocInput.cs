using UnityEngine;
using UnityEngine.InputSystem;

namespace KMVGS.FinalCharacterController
{
    [DefaultExecutionOrder(-2)]
    public class PlayerLocInput : MonoBehaviour, PlayerControls.IPlayerLocMapsActions
    {
        public PlayerControls PlayerControls { get; private set; }
        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool InteractHeld { get; private set; }
        public bool InteractReleased { get; private set; }

        public PlayerControls.PlayerLocMapsActions Player => PlayerControls.PlayerLocMaps;

        private void Awake()
        {
            PlayerControls = new PlayerControls();
            PlayerControls.PlayerLocMaps.SetCallbacks(this);
        }

        private void OnEnable()
        {
            PlayerControls.Enable();
        }

        private void OnDisable()
        {
            PlayerControls.Disable();
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            InteractPressed = context.performed && context.phase == InputActionPhase.Performed;
            InteractHeld = context.ReadValueAsButton();
            InteractReleased = context.canceled;
        }

        // Empty implementation since we handle this in PlayerController
        public void OnSwitchCamera(InputAction.CallbackContext context) { }
        public void OnJump(InputAction.CallbackContext context) 
        {
            InteractPressed = context.performed && context.phase == InputActionPhase.Performed;
        }

    }
}