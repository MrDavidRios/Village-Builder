using UnityEngine;

public class GlobalRules : MonoBehaviour
{
    //Floats
    public float maxRaycastDistance;

    public static float _maxRaycastDistance = 75f;

    private void Awake()
    {
        _maxRaycastDistance = maxRaycastDistance;
    }
}
