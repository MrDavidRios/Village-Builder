using UnityEngine;

public class Rotate : MonoBehaviour
{
    public enum RotationAxis
    {
        XAxis,
        YAxis,
        ZAxis
    }

    public RotationAxis rotationAxis;

    public float speed;

    private void Update()
    {
        switch (rotationAxis)
        {
            case RotationAxis.XAxis:
                transform.Rotate(speed * Time.unscaledDeltaTime, 0f, 0f);
                break;

            case RotationAxis.YAxis:
                transform.Rotate(0f, speed * Time.unscaledDeltaTime, 0f);
                break;

            case RotationAxis.ZAxis:
                transform.Rotate(0f, 0f, speed * Time.unscaledDeltaTime);
                break;
        }
    }
}