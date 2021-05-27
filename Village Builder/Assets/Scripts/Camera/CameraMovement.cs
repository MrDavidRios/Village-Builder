using System;
using DavidRios.Building;
using DavidRios.Input;
using Terrain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DavidRios.Camera
{
    public class CameraMovement : MonoBehaviour
    {
        /// <summary>
        ///     This script manages the movement, rotation, and panning of the camera. This is vital to a smooth experience.
        /// </summary>

        //Floats
        [Header("Speeds")]
        public float turnSpeed = 4.0f; //Speed of camera turning when mouse moves in along an axis

        public float panSpeed = 4.0f; //Speed of the camera when being panned
        
        public float verticalSpeed = 4.0f; //Speed of the camera when being raised/lowered
        private float _originalVerticalSpeed;
        
        public float speed = 5.0f;
        private float _originalSpeed;
        
        public float movementSmoothing;
        public float maxSpeed;

        [Header("Bounds")]
        public float minX = 1f; //Minimum x bound 
        public float minY = 2f; //Minimum y bound 
        public float minZ = 1f; //Minimum z bound 

        public float maxX = 100f; //Maximum x bound
        public float maxY = 100f; //Maximum y bound
        public float maxZ = 100f; //Maximum z bound

        //Scripts
        private CameraRotationLimit _cameraRotationLimit;

        private float _xAxisMovement; //X-Axis movement

        //Booleans
        private bool _isPanning; //Is the camera being panned?
        private bool _isRotating; //Is the camera being rotated?
        private bool _isZooming; //Is the camera zooming?
        private float _zAxisMovement; //Z-Axis movement

        //Camera
        private UnityEngine.Camera _mainCamera;

        //Positions
        private TerrainGenerator _terrainGenerator;
        private Vector3 _velocity = Vector3.zero;
        private bool _withinBounds; //Is the camera within its bounds?

        //Input Handler
        private PlayerController.DefaultActions _input;

        private void Awake()
        {
            //Caching
            _cameraRotationLimit = GetComponent<CameraRotationLimit>();
            _terrainGenerator = FindObjectOfType<TerrainGenerator>();

            _mainCamera = GetComponent<UnityEngine.Camera>();

            //The camera's original movement speed; this allows for functions that speed up the camera and then switch back to the original camera speed.
            _originalSpeed = speed;
            _originalVerticalSpeed = verticalSpeed;
        }

        private void Start() => _input = InputHandler.PlayerControllerInstance.Default;

        private void Update()
        {
            //Change the position limits of the camera based on the position of the terrain.
            //If the terrain is centralized, then set the camera's max and min positions half of the world size away from the origin position in every direction.
            if (_terrainGenerator.centralize)
            {
                maxX = _terrainGenerator.worldSize / 2f;
                maxZ = _terrainGenerator.worldSize / 2f;

                minX = -maxX;
                minZ = -maxZ;
            }
            else
                //If the terrain is not centralized, then set the camera's max positions to the size of the world, min positions at the origin.
            {
                maxX = _terrainGenerator.worldSize;
                maxZ = _terrainGenerator.worldSize;

                minX = 0f;
                minZ = 0f;
            }

            //Make sure the limits of the camera's position are proportional in every direction (square)
            if (maxX == maxZ)
                maxY = maxX;
            else
                maxY = (maxX + maxZ) / 2;

            //Bounds
            transform.position = new Vector3
            (
                Mathf.Clamp(transform.position.x, minX, maxX),
                Mathf.Clamp(transform.position.y, minY, maxY),
                Mathf.Clamp(transform.position.z, minZ, maxZ)
            );

            //Camera's new position on the y-axis
            var yValue = 0f;

            //Camera's new rotation position
            var yRotValue = 0f;

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

            //If the 'Shift' key is pressed and the camera's y position isn't out of minimum bounds, move the camera downward.
            if (InputHandler.PressedNegative(_input.CameraVerticalMovement) && !InputHandler.PressedPositive(_input.CameraVerticalMovement) && !(transform.position.y <= minY))
            {
                yValue = -verticalSpeed * Time.unscaledDeltaTime;

                transform.position = new Vector3(transform.position.x, transform.position.y + yValue, transform.position.z);
            }

            //If the 'Space' key is pressed and the camera's y position isn't out of maximum bounds, move the camera upward.
            if (InputHandler.PressedPositive(_input.CameraVerticalMovement) && !InputHandler.PressedNegative(_input.CameraVerticalMovement) && !(transform.position.y >= maxY))
            {
                yValue = verticalSpeed * Time.unscaledDeltaTime;
                transform.position = new Vector3(transform.position.x, transform.position.y + yValue, transform.position.z);
            }

            //If the 'LeftCtrl' button is pressed, speed the camera up. Values are divided by Time.timeScale to equalize movement when the game is running at different speeds.
            if (InputHandler.Held(_input.BoostCamera))
            {
                speed = _originalSpeed * 2f;
                verticalSpeed = _originalVerticalSpeed * 2f;
            }
            else
            {
                speed = _originalSpeed;
                verticalSpeed = _originalVerticalSpeed;
            }

            maxSpeed = speed + 50;

            //Camera's Horizontal Movement & Bounds
            bool moveFreely;
            
            //This is done by axis because rather than the y-axis, this is a 2-dimensional axis (x-axis and z-axis).
            var xAxisMovementValue = _input.MoveCamera.ReadValue<Vector2>().x * speed * Time.unscaledDeltaTime;
            var zAxisMovementValue = _input.MoveCamera.ReadValue<Vector2>().y * speed * Time.unscaledDeltaTime;

            //Check if the camera is within bounds: if so, then the camera can move freely. If not, then the camera isn't allowed to move freely.
            if (
                !(Mathf.Ceil(transform.position.x) > maxX) && !(Mathf.Ceil(transform.position.x) < minX) &&
                !(Mathf.Ceil(transform.position.y) > maxY) && !(Mathf.Ceil(transform.position.y) < minY) &&
                !(Mathf.Ceil(transform.position.z) > maxZ) && !(Mathf.Ceil(transform.position.z) < minZ)
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
            if (right.y < 0 && Mathf.Abs(transform.position.y - minY) <= 1f)
                right.y = 0f;

            if (forward.y < 0 && Mathf.Abs(transform.position.y - minY) <= 1f)
                forward.y = 0f;

            //Up Vertical
            if (Mathf.Abs(transform.position.y - maxY) <= 5f && transform.eulerAngles.x > 0)
            {
                right.y = 0f;
                forward.y = 0f;
            }

            //Set the camera's new position to the camera's current position added by the new axis values multiplied by the orientation/direction of the camera.
            var newPos = Vector3.SmoothDamp(transform.localPosition, transform.localPosition + right * _xAxisMovement + forward * _zAxisMovement,
                ref _velocity, movementSmoothing, maxSpeed, Time.unscaledDeltaTime);

            //If the new position of the camera would be outside bounds, don't let the camera move by setting the new position to the camera's current position. (x-axis)
            if (newPos.x >= maxX || newPos.x <= minX)
                newPos.x = transform.localPosition.x;

            //If the new position of the camera would be outside bounds, don't let the camera move by setting the new position to the camera's current position. (z-axis)
            if (newPos.z >= maxZ || newPos.z <= minZ)
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
                Mathf.Clamp(transform.position.x, minX, maxX),
                Mathf.Clamp(transform.position.y, minY, maxY),
                Mathf.Clamp(transform.position.z, minZ, maxZ)
            );
        }
    }
}