using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public enum RotationAxis
    {
        X_Axis,
        Y_Axis,
        Z_Axis
    }

    public RotationAxis rotationAxis;

    public float speed;

    private void Update()
    {
        switch (rotationAxis)
        {
            case RotationAxis.X_Axis:
                transform.Rotate(speed * Time.deltaTime, 0f, 0f);
                break;

            case RotationAxis.Y_Axis:
                transform.Rotate(0f, speed * Time.deltaTime, 0f);
                break;

            case RotationAxis.Z_Axis:
                transform.Rotate(0f, 0f, speed * Time.deltaTime);
                break;
        }
    }
}
