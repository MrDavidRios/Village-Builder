using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScroll : MonoBehaviour
{
    /// <summary>
    /// This script manages the scrolling of the camera: this effect is achieved by changing the camera's field of view. 
    /// </summary>

    private Camera mainCamera;

    //Floats
    public float minFov = 15f;
    public float maxFov = 60f;
    public float fovIncrementAmount = 5f;
    public float sensitivity = 10f;
    public float cameraButtonZoomRate = 5f;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (!PlaceBuilding._placingBuilding)
        {
            //Set 'fov' to the camera's 'fieldOfView' value; this is necessary to perform calculations and then set the camera's FOV to this value once all of the calculations are done.
            var fov = mainCamera.fieldOfView;

            //Gets the 'fov' value by getting the movement value from the scrollwheel.
            fov += Input.GetAxis("Mouse ScrollWheel") * -sensitivity;

            //Restricts the 'fov' value by making sure that it's within the bounds (minFov and maxFOV)
            fov = Mathf.Clamp(fov, minFov, maxFov);
            mainCamera.fieldOfView = fov;

            //Reset Field of View when 'R' is pressed
            if (Input.GetKeyDown(KeyCode.R))
                ResetFOV();
        }
    }

    //These functions are pretty self-explanatory.
    public void CameraZoomIn()
    {
        var desiredPostFOV = mainCamera.fieldOfView - fovIncrementAmount;

        if (mainCamera.fieldOfView != desiredPostFOV)
        {
            mainCamera.fieldOfView += (cameraButtonZoomRate * Time.deltaTime);
        }
    }

    public void CameraZoomOut()
    {
        mainCamera.fieldOfView += fovIncrementAmount;
    }

    public void ResetFOV()
    {
        mainCamera.fieldOfView = maxFov;
    }
}
