using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DavidRios.Building
{
    public class TemplateActions : MonoBehaviour
    {
        private static readonly int BuildingColor = Shader.PropertyToID("_BuildingColor");

        public static void SetTemplateColor(Color color)
        {
            var templateBuildingMaterials =
                PositionBuildingTemplate.TemplateBuilding.GetComponent<Renderer>().sharedMaterials;

            for (var i = 0; i < templateBuildingMaterials.Length; i++)
                templateBuildingMaterials[i].SetColor(BuildingColor, color);

            PositionBuildingTemplate.TemplateBuilding.GetComponent<Renderer>().sharedMaterials =
                templateBuildingMaterials;
        }

        public static void SetTemplate(GameObject _templateBuilding)
        {
            ClearTemplate();

            PositionBuildingTemplate.TemplateBuilding = Instantiate(_templateBuilding);

            //Removing the "(Clone)" part for added cleanliness and replacing it with "_Template"
            PositionBuildingTemplate.TemplateBuilding.name =
                PositionBuildingTemplate.TemplateBuilding.name.Remove(PositionBuildingTemplate.TemplateBuilding.name
                    .Length - 7) + "_Template";
        }

        public static void ClearTemplate()
        {
            PositionBuildingTemplate.PlacingBuilding = false;

            Destroy(PositionBuildingTemplate.TemplateBuilding);

            PositionBuildingTemplate.TemplateBuilding = null;
        }

        public static IEnumerator RotateTemplate(float _angle, float rotTime)
        {
            PositionBuildingTemplate.Rotating = true;

            var normalizedAngle = Vector3.up * _angle;

            var fromAngle = PositionBuildingTemplate.TemplateBuilding.transform.rotation;
            var toAngle =
                Quaternion.Euler(PositionBuildingTemplate.TemplateBuilding.transform.eulerAngles + normalizedAngle);

            for (var t = 0f; t < 1; t += Time.unscaledDeltaTime / rotTime)
            {
                if (PositionBuildingTemplate.TemplateBuilding == null)
                {
                    PositionBuildingTemplate.Rotating = false;
                    yield break;
                }

                PositionBuildingTemplate.TemplateBuilding.transform.rotation = Quaternion.Lerp(fromAngle, toAngle, t);

                yield return null;
            }

            PositionBuildingTemplate.TemplateBuilding.transform.rotation = toAngle;
            PositionBuildingTemplate.Rotating = false;
        }

        public void PlaceTemplate(Vector3 newBuildingPos, Vector3 newBuildingRotation,
            List<Vector3> occupiedTileCenters)
        {
            if (GetComponent<CheckTiles>().CheckIfInvalid(occupiedTileCenters))
            {
                Debug.LogError("Cannot place; obstruction in area.");
                return;
            }

            var buildingObject = Instantiate(PositionBuildingTemplate.TemplateBuilding);

            var buildingType = BuildingOperations
                .GetBuildingScriptableObject(PositionBuildingTemplate.TemplateBuilding.transform).buildingType;

            switch (buildingType)
            {
                case "Storage":
                    buildingObject.transform.parent = BuildingParentObjects.BuildingParentObjectDict["Storages"];
                    break;
                case "House":
                    buildingObject.transform.parent = BuildingParentObjects.BuildingParentObjectDict["Houses"];
                    break;
                case "Road":
                    buildingObject.transform.parent = BuildingParentObjects.BuildingParentObjectDict["Roads"];
                    break;
                default:
                    Debug.LogError("Invalid building type: " + buildingType);
                    break;
            }

            buildingObject.transform.position = new Vector3(newBuildingPos.x, 0f, newBuildingPos.z) +
                                                buildingObject.GetComponent<BuildingTemplate>().positionOffset;

            buildingObject.transform.localEulerAngles =
                new Vector3(newBuildingRotation.x, newBuildingRotation.y, newBuildingRotation.z);

            buildingObject.layer = LayerMask.NameToLayer("Building");

            buildingObject.AddComponent<BoxCollider>();

            buildingObject.GetComponent<BoxCollider>().center = Vector3.zero;
            buildingObject.GetComponent<BoxCollider>().size = new Vector3(2f, 2f, 1f);

            buildingObject.name = buildingObject.name.Split('_')[0];

            if (buildingObject.name.Substring(buildingObject.name.Length - 3).Contains("x"))
                buildingObject.name = buildingObject.name.Substring(0, buildingObject.name.Length - 3);

            StartCoroutine(FinishPlacement(buildingObject, occupiedTileCenters));
        }

        private IEnumerator FinishPlacement(GameObject buildingObject, List<Vector3> occupiedTileCenters)
        {
            var occupiedTileIndices = new List<Vector2>();

            //Set all occupied tiles as unwalkable
            for (var i = 0; i < occupiedTileCenters.Count; i++)
            {
                while (Environment.Environment.TileCentresMap.Forward[occupiedTileCenters[i]].x == 0 &&
                       Environment.Environment.TileCentresMap.Forward[occupiedTileCenters[i]].y == 0) yield return null;

                var tileX = Environment.Environment.TileCentresMap.Forward[occupiedTileCenters[i]].x;
                var tileY = Environment.Environment.TileCentresMap.Forward[occupiedTileCenters[i]].y;

                //Debug.Log($"Tile X: {tileX}; Tile Y: {tileY}");
                Environment.Environment.BuildingPlaced[tileX, tileY] = true;

                occupiedTileIndices.Add(new Vector2(tileX, tileY));
            }

            BuildingOperations.GetBuildingScript(buildingObject.transform)._occupiedIndices = occupiedTileIndices;

            buildingObject.GetComponent<UnderConstruction>().PlaceTemplate();
        }
    }
}