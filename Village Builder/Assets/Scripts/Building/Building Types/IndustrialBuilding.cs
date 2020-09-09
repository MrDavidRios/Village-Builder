using System.Collections.Generic;
using UnityEngine;

namespace DavidRios.Building.Building_Types
{
    public class IndustrialBuilding : Structure
    {
        public Building building;
        public override Building _Building { get; set; }

        public override List<Vector2> _occupiedIndices { get; set; }
        //public List<Vector2> occupiedIndices;

        private void Awake()
        {
            _Building = building;
        }
    }
}