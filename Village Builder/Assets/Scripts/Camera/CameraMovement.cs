using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CameraMovement : MonoBehaviour
{
    /// <summary>
    /// This script manages the movement, rotation, and panning of the camera. This is vital to a smooth experience.
    /// </summary>

    //Floats
    public float turnSpeed = 4.0f;      //Speed of camera turning when mouse moves in along an axis
    public float panSpeed = 4.0f;       //Speed of the camera when being panned
    public float zoomSpeed = 4.0f;      //Speed of the camera going back and forth
    public float speed = 5.0f;
    private float originalSpeed;
    public float sensitivity = 0.05f;

    public float MIN_X = 1f; //Minimum x bound 
    public float MIN_Y = 2f; //Minimum y bound 
    public float MIN_Z = 1f; //Minimum z bound 

    public float MAX_X = 100f; //Maximum x bound
    public float MAX_Y = 100f; //Maximum y bound
    public float MAX_Z = 100f; //Maximum z bound

    private float h = 0f; //X-Axis movement
    private float j = 0f; //Z-Axis movement

    //Positions
    private Vector3 mouseOrigin;    //Position of cursor when mouse dragging starts

    //Booleans
    private bool isPanning;     //Is the camera being panned?
    private bool isRotating;    //Is the camera being rotated?
    private bool isZooming;     //Is the camera zooming?
    private bool withinBounds;  //Is the camera within its bounds?

    //Scripts
    private CameraRotationLimit cameraRotationLimit;
    private TerrainGeneration.TerrainGenerator terrainGenerator;

    //Camera
    private Camera mainCamera;

    void Awake()
    {
        //Caching
        cameraRotationLimit = GetComponent<CameraRotationLimit>();
        terrainGenerator = FindObjectOfType<TerrainGeneration.TerrainGenerator>();

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        //The camera's original movement speed; this allows for functions that speed up the camera and then switch back to the original camera speed.
        originalSpeed = speed;
    }

    void Update()
    {
        //Change the position limits of the camera based on the position of the terrain.
        //If the terrain is centralized, then set the camera's max and min positions half of the world size away from the origin position in every direction.
        if (terrainGenerator.centralize)
        {
            MAX_X = terrainGenerator.worldSize / 2f;
            MAX_Z = terrainGenerator.worldSize / 2f;

            MIN_X = -MAX_X;
            MIN_Z = -MAX_Z;
        }
        else
        //If the terrain is not centralized, then set the camera's max positions to the size of the world, min positions at the origin.
        {
            MAX_X = terrainGenerator.worldSize;
            MAX_Z = terrainGenerator.worldSize;

            MIN_X = 0f;
            MIN_Z = 0f;
        }

        //Make sure the limits of the camera's position are proportional in every direction (square)
        if (MAX_X == MAX_Z)
            MAX_Y = MAX_X;
        else
            MAX_Y = ((MAX_X + MAX_Z) / 2);

        //If the game isn't paused: Time.timeScale = 0 means that it's paused. Time.timeScale = 1 means that the game is running at normal speed, 2 is 2x speed, and so on.
        if (Time.timeScale != 0)
        {
            //Bounds
            transform.position = new Vector3
            (
                Mathf.Clamp(transform.position.x, MIN_X, MAX_X),
                Mathf.Clamp(transform.position.y, MIN_Y, MAX_Y),
                Mathf.Clamp(transform.position.z, MIN_Z, MAX_Z)
            );

            //Camera's new position on the y-axis
            float yValue = 0f;

            //If the 'Q' key is pressed and the camera's y position isn't out of minimum bounds, move the camera downward.
            if (Input.GetKey(KeyCode.Q) && !(transform.position.y <= MIN_Y))
            {
                yValue = -speed * Time.deltaTime / Time.timeScale;
                transform.position = new Vector3(transform.position.x, transform.position.y + yValue, transform.position.z);
            }

            //If the 'Shift' key is pressed and the camera's y position isn't out of minimum bounds, move the camera downward.
            if (Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.Space) && !(transform.position.y <= MIN_Y))
            {
                yValue = -speed * Time.deltaTime / Time.timeScale;
                transform.position = new Vector3(transform.position.x, transform.position.y + yValue, transform.position.z);
            }

            //If the 'E' key is pressed and the camera's y position isn't out of maximum bounds, move the camera upward.
            if (Input.GetKey(KeyCode.E) && !(transform.position.y >= MAX_Y))
            {
                yValue = speed * Time.deltaTime / Time.timeScale;
                transform.position = new Vector3(transform.position.x, transform.position.y + yValue, transform.position.z);
            }

            //If the 'Space' key is pressed and the camera's y position isn't out of maximum bounds, move the camera upward.
            if (Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift) && !(transform.position.y >= MAX_Y))
            {
                yValue = speed * Time.deltaTime / Time.timeScale;
                transform.position = new Vector3(transform.position.x, transform.position.y + yValue, transform.position.z);
            }

            //If the 'LeftCtrl' button is pressed, speed the camera up. Values are divided by Time.timeScale to equalize movement when the game is running at different speeds.
            if (Input.GetKey(KeyCode.LeftControl))
                speed = 45f / Time.timeScale;
            else
                speed = originalSpeed / Time.timeScale;

            //Camera's Horizontal Movement & Bounds
            bool moveFreely = false;

            //This is done by axis because rather than the y-axis, this is a 2-dimensional axis (x-axis and z-axis).
            float xAxisMovementValue = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            float zAxisMovementValue = Input.GetAxis("Vertical") * speed * Time.deltaTime;

            //Check if the camera is within bounds: if so, then the camera can move freely. If not, then the camera isn't allowed to move freely.
            if (
                   !(Mathf.Ceil(transform.position.x) > MAX_X) && !(Mathf.Ceil(transform.position.x) < MIN_X) &&
                   !(Mathf.Ceil(transform.position.y) > MAX_Y) && !(Mathf.Ceil(transform.position.y) < MIN_Y) &&
                   !(Mathf.Ceil(transform.position.z) > MAX_Z) && !(Mathf.Ceil(transform.position.z) < MIN_Z)
               )
                moveFreely = true;
            else
                moveFreely = false;

            //If the camera is allowed to move freely, make the h and j variables equal to their movement axis values.
            if (moveFreely)
            {
                h = xAxisMovementValue;
                j = zAxisMovementValue;
            }
            else
            {
                h = 0;
                j = 0;
            }

            //Move the camera in directions (directions are expressed in Vector3s)
            Vector3 RIGHT = transform.TransformDirection(Vector3.right);
            Vector3 FORWARD = transform.TransformDirection(Vector3.forward);

            //Prevent camera glitching

            //Down Vertical
            if (RIGHT.y < 0 && Mathf.Abs(transform.position.y - MIN_Y) <= 1f)
                RIGHT.y = 0f;

            if (FORWARD.y < 0 && Mathf.Abs(transform.position.y - MIN_Y) <= 1f)
                FORWARD.y = 0f;

            //Up Vertical
            if (Input.anyKey && Mathf.Abs(transform.position.y - MAX_Y) <= 5f && transform.eulerAngles.x > 0)
            {
                RIGHT.y = 0f;
                FORWARD.y = 0f;
            }

            //Set the camera's new position to the camera's current position added by the new axis values multiplied by the orientation/direction of the camera.
            Vector3 newPos = transform.localPosition + (RIGHT * h) + (FORWARD * j);

            //If the new position of the camera would be outside bounds, don't let the camera move by setting the new position to the camera's current position. (x-axis)
            if (newPos.x >= MAX_X || newPos.x <= MIN_X)
                newPos.x = transform.localPosition.x;

            //If the new position of the camera would be outside bounds, don't let the camera move by setting the new position to the camera's current position. (z-axis)
            if (newPos.z >= MAX_Z || newPos.z <= MIN_Z)
                newPos.z = transform.localPosition.z;

            transform.localPosition = newPos;

            //Get the right mouse button
            if (Input.GetMouseButtonDown(1))
            {
                // Get mouse origin
                mouseOrigin = Input.mousePosition;
                isRotating = true;
            }

            //Get the left mouse button
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !PlaceBuilding._placingBuilding)
            {
                // Get mouse origin
                mouseOrigin = Input.mousePosition;
                isPanning = true;
            }

            //Get the middle mouse button
            if (Input.GetMouseButtonDown(2))
            {
                // Get mouse origin
                mouseOrigin = Input.mousePosition;

                isZooming = true;
            }

            //Disable movements on button release
            if (!Input.GetMouseButton(1))
                isRotating = false;
            if (!Input.GetMouseButton(0))
                isPanning = false;
            if (!Input.GetMouseButton(2))
                isZooming = false;

            //Rotate camera along X and Y axis
            if (isRotating)
                cameraRotationLimit.CameraRotate();

            //Move the camera on its XY plane
            if (isPanning)
            {
                Vector3 pos = mainCamera.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

                Vector3 move = new Vector3(pos.x * panSpeed, pos.y * panSpeed, 0);
                transform.Translate(move, Space.Self);
            }

            //Move the camera linearly along Z-axis
            if (isZooming)
            {
                Vector3 pos = mainCamera.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

                Vector3 move = pos.y * zoomSpeed * transform.forward;
                transform.Translate(move, Space.World);
            }
        }
    }
}