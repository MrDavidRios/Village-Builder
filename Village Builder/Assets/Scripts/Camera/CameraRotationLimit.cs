using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationLimit : MonoBehaviour {

    /// <summary>
    /// This script manages the rotation of the camera by setting its rotation limits.
    /// </summary>
	
    //Floats
	public float speed = 10.0F;
	public float RotSpeed = 150.0F;
	public float minY = 0.0f;
	public float maxY = -90.0f;

	private float forwardBackward;
	private float leftRight;
	private float RotLeftRight;
	private float RotUpDown;

	//Vectors
	Vector3 euler;

	public void CameraRotate()
	{
		euler = transform.localEulerAngles;

		// Getting axes
		RotLeftRight = Input.GetAxis("Mouse X") * RotSpeed * Time.unscaledDeltaTime;
		RotUpDown = Input.GetAxis("Mouse Y") * -RotSpeed * Time.unscaledDeltaTime;

		// Doing movements
		euler.y += RotLeftRight;

		euler.x += RotUpDown;

		LimitRotation ();

        //Set the camera's rotation value to the 'euler' rotation value
        transform.localEulerAngles = euler;
    }

	public void LimitRotation()
	{
		if(euler.x >= maxY)
			euler.x = maxY;
		if(euler.x <= minY)
			euler.x = minY;
	}
}
