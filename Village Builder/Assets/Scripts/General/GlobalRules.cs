using UnityEngine;

public class GlobalRules : MonoBehaviour
{
    public static float _maxRaycastDistance = 75f;

    //Floats
    public float maxRaycastDistance;

    private void Awake()
    {
        _maxRaycastDistance = maxRaycastDistance;
    }
}