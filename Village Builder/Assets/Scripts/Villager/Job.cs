using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Job
{
    public string jobType;
    public Vector3 position;
    public Transform[] objectiveTransforms;
    public int[] amounts;
}
