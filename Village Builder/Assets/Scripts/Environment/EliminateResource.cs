using DavidRios.Environment;
using UnityEngine;

public class EliminateResource : MonoBehaviour
{
    private Resource resource;

    private void Awake() => resource = transform.GetChild(0).GetComponent<Resource>();

    public void EliminateSelf()
    {
        switch (resource.resourceType)
        {
            case "wood":
                Environment.RemoveTree(resource.transform);
                break;
            case "stone":
                break;
            default:
                return;
        }
    }

    public void DeleteHarvestMarker() => Destroy(transform.GetChild(0).GetChild(0).gameObject);
}