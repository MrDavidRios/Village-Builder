using System.Collections.Generic;
using Terrain;
using UnityEngine;

namespace DavidRios.Building
{
    //Get the positions of the tiles that the building will occupy when placed
    public class CheckTiles : MonoBehaviour
    {
        [SerializeField] private PositionBuildingTemplate positionBuildingTemplate;
        [SerializeField] private TerrainGenerator terrainGenerator;

        /// <summary>
        ///     Gets all of the tiles that a building is occupying.
        /// </summary>
        /// <param name="building"></param>
        /// <param name="cachedBuildingPos"></param>
        /// <param name="cachedBuildingRot"></param>
        /// <returns></returns>
        public List<Vector3> GetOccupiedTileCentres(Building building, Vector3 cachedBuildingPos,
            Quaternion cachedBuildingRot)
        {
            var templatePos = PositionBuildingTemplate.TemplateBuilding.transform.position;

            //Prevent duplicate members; If the building's position and orientation is the same as before, the tiles it occupies aren't going to be different.
            if (cachedBuildingPos == templatePos &&
                cachedBuildingRot == PositionBuildingTemplate.TemplateBuilding.transform.rotation &&
                positionBuildingTemplate.OccupiedTileCentres.Count > 0)
                return positionBuildingTemplate.OccupiedTileCentres;
            positionBuildingTemplate.OccupiedTileCentres.Clear();

            if (building.width == building.length)
            {
                //If building's dimensions are odd
                if (building.width % 2 != 0) positionBuildingTemplate.OccupiedTileCentres.Add(templatePos);

                switch (building.width)
                {
                    //If the building is 1x1

                    #region 1x1

                    case 1:
                        return positionBuildingTemplate.OccupiedTileCentres;

                    #endregion

                    //If the building is 2x2

                    #region 2x2

                    case 2:
                        positionBuildingTemplate.OccupiedTileCentres.AddMany(
                            new Vector3(templatePos.x - 0.5f, templatePos.y, templatePos.z - 0.5f),
                            new Vector3(templatePos.x - 0.5f, templatePos.y, templatePos.z + 0.5f),
                            new Vector3(templatePos.x + 0.5f, templatePos.y, templatePos.z - 0.5f),
                            new Vector3(templatePos.x + 0.5f, templatePos.y, templatePos.z + 0.5f)
                        );
                        return positionBuildingTemplate.OccupiedTileCentres;

                    #endregion

                    //If the building is 3x3

                    #region 3x3

                    case 3:
                        positionBuildingTemplate.OccupiedTileCentres.AddMany(
                            new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z - 1f),
                            new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z),
                            new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z + 1f),
                            new Vector3(templatePos.x, templatePos.y, templatePos.z - 1f),
                            new Vector3(templatePos.x, templatePos.y, templatePos.z + 1f),
                            new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z - 1f),
                            new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z),
                            new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z + 1f)
                        );
                        return positionBuildingTemplate.OccupiedTileCentres;

                    #endregion

                    //If the building is 4x4

                    #region 4x4

                    case 4:
                        positionBuildingTemplate.OccupiedTileCentres.AddMany(
                            //Middle 2x2
                            new Vector3(templatePos.x - 0.5f, templatePos.y, templatePos.z - 0.5f),
                            new Vector3(templatePos.x - 0.5f, templatePos.y, templatePos.z + 0.5f),
                            new Vector3(templatePos.x + 0.5f, templatePos.y, templatePos.z - 0.5f),
                            new Vector3(templatePos.x + 0.5f, templatePos.y, templatePos.z + 0.5f),

                            //Outer Corners
                            new Vector3(templatePos.x - 1.5f, templatePos.y, templatePos.z - 1.5f),
                            new Vector3(templatePos.x - 1.5f, templatePos.y, templatePos.z + 1.5f),
                            new Vector3(templatePos.x + 1.5f, templatePos.y, templatePos.z - 1.5f),
                            new Vector3(templatePos.x + 1.5f, templatePos.y, templatePos.z + 1.5f),

                            //Outer Sides (Horizontal)
                            new Vector3(templatePos.x - 0.5f, templatePos.y, templatePos.z - 1.5f),
                            new Vector3(templatePos.x + 0.5f, templatePos.y, templatePos.z - 1.5f),
                            new Vector3(templatePos.x - 0.5f, templatePos.y, templatePos.z + 1.5f),
                            new Vector3(templatePos.x + 0.5f, templatePos.y, templatePos.z + 1.5f),

                            //Outer sides (Vertical)
                            new Vector3(templatePos.x - 1.5f, templatePos.y, templatePos.z - 0.5f),
                            new Vector3(templatePos.x - 1.5f, templatePos.y, templatePos.z + 0.5f),
                            new Vector3(templatePos.x + 1.5f, templatePos.y, templatePos.z - 0.5f),
                            new Vector3(templatePos.x + 1.5f, templatePos.y, templatePos.z + 0.5f)
                        );
                        return positionBuildingTemplate.OccupiedTileCentres;

                    #endregion

                    //If the building is 5x5

                    #region 5x5

                    case 5:
                        positionBuildingTemplate.OccupiedTileCentres.AddMany(
                            //Get the center x and z axis tiles

                            //x-axis
                            new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z),
                            new Vector3(templatePos.x + 2f, templatePos.y, templatePos.z),
                            new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z),
                            new Vector3(templatePos.x - 2f, templatePos.y, templatePos.z),

                            //z-axis
                            new Vector3(templatePos.x, templatePos.y, templatePos.z + 1f),
                            new Vector3(templatePos.x, templatePos.y, templatePos.z + 2f),
                            new Vector3(templatePos.x, templatePos.y, templatePos.z - 1f),
                            new Vector3(templatePos.x, templatePos.y, templatePos.z - 2f),

                            //Get the 4 2x2 quadrants that are left

                            //Bottom left
                            new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z - 1f),
                            new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z - 2f),
                            new Vector3(templatePos.x - 2f, templatePos.y, templatePos.z - 1f),
                            new Vector3(templatePos.x - 2f, templatePos.y, templatePos.z - 2f),

                            //Top left
                            new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z + 1f),
                            new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z + 2f),
                            new Vector3(templatePos.x - 2f, templatePos.y, templatePos.z + 1f),
                            new Vector3(templatePos.x - 2f, templatePos.y, templatePos.z + 2f),

                            //Bottom right
                            new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z - 1f),
                            new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z - 2f),
                            new Vector3(templatePos.x + 2f, templatePos.y, templatePos.z - 1f),
                            new Vector3(templatePos.x + 2f, templatePos.y, templatePos.z - 2f),

                            //Top right
                            new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z + 1f),
                            new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z + 2f),
                            new Vector3(templatePos.x + 2f, templatePos.y, templatePos.z + 1f),
                            new Vector3(templatePos.x + 2f, templatePos.y, templatePos.z + 2f)
                        );

                        return positionBuildingTemplate.OccupiedTileCentres;

                    #endregion

                    default:
                        Debug.LogWarning("Invalid building width specified:" + building.width);
                        return null;
                }
            }

            return null;
        }

        /// <summary>
        ///     Checks if there are any obstructions in the given tiles.
        /// </summary>
        /// <param name="occupiedTiles"></param>
        /// <param name="canBePlacedOnWater"></param>
        /// <returns></returns>
        public bool CheckIfInvalid(List<Vector3> occupiedTiles, bool canBePlacedOnWater = false)
        {
            var maxWorldBounds = new Vector3(terrainGenerator.worldSize - 0.5f, 0f, terrainGenerator.worldSize - 0.5f);

            for (var i = 0; i < occupiedTiles.Count; i++)
            {
                occupiedTiles[i] = new Vector3(occupiedTiles[i].x, 0f, occupiedTiles[i].z);

                //Check if any tiles are out of world bounds
                if (occupiedTiles[i].x < 0.5f || occupiedTiles[i].z < 0.5f)
                    return true;

                if (occupiedTiles[i].x > maxWorldBounds.x || occupiedTiles[i].z > maxWorldBounds.z)
                    return true;

                var tileX = Environment.tileCentresMap.Forward[occupiedTiles[i]].x;
                var tileY = Environment.tileCentresMap.Forward[occupiedTiles[i]].y;

                //Check if any tiles are over unbuildable/occupied tiles (e.g. water, resource, another building)
                if (!canBePlacedOnWater)
                    if (Environment.tileType[tileX, tileY] == "Water")
                        return true;

                //Debug.Log("Walkable: " + Environment.walkable[tileX, tileY] + "; Placed Building: " + Environment.buildingPlaced[tileX, tileY]);

                if (!Environment.walkable[tileX, tileY] || Environment.buildingPlaced[tileX, tileY])
                    return true;
            }

            return false;
        }
    }
}