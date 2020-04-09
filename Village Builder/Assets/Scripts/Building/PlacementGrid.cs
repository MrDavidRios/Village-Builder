using TerrainGeneration;
using UnityEngine;

[ExecuteInEditMode]
public class PlacementGrid : MonoBehaviour
{
   [Range (1,8)]
    public int gridCellSize = 2;

    public bool display;

    private float width = 1f;

    private float depth = 1f;

    [Header("Extra Settings")]
    public bool centerInTiles;

    //Scripts
    private TerrainGenerator terrainGenerator;

    private void Start()
    {
        //The terrainGenerator script is stored in a variable so the call to the FindObjectOfType<>() method isn't done more than once; it could hinder performance
        //This is called caching
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
    }

    private void Update()
    {
        if (!PlaceBuilding._placingBuilding)
            centerInTiles = true;
    }

    //This function returns the nearest building placement point on the ground based on a given position
    public Vector3 GetNearestPointOnGrid(Vector3 position)
    {
        //Width is the size of the world, measured by tiles (20 = 20 tiles). Width and Depth form a 2D plane of axes X and Z; width is X, depth is Z 
        width = terrainGenerator.worldSize;
        depth = width;

        //x, y, and zCount are all the amount of placement points per axis (calculated based on the desired size of the grid cells and position on axis)
        int xCount = Mathf.RoundToInt(position.x * 2) / 2 / gridCellSize;
        int yCount = Mathf.RoundToInt(position.y * 2) / 2 / gridCellSize;
        int zCount = Mathf.RoundToInt(position.z * 2) / 2 / gridCellSize;

        //The 'result' variable is the position of the grid point
        Vector3 result = new Vector3(
            (float)xCount * gridCellSize,
            (float)yCount * gridCellSize,
            (float)zCount * gridCellSize);

        //If the terrain is centered (the center of the terrain is (0, 0, 0)) center the grid; if not, then match the position of the terrain gameObject
        if (terrainGenerator.centralize)
            result += transform.position - new Vector3(terrainGenerator.worldSize / 2f, 0f, terrainGenerator.worldSize / 2f);
        else
            result += transform.position;

        //The variable 'centerInTiles' dictates whether or not the points are centered in the tiles; if not they're positioned on the tiles' vertices.
        if (centerInTiles)
            result += new Vector3(gridCellSize / 2f, 0f, gridCellSize / 2f);

        return result;
    }

    public Vector3 GetCenteredPoint(Vector3 point, bool centered)
    {
        if (centered)
            return point -= new Vector3(gridCellSize / 2f, 0f, gridCellSize / 2f);
        else
            return point += new Vector3(gridCellSize / 2f, 0f, gridCellSize / 2f);
    }

    //OnDrawGizmos is the function called by the Editor; this basically draws spheres depicting the positions of the placement points.
    //Its purpose is to confirm that the grid is positioned correctly.
    private void OnDrawGizmos()
    {
        if (display)
        {
            Gizmos.color = Color.red;
            for (float x = 0; x < width; x += gridCellSize)
            {
                for (float z = 0; z < depth; z += gridCellSize)
                {
                    var point = GetNearestPointOnGrid(new Vector3(x, 0f, z));
                    Gizmos.DrawSphere(point, 0.1f);
                }
            }
        }
    }
}
