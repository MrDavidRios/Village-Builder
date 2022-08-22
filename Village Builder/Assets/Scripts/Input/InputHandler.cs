using UnityEngine;
using UnityEngine.InputSystem;

namespace DavidRios.Input
{
    public class InputHandler : MonoBehaviour
    {
        public static Vector3 mousePosition;

        public static float scrollValue;

        public static PlayerController playerControllerInstance;

        private PlayerController _playerController;

        private void Awake()
        {
            _playerController = new PlayerController();

            playerControllerInstance = _playerController;
        }

        private void Update()
        {
            mousePosition = Mouse.current.position.ReadValue();

            scrollValue = Mouse.current.scroll.ReadValue().y;
        }

        private void OnEnable()
        {
            _playerController.Enable();
        }

        private void OnDisable()
        {
            _playerController.Disable();
        }

        public static bool PressedPositive(InputAction inputAction)
        {
            return inputAction.ReadValue<float>() > 0;
        }

        public static bool PressedNegative(InputAction inputAction)
        {
            return inputAction.ReadValue<float>() < 0;
        }

        public static bool Held(InputAction inputAction)
        {
            return inputAction.phase == InputActionPhase.Performed;
        }

        public static bool Pressed(InputAction inputAction)
        {
            return inputAction.triggered;
        }

        public static bool HeldSimultaneously(InputAction inputAction1, InputAction inputAction2)
        {
            return inputAction1.phase == InputActionPhase.Performed && inputAction2.phase == InputActionPhase.Performed;
        }

        public static bool PressedSimultaneously(InputAction inputAction1, InputAction inputAction2)
        {
            return inputAction1.triggered && inputAction2.triggered;
        }
    }
}