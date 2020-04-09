using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public int resourceAmount { get; private set; }

    public bool beingHarvested;

    public string resourceType;

    private void Start()
    {
        bool indeterminateScale = resourceType.ToLower() == "stone";

        resourceAmount = GlobalResourceSettings.ResourceAmount(resourceType);

        float resourceAmountCalculatable = resourceAmount;

        if (indeterminateScale)
        {
            //0.7f is the original stone scale.

            float scaleDifference = transform.localScale.x - 0.7f;

            float percentIncrease = scaleDifference / 0.7f * 10f;

            resourceAmount += Mathf.RoundToInt(GlobalResourceSettings.resourceVariationAmount * percentIncrease);
        }
        else
            resourceAmount = Mathf.RoundToInt(resourceAmountCalculatable * Mathf.Pow(transform.localScale.x, GlobalResourceSettings.resourceVariationAmount));
    }

    public void HarvestResource(int _resourceAmount) => resourceAmount -= _resourceAmount;
}
