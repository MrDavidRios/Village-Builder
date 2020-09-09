using System.Collections.Generic;
using UnityEngine;

namespace DavidRios.Building
{
    public class BuildingParentObjects : MonoBehaviour
    {
        public static Dictionary<string, Transform> BuildingParentObjectDict = new Dictionary<string, Transform>();

        [SerializeField] private List<string> _buildingParentNames;
        [SerializeField] private List<Transform> _buildingParents;

        private void Awake()
        {
            for (var i = 0; i < _buildingParentNames.Count; i++)
                BuildingParentObjectDict.Add(_buildingParentNames[i], _buildingParents[i]);
        }
    }
}