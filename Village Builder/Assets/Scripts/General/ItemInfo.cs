using System.Collections.Generic;
using UnityEngine;

public static class ItemInfo
{
    public static Dictionary<string, int> itemSizes = new Dictionary<string, int>
    {
        {"Log", 2},
        {"Stone", 1},
        {"StoneBrick", 1},
        {"Iron", 1},
        {"IronOre", 1},
        {"Charcoal", 1}
    };

    public static int GetItemIndex(string itemType)
    {
        switch (itemType)
        {
            case "Log":
                return 0;
            case "Stone":
                return 1;
            case "StoneBrick":
                return 2;
            case "IronOre":
                return 3;
            case "Iron":
                return 4;
            case "Tools":
                return 5;
            case "Food":
                return 6;
            default:
                Debug.LogError("Invalid item type: " + itemType);
                return -1;
        }
    }
}