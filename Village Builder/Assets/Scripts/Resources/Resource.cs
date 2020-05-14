using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public int resourceAmount { get; private set; }

    public string resourceType;

    public bool beingHarvested;

    private bool harvestIndicatorExists;

    private void Start()
    {
        harvestIndicatorExists = false;

        bool indeterminateScale = resourceType.ToLower() == "stone";

        resourceAmount = GlobalResourceSettings.ResourceAmount(resourceType);

        float resourceAmountCalculatable = resourceAmount;

        if (indeterminateScale)
        {
            //0.7f is the original stone scale.

            float scaleDifference = transform.parent.localScale.x - 0.7f;

            float percentIncrease = scaleDifference / 0.7f * 10f;

            resourceAmount += Mathf.RoundToInt(GlobalResourceSettings.resourceVariationAmount * percentIncrease);
        }
        else
            resourceAmount = Mathf.RoundToInt(resourceAmountCalculatable * Mathf.Pow(transform.parent.localScale.x, GlobalResourceSettings.resourceVariationAmount));
    }

    public void HarvestResource(int _resourceAmount) => resourceAmount -= _resourceAmount;

    public void AddHarvestIndicator() 
    {
        if (harvestIndicatorExists)
            return;

        Vector3 posOffset = Vector3.zero;
        Vector3 scaleFactor = Vector3.one;

        //float rotateSpeed;

        GameObject indicatorPrefab = null;

        switch (resourceType.ToLower())
        {
            case "wood":
                indicatorPrefab = Resources.Load("Prefabs/VFX/Axe") as GameObject;

                posOffset = new Vector3(posOffset.x, posOffset.y + 4.5f, posOffset.z);
                break;
            case "stone":
                indicatorPrefab = Resources.Load("Prefabs/VFX/Pickaxe") as GameObject;
                break;
            default:
                return;
        }

        GameObject harvestIndicator = Instantiate(indicatorPrefab, transform);

        harvestIndicator.transform.localPosition = posOffset;
        harvestIndicator.transform.localScale = new Vector3(harvestIndicator.transform.localScale.x * scaleFactor.x, harvestIndicator.transform.localScale.y * scaleFactor.y, harvestIndicator.transform.localScale.z * scaleFactor.z);

        harvestIndicator.name = indicatorPrefab.name;

        harvestIndicatorExists = true;
    }
}
