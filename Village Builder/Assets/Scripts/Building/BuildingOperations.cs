using DavidRios.Building.Building_Types;
using UnityEngine;

namespace DavidRios.Building
{
    public static class BuildingOperations
    {
        public static Building GetBuildingScriptableObject<T>(T building) where T : Transform
        {
            switch (building.tag)
            {
                case "Infrastructure":
                    return building.GetComponent<InfrastructureBuilding>()._Building;
                case "Industrial":
                    return building.GetComponent<IndustrialBuilding>()._Building;
                case "Residential":
                    return building.GetComponent<ResidentialBuilding>()._Building;
                default:
                    Debug.LogError("Invalid building type: " + building.tag);
                    return null;
            }
        }

        public static Structure GetBuildingScript<T>(T building) where T : Transform
        {
            switch (building.tag)
            {
                case "Infrastructure":
                    return building.GetComponent<InfrastructureBuilding>();
                case "Industrial":
                    return building.GetComponent<IndustrialBuilding>();
                case "Residential":
                    return building.GetComponent<ResidentialBuilding>();
                default:
                    Debug.LogError("Invalid building type: " + building.tag);
                    return null;
            }
        }
    }
}