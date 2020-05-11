using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour
{
    public int totalStorage;

    [SerializeField]
    private int plotCapacity;

    public int numberOfPlots;

    private void Awake()
    {
        plotCapacity = totalStorage / numberOfPlots;
    }
}
