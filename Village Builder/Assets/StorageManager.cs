using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageManager : MonoBehaviour
{
    public static List<Transform> storages = new List<Transform>();

    public void Update()
    {
        foreach (Transform child in transform)
        {
            if (!storages.Contains(child))
                storages.Add(child);
        }
    }

    public static bool SpaceLeftForItemBundle(string itemType, int itemAmount)
    {
        if (storages.Count == 0)
            return false;

        int spaceLeft = 0;

        for (int i = 0; i < storages.Count; i++)
        {
            spaceLeft += storages[i].GetComponent<Storage>().GetSpaceForItemsByItemType(itemType);
        }

        if (spaceLeft > itemAmount)
            return true;
        else
            return false;
    }
}