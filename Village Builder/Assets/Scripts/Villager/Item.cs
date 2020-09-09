using System;
using UnityEngine;

[Serializable]
public class Item
{
    public GameObject itemObject;
    public string itemType;
}

[Serializable]
public class ItemBundle
{
    public Item item;
    public int amount;
}