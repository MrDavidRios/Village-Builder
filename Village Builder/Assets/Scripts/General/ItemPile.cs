using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPile : MonoBehaviour
{
    public bool beingPickedUp;

    public int amountOfItems;

    private void Awake()
    { 
        amountOfItems = transform.childCount;

        switch (tag)
        {
            case "Tree":
                tag = "Log";
                break;
            default:
                Debug.LogError("Undefined item type:" + tag);
                break;
        }
    }

    //Returns itemType.
    public string TakeItem(bool lastTake)
    {
        amountOfItems--;

        Transform itemToRemove = transform.GetChild(transform.childCount - 1);

        GameObject.Destroy(itemToRemove.gameObject);

        if (lastTake && amountOfItems > 0)
            beingPickedUp = false;

        return tag;
    }
}
