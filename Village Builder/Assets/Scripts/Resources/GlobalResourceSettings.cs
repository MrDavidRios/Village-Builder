using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalResourceSettings
{
    public static int defaultWoodAmount = 5;
    public static int defaultStoneAmount = 10;

    public static float resourceVariationAmount = 2;

    public static int ResourceAmount(string resourceType = "wood") 
    {
        resourceType = resourceType.ToLower();

        switch (resourceType)
        {
            default:
            case "wood":
                return defaultWoodAmount;
            case "stone":
                return defaultStoneAmount;
        }
    }
}
