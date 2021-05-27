using System.Collections.Generic;
using DavidRios.Assets.Scripts.Villager;
using UnityEngine;

namespace DavidRios.Building
{
    [CreateAssetMenu(fileName = "New Building", menuName = "Building")]
    public class Building : ScriptableObject
    {
        public new string name;
        public string buildingType;

        public int width;
        public int length;

        public bool walkable;

        public ItemBundle[] requiredResources;
    }

    public interface IBuilding
    {
    }

    public abstract class Structure : MonoBehaviour
    {
        public abstract Building _Building { get; set; }
        public abstract List<Vector2> _occupiedIndices { get; set; }
    }
}