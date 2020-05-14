using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public GameObject itemObject;
    public string itemType;
}

[System.Serializable]
public class ItemBundle
{
    public Item item;
    public int amount;
}
