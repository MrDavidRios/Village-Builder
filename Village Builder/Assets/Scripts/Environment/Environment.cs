using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Pathfinding;
using Terrain;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace DavidRios.Environment
{
    public class Environment : MonoBehaviour
    {
        // Cached data:
        public static Vector3[,] TileCentres;
        public static string[,] TileType;
        public static bool[,] Walkable;
        public static bool[,] BuildingPlaced;
        public static int[,] Fertility;
        public static Vector2[,] Uvs;
        public static Map<Vector3, Vector2Int> TileCentresMap;

        public static bool[,] FishingTile;

        private static int _size;
        private static Coord[,][] _walkableNeighboursMap;
        private static List<Coord> _walkableCoords;

        //Array of visible tiles from any tile; value is Coord.invalid if no visible water tile
        private static Coord[,] _closestVisibleWaterMap;

        private static Random _prng;
        private static TerrainGenerator.TerrainData _terrainData;
        public int seed;

        public bool meshCombination;

        [Header("Trees")] public GameObject treePrefab;

        public GameObject treeContainer;

        public Color lightestLeafColor;
        public Color darkestLeafColor;

        [Range(0, 1)] public float treePlacementProbability;
        [Range(0, 5)] public float treeDensityCoefficient;

        public int amountOfTreesPerSector;

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
            _prng = new Random();

            Init();
        }

        private static Coord GetNextTileRandom(Coord current)
        {
            var neighbours = _walkableNeighboursMap[current.x, current.y];
            return neighbours.Length == 0 ? current : neighbours[_prng.Next(neighbours.Length)];
        }

        /// Get random neighbour tile, weighted towards those in similar direction as currently facing
        public static Coord GetNextTileWeighted(Coord current, Coord previous, double forwardProbability = 0.2,
            int weightingIterations = 3)
        {
            if (current == previous) return GetNextTileRandom(current);

            var forwardOffset = current - previous;
            // Random chance of returning foward tile (if walkable)
            if (_prng.NextDouble() < forwardProbability)
            {
                var forwardCoord = current + forwardOffset;

                if (forwardCoord.x >= 0 && forwardCoord.x < _size && forwardCoord.y >= 0 && forwardCoord.y < _size)
                    if (Walkable[forwardCoord.x, forwardCoord.y])
                        return forwardCoord;
            }

            // Get walkable neighbours
            var neighbours = _walkableNeighboursMap[current.x, current.y];
            if (neighbours.Length == 0) return current;

            // From n random tiles, pick the one that is most aligned with the forward direction:
            var forwardDir = new Vector2(forwardOffset.x, forwardOffset.y).normalized;
            var bestScore = float.MinValue;
            var bestNeighbour = current;

            for (var i = 0; i < weightingIterations; i++)
            {
                var neighbour = neighbours[_prng.Next(neighbours.Length)];
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
            _terrainData = terrainGenerator.Generate();

            TileCentres = _terrainData.tileCentres;
            TileType = _terrainData.tileType;
            Walkable = _terrainData.walkable;
            Fertility = _terrainData.fertility;
            _size = _terrainData.size;
            Uvs = _terrainData.uvs;
            TileCentresMap = _terrainData.tileCentresMap;

            FishingTile = new bool[_terrainData.size, _terrainData.size];
            BuildingPlaced = new bool[_terrainData.size, _terrainData.size];

            SpawnTrees();
            SpawnRocks();
            GenerateFishingTiles();

            _terrainData.walkable = Walkable;
            _walkableNeighboursMap = new Coord[_size, _size][];

            // Find and store all walkable neighbours for each walkable tile on the map
            for (var y = 0; y < _terrainData.size; y++)
            for (var x = 0; x < _terrainData.size; x++)
            {
                //No buildings have been placed, since this land is newly generated.
                BuildingPlaced[x, y] = false;

                if (Walkable[x, y])
                {
                    var walkableNeighbours = new List<Coord>();
                    for (var offsetY = -1; offsetY <= 1; offsetY++)
                    for (var offsetX = -1; offsetX <= 1; offsetX++)
                        if (offsetX != 0 || offsetY != 0)
                        {
                            var neighbourX = x + offsetX;
                            var neighbourY = y + offsetY;
                            if (neighbourX >= 0 && neighbourX < _size && neighbourY >= 0 && neighbourY < _size)
                                if (Walkable[neighbourX, neighbourY])
                                    walkableNeighbours.Add(new Coord(neighbourX, neighbourY));
                        }

                    _walkableNeighboursMap[x, y] = walkableNeighbours.ToArray();
                }
            }

            ModifyWalkableTiles();
            
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

                    node.Walkable = _terrainData.walkable[x, z];
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
            _walkableCoords = new List<Coord>();

            var sectorTreeAmount = 0;
            var currentSectorNumber = 1;
        
            var currentSector = new GameObject().transform;
            currentSector.parent = treeContainer.transform;
            currentSector.name = "Sector_1";
        
            for (var y = 0; y < _terrainData.size; y++)
            for (var x = 0; x < _terrainData.size; x++)
                if (Walkable[x, y])
                {
                    var respectiveTreePlacementProbability =
                        treePlacementProbability * Mathf.Pow(Fertility[x, y], treeDensityCoefficient);

                    //Has to comply with probablility, but cannot spawn on sand due to its infertility
                    if (_prng.NextDouble() < respectiveTreePlacementProbability)
                    {
                        // Randomize rot/scale
                        var rotX = Mathf.Lerp(-maxRot, maxRot, (float) spawnPrng.NextDouble());
                        var rotZ = Mathf.Lerp(-maxRot, maxRot, (float) spawnPrng.NextDouble());
                        var rotY = (float) spawnPrng.NextDouble() * 360f;
                        var rot = new Vector3(rotX, rotY, rotZ);

                        var scaleDeviation = ((float) spawnPrng.NextDouble() * 2 - 1) * maxScaleDeviation;

                        if (Fertility[x, y] == 1 && scaleDeviation > 0f)
                            scaleDeviation = -scaleDeviation;

                        if (Fertility[x, y] > 1 && scaleDeviation < 0f)
                            scaleDeviation = Mathf.Abs(scaleDeviation);

                        if (Fertility[x, y] > 2f)
                            scaleDeviation *= 2f;

                        var scale = 1 + scaleDeviation;

                        sectorTreeAmount++;
                    
                        if (sectorTreeAmount > amountOfTreesPerSector)
                        {                        
                            sectorTreeAmount = 0;
                            currentSectorNumber++;
                        
                            currentSector = new GameObject().transform;
                            currentSector.parent = treeContainer.transform;
                            currentSector.name = $"Sector_{currentSectorNumber}";
                        }

                        // Spawn
                        var tree = Instantiate(treePrefab, currentSector, true);
                        var treeMesh = tree.transform.GetChild(0).GetComponent<MeshRenderer>();

                        //Initialize
                        tree.transform.position = TileCentres[x, y];
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
                        Walkable[x, y] = false;
                    }
                    else
                    {
                        _walkableCoords.Add(new Coord(x, y));
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
            _walkableCoords = new List<Coord>();

            for (var y = 0; y < _terrainData.size; y++)
            for (var x = 0; x < _terrainData.size; x++)
                if (Walkable[x, y])
                    if (_prng.NextDouble() < rockPlacementProbability)
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

                        var rock = Instantiate(rockPrefabs[rockPrefabIndex], TileCentres[x, y], rot);

                        //Initialize
                        var rockTransform = rock.transform;

                        rockTransform.parent = rockHolder;
                        rockTransform.localScale = Vector3.one * scale;

                        rock.gameObject.layer = LayerMask.NameToLayer("Resource");
                        rock.tag = "Stone";
                        rock.name = rock.tag;

                        // Mark tile as unwalkable
                        Walkable[x, y] = false;
                    }
                    else
                        _walkableCoords.Add(new Coord(x, y));
        }

        private void GenerateFishingTiles()
        {
            var spawnPrng = new Random(seed);
            var fishingTileHolder = fishingTileContainer.transform;

            for (var y = 0; y < _terrainData.size; y++)
            for (var x = 0; x < _terrainData.size; x++)
                if (TileType[x, y] == "Shore")
                {
                    if (_prng.NextDouble() < fishingTilePlacementProbability)
                    {
                        // Randomize rot/scale
                        var rotY = (float) spawnPrng.NextDouble() * 360f;
                        var rot = Quaternion.Euler(-90f, rotY, 0f);

                        FishingTile[x, y] = true;

                        var fishingTileObject = Instantiate(fishingTilePrefab, TileCentres[x, y], rot);

                        fishingTileObject.transform.parent = fishingTileHolder;
                        fishingTileObject.transform.localScale = new Vector3(30, 30, 30);
                        fishingTileObject.material = fishingTileMaterial;

                        //Make the gameobject static
                        fishingTileObject.gameObject.isStatic = true;

                        fishingTileObject.transform.position = TileCentres[x, y];
                    }
                    else
                        FishingTile[x, y] = false;
                }
                else
                    FishingTile[x, y] = false;
        }

        public static void RemoveTree(Transform treeToRemove)
        {
            for (var y = 0; y < Walkable.GetLength(0); y++)
            for (var x = 0; x < Walkable.GetLength(1); x++)
                if (treeToRemove.position == TileCentres[x, y])
                {
                    Destroy(treeToRemove.gameObject);

                    Walkable[x, y] = true;
                    ModifyWalkableTiles(x, y);
                }
        }
    }
}