using System.Collections.Generic;
using System.Diagnostics;
using Pathfinding;
using Terrain;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public class Environment : MonoBehaviour
{
    // Cached data:
    public static Vector3[,] tileCentres;
    public static string[,] tileType;
    public static bool[,] walkable;
    public static bool[,] buildingPlaced;
    public static int[,] fertility;
    public static Vector2[,] uvs;
    public static Map<Vector3, Vector2Int> tileCentresMap;

    public static bool[,] fishingTile;

    private static int size;
    private static Coord[,][] walkableNeighboursMap;
    private static List<Coord> walkableCoords;

    //Array of visible tiles from any tile; value is Coord.invalid if no visible water tile
    private static Coord[,] closestVisibleWaterMap;

    private static Random prng;
    private static TerrainGenerator.TerrainData terrainData;
    public int seed;

    [Header("Trees")] public GameObject treePrefab;

    public GameObject treeContainer;

    public Color lightestLeafColor;
    public Color darkestLeafColor;

    [Range(0, 1)] public float treePlacementProbability;
    [Range(0, 5)] public float treeDensityCoefficient;

    [Header("Rocks")] public MeshRenderer[] rockPrefabs;

    public GameObject rockContainer;

    public Material stoneMaterial;

    [Range(0, 1)] public float rockPlacementProbability;

    [Header("Fishing Tiles")] public MeshRenderer fishingTilePrefab;

    public GameObject fishingTileContainer;

    public Material fishingTileMaterial;

    [Range(0, 1)] public float fishingTilePlacementProbability;

    [Header("Debug")] public bool showMapDebug;

    public bool debugInitTime;
    public Transform mapCoordTransform;
    public float mapViewDst;

    private void Start()
    {
        prng = new Random();

        Init();
    }

    public static Coord GetNextTileRandom(Coord current)
    {
        var neighbours = walkableNeighboursMap[current.x, current.y];
        if (neighbours.Length == 0) return current;
        return neighbours[prng.Next(neighbours.Length)];
    }

    /// Get random neighbour tile, weighted towards those in similar direction as currently facing
    public static Coord GetNextTileWeighted(Coord current, Coord previous, double forwardProbability = 0.2,
        int weightingIterations = 3)
    {
        if (current == previous) return GetNextTileRandom(current);

        var forwardOffset = current - previous;
        // Random chance of returning foward tile (if walkable)
        if (prng.NextDouble() < forwardProbability)
        {
            var forwardCoord = current + forwardOffset;

            if (forwardCoord.x >= 0 && forwardCoord.x < size && forwardCoord.y >= 0 && forwardCoord.y < size)
                if (walkable[forwardCoord.x, forwardCoord.y])
                    return forwardCoord;
        }

        // Get walkable neighbours
        var neighbours = walkableNeighboursMap[current.x, current.y];
        if (neighbours.Length == 0) return current;

        // From n random tiles, pick the one that is most aligned with the forward direction:
        var forwardDir = new Vector2(forwardOffset.x, forwardOffset.y).normalized;
        var bestScore = float.MinValue;
        var bestNeighbour = current;

        for (var i = 0; i < weightingIterations; i++)
        {
            var neighbour = neighbours[prng.Next(neighbours.Length)];
            Vector2 offset = neighbour - current;
            var score = Vector2.Dot(offset.normalized, forwardDir);
            if (score > bestScore)
            {
                bestScore = score;
                bestNeighbour = neighbour;
            }
        }

        return bestNeighbour;
    }

    // Call terrain generator and cache useful info
    private void Init()
    {
        var sw = Stopwatch.StartNew();

        var terrainGenerator = FindObjectOfType<TerrainGenerator>();
        terrainData = terrainGenerator.Generate();

        tileCentres = terrainData.tileCentres;
        tileType = terrainData.tileType;
        walkable = terrainData.walkable;
        fertility = terrainData.fertility;
        size = terrainData.size;
        uvs = terrainData.uvs;
        tileCentresMap = terrainData.tileCentresMap;

        fishingTile = new bool[terrainData.size, terrainData.size];
        buildingPlaced = new bool[terrainData.size, terrainData.size];

        SpawnTrees();
        SpawnRocks();
        GenerateFishingTiles();

        terrainData.walkable = walkable;
        walkableNeighboursMap = new Coord[size, size][];

        // Find and store all walkable neighbours for each walkable tile on the map
        for (var y = 0; y < terrainData.size; y++)
        for (var x = 0; x < terrainData.size; x++)
        {
            //No buildings have been placed, since this land is newly generated.
            buildingPlaced[x, y] = false;

            if (walkable[x, y])
            {
                var walkableNeighbours = new List<Coord>();
                for (var offsetY = -1; offsetY <= 1; offsetY++)
                for (var offsetX = -1; offsetX <= 1; offsetX++)
                    if (offsetX != 0 || offsetY != 0)
                    {
                        var neighbourX = x + offsetX;
                        var neighbourY = y + offsetY;
                        if (neighbourX >= 0 && neighbourX < size && neighbourY >= 0 && neighbourY < size)
                            if (walkable[neighbourX, neighbourY])
                                walkableNeighbours.Add(new Coord(neighbourX, neighbourY));
                    }

                walkableNeighboursMap[x, y] = walkableNeighbours.ToArray();
            }
        }

        ModifyWalkableTiles();

        #region Useless Code

        /*
        // Generate offsets within max view distance, sorted by distance ascending
        // Used to speed up per-tile search for closest water tile
        List<Coord> viewOffsets = new List<Coord>();
        int viewRadius = Animal.maxViewDistance;
        int sqrViewRadius = viewRadius * viewRadius;
        for (int offsetY = -viewRadius; offsetY <= viewRadius; offsetY++)
        {
            for (int offsetX = -viewRadius; offsetX <= viewRadius; offsetX++)
            {
                int sqrOffsetDst = offsetX * offsetX + offsetY * offsetY;
                if ((offsetX != 0 || offsetY != 0) && sqrOffsetDst <= sqrViewRadius)
                {
                    viewOffsets.Add(new Coord(offsetX, offsetY));
                }
            }
        }
        viewOffsets.Sort((a, b) => (a.x * a.x + a.y * a.y).CompareTo(b.x * b.x + b.y * b.y));
        Coord[] viewOffsetsArr = viewOffsets.ToArray();

        // Find closest accessible water tile for each tile on the map:
        closestVisibleWaterMap = new Coord[size, size];
        for (int y = 0; y < terrainData.size; y++)
        {
            for (int x = 0; x < terrainData.size; x++)
            {
                bool foundWater = false;
                if (walkable[x, y])
                {
                    for (int i = 0; i < viewOffsets.Count; i++)
                    {
                        int targetX = x + viewOffsetsArr[i].x;
                        int targetY = y + viewOffsetsArr[i].y;
                        if (targetX >= 0 && targetX < size && targetY >= 0 && targetY < size)
                        {
                            if (terrainData.tileType[targetX, targetY] == "Shore")
                            {
                                if (EnvironmentUtility.TileIsVisibile(x, y, targetX, targetY))
                                {
                                    closestVisibleWaterMap[x, y] = new Coord(targetX, targetY);
                                    foundWater = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (!foundWater)
                {
                    closestVisibleWaterMap[x, y] = Coord.invalid;
                }
            }
        }
        */

        #endregion

        if (debugInitTime)
            Debug.Log("Init time: " + sw.ElapsedMilliseconds);
    }

    /// <summary>
    ///     Updates all the tiles in the GridGraph or only one selected tile.
    /// </summary>
    /// <param name="xIndex"></param>
    /// <param name="zIndex"></param>
    /// <param name="walkable"></param>
    public static void ModifyWalkableTiles(int xIndex = -1, int zIndex = -1, bool walkable = true)
    {
        var gg = AstarPath.active.data.gridGraph;

        if (xIndex == -1 && zIndex == -1)
        {
            for (var z = 0; z < gg.depth; z++)
            for (var x = 0; x < gg.width; x++)
            {
                var node = gg.GetNode(x, z);

                node.Walkable = terrainData.walkable[x, z];
            }

            // Recalculate all grid connections
            // This is required because we have updated the walkability of some nodes
            gg.GetNodes(node => gg.CalculateConnections((GridNodeBase) node));
        }
        else if (xIndex != -1 && zIndex != -1)
        {
            AstarPath.active.AddWorkItem(ctx =>
            {
                var grid = AstarPath.active.data.gridGraph;

                grid.GetNode(xIndex, zIndex).Walkable = false;
                grid.CalculateConnectionsForCellAndNeighbours(xIndex, zIndex);
            });
        }

        // If you are only updating one or a few nodes you may want to use
        // gg.CalculateConnectionsForCellAndNeighbours only on those nodes instead for performance.
    }

    private void SpawnTrees()
    {
        // Settings:
        float maxRot = 4;
        var maxScaleDeviation = .2f;

        var spawnPrng = new Random(seed);
        var treeHolder = treeContainer.transform;
        walkableCoords = new List<Coord>();

        for (var y = 0; y < terrainData.size; y++)
        for (var x = 0; x < terrainData.size; x++)
            if (walkable[x, y])
            {
                var respectiveTreePlacementProbability =
                    treePlacementProbability * Mathf.Pow(fertility[x, y], treeDensityCoefficient);

                //Has to comply with probablility, but cannot spawn on sand due to its infertility
                if (prng.NextDouble() < respectiveTreePlacementProbability)
                {
                    // Randomize rot/scale
                    var rotX = Mathf.Lerp(-maxRot, maxRot, (float) spawnPrng.NextDouble());
                    var rotZ = Mathf.Lerp(-maxRot, maxRot, (float) spawnPrng.NextDouble());
                    var rotY = (float) spawnPrng.NextDouble() * 360f;
                    var rot = new Vector3(rotX, rotY, rotZ);

                    var scaleDeviation = ((float) spawnPrng.NextDouble() * 2 - 1) * maxScaleDeviation;

                    if (fertility[x, y] == 1 && scaleDeviation > 0f)
                        scaleDeviation = -scaleDeviation;

                    if (fertility[x, y] > 1 && scaleDeviation < 0f)
                        scaleDeviation = Mathf.Abs(scaleDeviation);

                    if (fertility[x, y] > 2f)
                        scaleDeviation *= 2f;

                    var scale = 1 + scaleDeviation;

                    // Spawn
                    var tree = Instantiate(treePrefab);
                    var treeMesh = tree.transform.GetChild(0).GetComponent<MeshRenderer>();

                    //Initialize
                    tree.transform.parent = treeHolder;
                    tree.transform.position = tileCentres[x, y];
                    tree.transform.localScale = Vector3.one * scale;
                    tree.GetComponent<Animator>().enabled = false;
                    tree.name = tree.tag;

                    //Set the color of the tree's 'Leaves' material based on its scale.

                    //Scale Percentage
                    var scalePercentage = Mathf.Abs(scaleDeviation) / (maxScaleDeviation * 2);

                    //Debug.Log(scaleDeviation + "; " + scalePercentage);

                    treeMesh.materials[1].SetColor("_MainColor",
                        Color.Lerp(lightestLeafColor, darkestLeafColor, scalePercentage));

                    treeMesh.transform.eulerAngles = rot;

                    //Mark tile as unwalkable
                    walkable[x, y] = false;
                }
                else
                {
                    walkableCoords.Add(new Coord(x, y));
                }
            }
    }

    private void SpawnRocks()
    {
        // Settings:
        float maxRot = 4;
        var maxScaleDeviation = .2f;
        var startingScale = 0.65f;
        var colVariationFactor = 0.15f;
        var minCol = .8f;

        var spawnPrng = new Random(seed);
        var rockHolder = rockContainer.transform;
        walkableCoords = new List<Coord>();

        for (var y = 0; y < terrainData.size; y++)
        for (var x = 0; x < terrainData.size; x++)
            if (walkable[x, y])
            {
                if (prng.NextDouble() < rockPlacementProbability)
                {
                    // Randomize rot/scale
                    var rotX = Mathf.Lerp(-maxRot, maxRot, (float) spawnPrng.NextDouble());
                    var rotZ = Mathf.Lerp(-maxRot, maxRot, (float) spawnPrng.NextDouble());
                    var rotY = (float) spawnPrng.NextDouble() * 360f;
                    var rot = Quaternion.Euler(rotX, rotY, rotZ);
                    var scale = startingScale + ((float) spawnPrng.NextDouble() * 2 - 1) * maxScaleDeviation;

                    // Randomize colour
                    var col = Mathf.Lerp(minCol, 1, (float) spawnPrng.NextDouble());
                    var r = col + ((float) spawnPrng.NextDouble() * 2 - 1) * colVariationFactor;
                    var g = col + ((float) spawnPrng.NextDouble() * 2 - 1) * colVariationFactor;
                    var b = col + ((float) spawnPrng.NextDouble() * 2 - 1) * colVariationFactor;

                    // Spawn
                    var rockPrefabIndex = Mathf.RoundToInt(UnityEngine.Random.Range(0, rockPrefabs.Length));

                    var rock = Instantiate(rockPrefabs[rockPrefabIndex], tileCentres[x, y], rot);

                    //Initialize
                    rock.transform.parent = rockHolder;
                    rock.transform.localScale = Vector3.one * scale;
                    //rock.material = stoneMaterial;
                    //rock.material.color = new Color(r, g, b);
                    rock.gameObject.layer = LayerMask.NameToLayer("Resource");
                    rock.tag = "Stone";
                    rock.name = rock.tag;

                    // Mark tile unwalkable
                    walkable[x, y] = false;
                }
                else
                {
                    walkableCoords.Add(new Coord(x, y));
                }
            }
    }

    private void GenerateFishingTiles()
    {
        var spawnPrng = new Random(seed);
        var fishingTileHolder = fishingTileContainer.transform;

        for (var y = 0; y < terrainData.size; y++)
        for (var x = 0; x < terrainData.size; x++)
            if (tileType[x, y] == "Shore")
            {
                if (prng.NextDouble() < fishingTilePlacementProbability)
                {
                    // Randomize rot/scale
                    var rotY = (float) spawnPrng.NextDouble() * 360f;
                    var rot = Quaternion.Euler(-90f, rotY, 0f);

                    fishingTile[x, y] = true;

                    var fishingTileObject = Instantiate(fishingTilePrefab, tileCentres[x, y], rot);

                    fishingTileObject.transform.parent = fishingTileHolder;
                    fishingTileObject.transform.localScale = new Vector3(30, 30, 30);
                    fishingTileObject.material = fishingTileMaterial;

                    //Make the gameobject static
                    fishingTileObject.gameObject.isStatic = true;

                    fishingTileObject.transform.position = tileCentres[x, y];
                }
                else
                {
                    fishingTile[x, y] = false;
                }
            }
            else
            {
                fishingTile[x, y] = false;
            }
    }

    public static void RemoveTree(Transform treeToRemove)
    {
        for (var y = 0; y < walkable.GetLength(0); y++)
        for (var x = 0; x < walkable.GetLength(1); x++)
            if (treeToRemove.position == tileCentres[x, y])
            {
                Destroy(treeToRemove.gameObject);

                walkable[x, y] = true;
                ModifyWalkableTiles(x, y);
            }
    }
}