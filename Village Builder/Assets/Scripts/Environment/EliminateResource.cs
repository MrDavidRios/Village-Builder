using System.Collections;
using System.Collections.Generic;
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

    public void DeleteHarvestMarker() => GameObject.Destroy(transform.GetChild(0).GetChild(0).gameObject); 
}
