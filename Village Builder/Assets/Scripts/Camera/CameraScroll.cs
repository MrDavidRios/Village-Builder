using System;
using DavidRios.Building;
using DavidRios.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DavidRios.Camera
{
    public class CameraScroll : MonoBehaviour
    {
        //Floats
        public float minFov = 15f;
        public float maxFov = 60f;
        public float fovIncrementAmount = 5f;
        public float sensitivity = 10f;
        public float cameraButtonZoomRate = 5f;

        /// <summary>
        ///     This script manages the scrolling of the camera: this effect is achieved by changing the camera's field of view.
        /// </summary>
        private UnityEngine.Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = GetComponent<UnityEngine.Camera>();
        }

        private void Update()
        {
            //Make sure that zooming doesn't occur if the cursor is over UI or if the player is placing a building (because the scroll wheel is used to rotate the building)
            if (!PositionBuildingTemplate.PlacingBuilding && !EventSystem.current.IsPointerOverGameObject())
            {
                //Set 'fov' to the camera's 'fieldOfView' value; this is necessary to perform calculations and then set the camera's FOV to this value once all of the calculations are done.
                var fov = _mainCamera.fieldOfView;
                
                //Gets the 'fov' value by getting the movement value from the scroll wheel.
                fov += (InputHandler.ScrollValue / sensitivity) * -1;

                //Restricts the 'fov' value by making sure that it's within the bounds (minFov and maxFOV)
                fov = Mathf.Clamp(fov, minFov, maxFov);
                
                //TODO: Use DOTween to animate this value
                _mainCamera.fieldOfView = fov;

                //Reset Field of View when 'R' is pressed
                if (InputHandler.Pressed(InputHandler.PlayerControllerInstance.Default.ResetRotate))
                    ResetFOV();
            }
        }

        //These functions are pretty self-explanatory.
        public void CameraZoomIn()
        {
            var desiredPostFOV = _mainCamera.fieldOfView - fovIncrementAmount;

            if (_mainCamera.fieldOfView != desiredPostFOV)
                _mainCamera.fieldOfView += cameraButtonZoomRate * Time.unscaledDeltaTime;
        }

        public void CameraZoomOut() => _mainCamera.fieldOfView += fovIncrementAmount;

        private void ResetFOV() => _mainCamera.fieldOfView = maxFov;
    }
}