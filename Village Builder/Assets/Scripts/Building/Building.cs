using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "Building")]
public class Building : ScriptableObject
{
    public new string name;

    public int width;
    public int length;

    public ItemBundle[] requiredResources;
}