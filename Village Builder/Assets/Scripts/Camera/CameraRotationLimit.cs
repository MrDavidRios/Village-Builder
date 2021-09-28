using UnityEngine;
using UnityEngine.InputSystem;

namespace DavidRios.Camera
{
    public class CameraRotationLimit : MonoBehaviour
    {
        /// <summary>
        ///     This script manages the rotation of the camera by setting its rotation limits.
        /// </summary>

        //Floats
        public float speed = 10.0F;

        public float rotSpeed = 150.0F;
        public float minY;
        public float maxY = -90.0f;

        //Vectors
        private Vector3 _euler;

        private float _forwardBackward;
        private float _leftRight;
        private float _rotLeftRight;
        private float _rotUpDown;

        public void CameraRotate()
        {
            _euler = transform.localEulerAngles;

            // Getting axes
            _rotLeftRight = Mouse.current.delta.ReadValue().x * rotSpeed * Time.unscaledDeltaTime;
            _rotUpDown = Mouse.current.delta.ReadValue().y * -rotSpeed * Time.unscaledDeltaTime;

            // Doing movements
            _euler.y += _rotLeftRight;

            _euler.x += _rotUpDown;

            LimitRotation();

            //Set the camera's rotation value to the 'euler' rotation value
            transform.localEulerAngles = _euler;
        }

        private void LimitRotation()
        {
            if (_euler.x >= maxY)
                _euler.x = maxY;
            if (_euler.x <= minY)
                _euler.x = minY;
        }
    }
}