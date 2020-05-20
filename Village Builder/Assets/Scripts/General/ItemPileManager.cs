using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPileManager : MonoBehaviour
{
    public List<Transform> untendedPiles = new List<Transform>();

    private void Update()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (child.GetComponent<ItemPile>() != null)
            {
                if (!child.GetComponent<ItemPile>().beingPickedUp && !untendedPiles.Contains(child))
                    untendedPiles.Add(child);
            }
        }

        for (int i = 0; i < untendedPiles.Count; i++)
        {
            if (untendedPiles[i] == null)
            {
                untendedPiles.RemoveAt(i);
            }
            else if (untendedPiles[i].GetComponent<ItemPile>().beingPickedUp)
                untendedPiles.RemoveAt(i);
        }
    }

    public Transform GetNextAvailableItemPile()
    {
        for (int i = 0; i < untendedPiles.Count; i++)
        {
            if (untendedPiles[i] == null)
                return null;

            if (!untendedPiles[i].GetComponent<ItemPile>().beingPickedUp)
                return untendedPiles[i];
        }

        return null;
    }
}
