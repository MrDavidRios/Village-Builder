using UnityEngine;

public class GlobalRules : MonoBehaviour
{
    public static float MAXRaycastDistance = 75f;

    //Floats
    public float maxRaycastDistance;

    private void Awake()
    {
        MAXRaycastDistance = maxRaycastDistance;
    }
}