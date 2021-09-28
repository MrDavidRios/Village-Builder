using System;
using DavidRios.Building;
using DavidRios.General;
using DavidRios.Input;
using Terrain;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DavidRios.Camera
{
    public class CameraMovement : MonoBehaviour
    {
        //Floats
        public float turnSpeed; //Speed of camera turning when mouse moves in along an axis

        public float panSpeed; //Speed of the camera when being panned
        public float moveSpeed; //Speed of the camera moving (WASD)

        public float ascensionSpeedCoefficient = 0.03f; //Speed of the camera being heightened/lowered (percentage of main speed)

        public float movementSmoothing;
        private float maxSpeed;

        [Header("Bounds (Min, Max)")]
        private Vector2 xBounds = new Vector2(1f, 100f); //X Bounds (min, max)
        private Vector2 yBounds = new Vector2(2f, 100f); //Y Bounds (min, max)
        private Vector2 zBounds = new Vector2(1f, 100f); //Z Bounds (min, max)

        //Scripts
        private CameraRotationLimit _cameraRotationLimit;

        private float _xAxisMovement; //X-Axis movement

        //Booleans
        private bool _isPanning; //Is the camera being panned?
        private bool _isRotating; //Is the camera being rotated?
        private float _zAxisMovement; //Z-Axis movement

        //Positions
        private float _originalSpeed;
        private TerrainGenerator _terrainGenerator;
        private Vector3 _velocity = Vector3.zero;

        //Input Handler
        private PlayerController.DefaultActions _input;

        private void Awake()
        {
            //Caching
            _cameraRotationLimit = GetComponent<CameraRotationLimit>();
            _terrainGenerator = FindObjectOfType<TerrainGenerator>();

            //The camera's original movement speed; this allows for functions that speed up the camera and then switch back to the original camera speed.
            _originalSpeed = moveSpeed;
        }

        private void Start() => _input = InputHandler.playerControllerInstance.Default;

        private void Update()
        {
            #region Bounds

            //Change the position limits of the camera based on the position of the terrain.
            //If the terrain is centralized, then set the camera's max and min positions half of the world size away from the origin position in every direction.
            if (_terrainGenerator.centralize)
            {
                xBounds[1] = _terrainGenerator.worldSize / 2f;
                zBounds[1] = _terrainGenerator.worldSize / 2f;

                xBounds[0] = -xBounds[1]; //If centralized, the minimum x bound is equal to the maximum x bound.
                zBounds[0] = -zBounds[1]; //If centralized, the minimum z bound is equal to the maximum z bound.
            }
            else
            //If the terrain is not centralized, then set the camera's max positions to the size of the world, min positions at the origin.
            {
                xBounds[1] = _terrainGenerator.worldSize;
                zBounds[1] = _terrainGenerator.worldSize;

                xBounds[0] = 0f;
                zBounds[0] = 0f;
            }

            //Make sure the limits of the camera's position are proportional in every direction (square)
            if (Math.Abs(xBounds[1] - zBounds[1]) < NumberGuidelines.FloatComparisonTolerance)
                yBounds[1] = xBounds[1];
            else
                yBounds[1] = (xBounds[1] + zBounds[1]) / 2;

            //Bounds
            transform.position = new Vector3
            (
                Mathf.Clamp(transform.position.x, xBounds[0], xBounds[1]),
                Mathf.Clamp(transform.position.y, yBounds[0], yBounds[1]),
                Mathf.Clamp(transform.position.z, zBounds[0], zBounds[1])
            );

            #endregion

            //Camera's new position on the y-axis
            float yValue;

            //Camera's new rotation position
            float yRotValue;

            if (InputHandler.PressedNegative(_input.RotateCamera))
            {
                yRotValue = -turnSpeed * Time.unscaledDeltaTime;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + yRotValue,
                    transform.eulerAngles.z);
            }

            if (InputHandler.PressedPositive(_input.RotateCamera))
            {
                yRotValue = turnSpeed * Time.unscaledDeltaTime;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + yRotValue,
                    transform.eulerAngles.z);
            }

            //If the 'LeftCtrl' button is pressed, speed the camera up. Values are divided by Time.timeScale to equalize movement when the game is running at different speeds.
            if (InputHandler.Held(_input.BoostCamera))
                moveSpeed = _originalSpeed * 2f;
            else if (InputHandler.PressedNegative(_input.CameraVerticalMovement) && InputHandler.PressedPositive(_input.CameraVerticalMovement))
                moveSpeed = _originalSpeed / 2f;
            else
                moveSpeed = _originalSpeed;

            maxSpeed = moveSpeed + 50;

            //If the 'Shift' key is pressed and the camera's y position isn't out of minimum bounds, move the camera downward.
            if (InputHandler.PressedNegative(_input.CameraVerticalMovement) && !InputHandler.PressedPositive(_input.CameraVerticalMovement) && !(transform.position.y <= yBounds[0]))
            {
                yValue = -moveSpeed * ascensionSpeedCoefficient * Time.unscaledDeltaTime;

                transform.position = new Vector3(transform.position.x, transform.position.y + yValue, transform.position.z);
            }

            //If the 'Space' key is pressed and the camera's y position isn't out of maximum bounds, move the camera upward.
            if (InputHandler.PressedPositive(_input.CameraVerticalMovement) && !InputHandler.PressedNegative(_input.CameraVerticalMovement) && !(transform.position.y >= yBounds[1]))
            {
                yValue = moveSpeed * ascensionSpeedCoefficient * Time.unscaledDeltaTime;
                transform.position = new Vector3(transform.position.x, transform.position.y + yValue, transform.position.z);
            }

            //Camera's Horizontal Movement & Bounds
            bool moveFreely;

            //This is done by axis because rather than the y-axis, this is a 2-dimensional axis (x-axis and z-axis).
            var xAxisMovementValue = _input.MoveCamera.ReadValue<Vector2>().x * moveSpeed * Time.unscaledDeltaTime;
            var zAxisMovementValue = _input.MoveCamera.ReadValue<Vector2>().y * moveSpeed * Time.unscaledDeltaTime;

            //Check if the camera is within bounds: if so, then the camera can move freely. If not, then the camera isn't allowed to move freely.
            if (
                !(Mathf.Ceil(transform.position.x) > xBounds[1]) && !(Mathf.Ceil(transform.position.x) < xBounds[0]) &&
                !(Mathf.Ceil(transform.position.y) > yBounds[1]) && !(Mathf.Ceil(transform.position.y) < yBounds[0]) &&
                !(Mathf.Ceil(transform.position.z) > zBounds[1]) && !(Mathf.Ceil(transform.position.z) < zBounds[0])
            )
                moveFreely = true;
            else
                moveFreely = false;

            //If the camera is allowed to move freely, make the h and j variables equal to their movement axis values.
            if (moveFreely)
            {
                _xAxisMovement = xAxisMovementValue;
                _zAxisMovement = zAxisMovementValue;
            }
            else
            {
                _xAxisMovement = 0;
                _zAxisMovement = 0;
            }

            //Move the camera in directions (directions are expressed in Vector3s)
            var right = transform.TransformDirection(Vector3.right);
            var forward = transform.TransformDirection(Vector3.forward);

            //Prevent camera glitching

            //Down Vertical
            if (right.y < 0 && Mathf.Abs(transform.position.y - yBounds[0]) <= 1f)
                right.y = 0f;

            if (forward.y < 0 && Mathf.Abs(transform.position.y - yBounds[0]) <= 1f)
                forward.y = 0f;

            //Up Vertical
            if (Mathf.Abs(transform.position.y - yBounds[1]) <= 5f && transform.eulerAngles.x > 0)
            {
                right.y = 0f;
                forward.y = 0f;
            }

            //Set the camera's new position to the camera's current position added by the new axis values multiplied by the orientation/direction of the camera.
            var newPos = Vector3.SmoothDamp(transform.localPosition, transform.localPosition + right * _xAxisMovement + forward * _zAxisMovement,
                ref _velocity, movementSmoothing, maxSpeed, Time.unscaledDeltaTime);

            //If the new position of the camera would be outside bounds, don't let the camera move by setting the new position to the camera's current position. (x-axis)
            if (newPos.x >= xBounds[1] || newPos.x <= xBounds[0])
                newPos.x = transform.localPosition.x;

            //If the new position of the camera would be outside bounds, don't let the camera move by setting the new position to the camera's current position. (z-axis)
            if (newPos.z >= zBounds[1] || newPos.z <= zBounds[0])
                newPos.z = transform.localPosition.z;

            transform.localPosition = newPos;

            //Get the left mouse button
            if (InputHandler.HeldSimultaneously(_input.LeftClick, _input.RightClick) && !PositionBuildingTemplate.PlacingBuilding)
                _isPanning = true;
            else
                _isPanning = false;

            //Get the right mouse button
            if (InputHandler.Held(_input.RightClick) && !InputHandler.Held(_input.LeftClick))
                _isRotating = true;
            else
                _isRotating = false;

            //Rotate camera along X and Y axis
            if (_isRotating)
                _cameraRotationLimit.CameraRotate();

            //Move the camera on its XY plane
            if (_isPanning)
            {
                var move = new Vector3(Mouse.current.delta.ReadValue().x * panSpeed, Mouse.current.delta.ReadValue().y * panSpeed, 0);
                transform.Translate(move, Space.Self);
            }

            transform.position = new Vector3
            (
                Mathf.Clamp(transform.position.x, xBounds[0], xBounds[1]),
                Mathf.Clamp(transform.position.y, yBounds[0], yBounds[1]),
                Mathf.Clamp(transform.position.z, zBounds[0], zBounds[1])
            );
        }
    }
}