using System;
using UnityEngine;

namespace DavidRios.Assets.Scripts.Villager
{
    [Serializable]
    public class Job
    {
        public string jobType;
        public Vector3 position;
        public Transform[] objectiveTransforms;
        public int[] amounts;
    }
}