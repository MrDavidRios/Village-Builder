using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DavidRios.Input
{
    public class InputHandler : MonoBehaviour
    {
        private PlayerController _playerController;

        public static Vector3 MousePosition;

        public static float ScrollValue;

        public static PlayerController PlayerControllerInstance;

        [SerializeField] private UnityEngine.Camera mainCamera;

        public static bool PressedPositive(InputAction inputAction) => inputAction.ReadValue<float>() > 0;
        public static bool PressedNegative(InputAction inputAction) => inputAction.ReadValue<float>() < 0;

        public static bool Held(InputAction inputAction) => inputAction.phase == InputActionPhase.Started;

        public static bool Pressed(InputAction inputAction) => inputAction.triggered;

        public static bool HeldSimultaneously(InputAction inputAction1, InputAction inputAction2) =>
            inputAction1.phase == InputActionPhase.Started && inputAction2.phase == InputActionPhase.Started;

        public static bool PressedSimultaneously(InputAction inputAction1, InputAction inputAction2) =>
            inputAction1.triggered && inputAction2.triggered;

        private void Awake()
        {
            _playerController = new PlayerController();

            PlayerControllerInstance = _playerController;
        }

        private void Update()
        {
            MousePosition = Mouse.current.position.ReadValue();

            ScrollValue = Mouse.current.scroll.ReadValue().y;
        }

        private void OnEnable()
        {
            _playerController.Enable();
        }

        private void OnDisable()
        {
            _playerController.Disable();
        }
    }
}