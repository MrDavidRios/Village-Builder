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
        float relativeDeltaTime;

        if (Time.timeScale != 0)
            relativeDeltaTime = Time.deltaTime / Time.timeScale;
        else
            relativeDeltaTime = 0f;

        switch (rotationAxis)
        {
            case RotationAxis.X_Axis:
                transform.Rotate(speed * relativeDeltaTime, 0f, 0f);
                break;

            case RotationAxis.Y_Axis:
                transform.Rotate(0f, speed * relativeDeltaTime, 0f);
                break;

            case RotationAxis.Z_Axis:
                transform.Rotate(0f, 0f, speed * relativeDeltaTime);
                break;
        }
    }
}
