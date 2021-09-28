using System;
using System.Collections.Generic;
using DavidRios.Terrain;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terrain
{
    [ExecuteInEditMode]
    public class TerrainGenerator : MonoBehaviour
    {
        private const string meshHolderName = "Terrain Mesh";

        public bool autoUpdate = true;

        public bool centralize = true;
        public int worldSize = 20;
        public float waterDepth = .2f;
        public float edgeDepth = .2f;

        [Range(0, 10)] public float centerDensity;
        [Range(0, 10)] public float closenessToEdge;

        public NoiseSettings terrainNoise;
        public Material mat;

        public Biome water;
        public Biome sand;
        public Biome grass;

        [Header("Info")] public int numTiles;

        public int numLandTiles;
        public int numWaterTiles;
        public float waterPercent;

        [Header("Debug")] public bool viewCoastTiles;

        public bool viewWaterTiles;
        public bool viewWalkableTiles;

        public bool useFalloff;
        private TerrainData debugTerrainData;

        private List<Vector3> dotDrawLocations;

        private float[,] falloffMap;
        private Mesh mesh;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        private bool needsUpdate;

        private void Awake()
        {
            falloffMap = FalloffGenerator.GenerateFalloffMap(worldSize, centerDensity, closenessToEdge);
        }

        private void Update()
        {
            if (needsUpdate && autoUpdate)
            {
                needsUpdate = false;
                Generate();
            }
            else
            {
                if (!Application.isPlaying) UpdateColours();
            }
        }

        private void OnDrawGizmos()
        {
            if (viewCoastTiles || viewWalkableTiles || viewWaterTiles)
            {
                if (debugTerrainData == null)
                    debugTerrainData = Generate();

                for (var y = 0; y < debugTerrainData.size; y++)
                for (var x = 0; x < debugTerrainData.size; x++)
                {
                    if (viewCoastTiles)
                        if (debugTerrainData.tileType[x, y] == "Shore")
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawCube(debugTerrainData.tileCentres[x, y], new Vector3(0.3f, 0.3f, 0.3f));
                        }

                    if (viewWalkableTiles)
                        if (Environment.walkable[x, y])
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawSphere(debugTerrainData.tileCentres[x, y], 0.3f);
                        }

                    if (viewWaterTiles)
                        if (debugTerrainData.tileType[x, y] == "Water")
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawCube(debugTerrainData.tileCentres[x, y], new Vector3(0.3f, 0.3f, 0.3f));
                        }
                }
            }
        }

        private void OnValidate()
        {
            needsUpdate = true;

            falloffMap = FalloffGenerator.GenerateFalloffMap(worldSize, centerDensity, closenessToEdge);
        }

        public TerrainData Generate()
        {
            CreateMeshComponents();

            var numTilesPerLine = Mathf.CeilToInt(worldSize);
            var min = centralize ? -numTilesPerLine / 2f : 0;
            var map = HeightmapGenerator.GenerateHeightmap(terrainNoise, numTilesPerLine);

            var hexCodes = new string[worldSize, worldSize];

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            // Some convenience stuff:
            var biomes = new[] {water, sand, grass};
            Vector3[] upVectorX4 = {Vector3.up, Vector3.up, Vector3.up, Vector3.up};
            Coord[] nswe = {Coord.up, Coord.down, Coord.left, Coord.right};
            int[][] sideVertIndexByDir = {new[] {0, 1}, new[] {3, 2}, new[] {2, 0}, new[] {1, 3}};
            Vector3[] sideNormalsByDir = {Vector3.forward, Vector3.back, Vector3.left, Vector3.right};

            // Terrain data:
            var terrainData = new TerrainData(numTilesPerLine);
            numLandTiles = 0;
            numWaterTiles = 0;

            var colors = new List<Color>();
            Color[] startCols = {water.startCol, sand.startCol, grass.startCol};
            Color[] endCols = {water.endCol, sand.endCol, grass.endCol};

            var waterColorSteps = new List<string>();

            for (var y = 0; y < numTilesPerLine; y++)
            for (var x = 0; x < numTilesPerLine; x++)
            {
                if (useFalloff)
                    map[x, y] = Mathf.Clamp01(map[x, y] - falloffMap[x, y]);

                var uv = GetBiomeInfo(map[x, y], biomes);

                var isWaterTile = uv.x == 0f;

                var color = Color.Lerp(startCols[(int) uv.x], endCols[(int) uv.x], uv.y);

                var tileHexCode = ColorUtility.ToHtmlStringRGBA(color);

                if (isWaterTile && !waterColorSteps.Contains(tileHexCode))
                    waterColorSteps.Add(tileHexCode);

                hexCodes[x, y] = ColorUtility.ToHtmlStringRGBA(color);
            }

            var neighboursOutOfBounds = 0;

            for (var y = 0; y < numTilesPerLine; y++)
            for (var x = 0; x < numTilesPerLine; x++)
            {
                var uv = GetBiomeInfo(map[x, y], biomes);

                var color = Color.Lerp(startCols[(int) uv.x], endCols[(int) uv.x], uv.y);
                colors.AddRange(new[] {color, color, color, color});

                var isWaterTile = uv.x == 0f;
                var isLandTile = !isWaterTile;

                if (isWaterTile)
                    numWaterTiles++;
                else
                    numLandTiles++;

                // Vertices
                var vertIndex = vertices.Count;
                var height = isWaterTile ? -waterDepth : 0;
                var nw = new Vector3(min + x, height, min + y + 1);
                var ne = nw + Vector3.right;
                var sw = nw - Vector3.forward;
                var se = sw + Vector3.right;
                Vector3[] tileVertices = {nw, ne, sw, se};
                vertices.AddRange(tileVertices);
                normals.AddRange(upVectorX4);

                // Add triangles
                triangles.Add(vertIndex);
                triangles.Add(vertIndex + 1);
                triangles.Add(vertIndex + 2);
                triangles.Add(vertIndex + 1);
                triangles.Add(vertIndex + 3);
                triangles.Add(vertIndex + 2);

                // Bridge gaps between water and land tiles
                var isEdgeTile = x == 0 || x == numTilesPerLine - 1 || y == 0 || y == numTilesPerLine - 1;

                if (isLandTile || isEdgeTile)
                    for (var i = 0; i < nswe.Length; i++)
                    {
                        var neighbourX = x + nswe[i].x;
                        var neighbourY = y + nswe[i].y;
                        var neighbourIsOutOfBounds = neighbourX < 0 || neighbourX >= numTilesPerLine ||
                                                     neighbourY < 0 || neighbourY >= numTilesPerLine;
                        var neighbourIsWater = false;

                        if (neighbourIsOutOfBounds)
                            neighboursOutOfBounds++;

                        if (!neighbourIsOutOfBounds)
                        {
                            var neighbourHex = hexCodes[neighbourX, neighbourY];

                            neighbourIsWater = waterColorSteps.Contains(neighbourHex);

                            if (isEdgeTile && isLandTile && neighbourIsWater)
                                neighbourIsWater = true;
                            else if (isEdgeTile)
                                neighbourIsWater = false;

                            if (neighbourIsWater) terrainData.tileType[neighbourX, neighbourY] = "Shore";
                        }

                        if (neighbourIsOutOfBounds || isLandTile && neighbourIsWater)
                        {
                            var depth = waterDepth;

                            if (neighbourIsOutOfBounds) depth = isWaterTile ? edgeDepth : edgeDepth + waterDepth;

                            vertIndex = vertices.Count;
                            var edgeVertIndexA = sideVertIndexByDir[i][0];
                            var edgeVertIndexB = sideVertIndexByDir[i][1];
                            vertices.Add(tileVertices[edgeVertIndexA]);
                            vertices.Add(tileVertices[edgeVertIndexA] + Vector3.down * depth);
                            vertices.Add(tileVertices[edgeVertIndexB]);
                            vertices.Add(tileVertices[edgeVertIndexB] + Vector3.down * depth);

                            colors.AddRange(new[] {color, color, color, color});

                            int[] sideTriIndices =
                                {vertIndex, vertIndex + 1, vertIndex + 2, vertIndex + 1, vertIndex + 3, vertIndex + 2};
                            triangles.AddRange(sideTriIndices);
                            normals.AddRange(new[]
                                {sideNormalsByDir[i], sideNormalsByDir[i], sideNormalsByDir[i], sideNormalsByDir[i]});
                        }
                    }

                // Terrain data:
                terrainData.tileCentres[x, y] = nw + new Vector3(0.5f, 0, -0.5f);

                var tileCentreForMap =
                    new Vector3(terrainData.tileCentres[x, y].x, 0f, terrainData.tileCentres[x, y].z);
                terrainData.tileCentresMap.Add(tileCentreForMap, new Vector2Int(x, y));

                terrainData.walkable[x, y] = isLandTile;

                //UV: x = 2.0 is grass, x = 1.0 is sand

                /* Grass:
                     * y = 0.0-0.3 is light grass (Fertility: 1)
                     * y = 0.3-0.5 is medium dark grass (Fertility: 2)
                     * y = 0.5+ is dark grass (Fertility: 3)
                    */
                /* Sand:
                     * Fertility will always be 0
                    */

                terrainData.uvs[x, y] = uv;

                if (isLandTile)
                {
                    //If the tile is sand, set its fertility to 0 and return
                    if (uv.x == 1f)
                    {
                        terrainData.fertility[x, y] = 0;
                        terrainData.tileType[x, y] = "Sand";
                    }
                    else
                    {
                        var colorDepth = uv.y;

                        if (colorDepth <= 0.2f)
                            terrainData.fertility[x, y] = 1;
                        else if (colorDepth > 0.2f && colorDepth <= 0.5f)
                            terrainData.fertility[x, y] = 2;
                        else if (colorDepth > 0.5f)
                            terrainData.fertility[x, y] = 3;

                        terrainData.tileType[x, y] = "Grass";
                    }
                }
                else
                {
                    terrainData.fertility[x, y] = -1;

                    if (terrainData.tileType[x, y] != "Shore")
                        terrainData.tileType[x, y] = "Water";
                }
            }

            // Update mesh:
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0, true);
            mesh.SetColors(colors);
            mesh.SetNormals(normals);

            meshRenderer.sharedMaterial = mat;
            UpdateColours();

            numTiles = numLandTiles + numWaterTiles;
            waterPercent = numWaterTiles / (float) numTiles;

            return terrainData;
        }

        private void UpdateColours()
        {
            if (mat != null)
            {
                Color[] startCols = {water.startCol, sand.startCol, grass.startCol};
                Color[] endCols = {water.endCol, sand.endCol, grass.endCol};
                mat.SetColorArray("_StartCols", startCols);
                mat.SetColorArray("_EndCols", endCols);
            }
        }

        private Vector2 GetBiomeInfo(float height, Biome[] biomes)
        {
            // Find current biome
            var biomeIndex = 0;
            float biomeStartHeight = 0;

            for (var i = 0; i < biomes.Length; i++)
            {
                if (height <= biomes[i].height)
                {
                    biomeIndex = i;
                    break;
                }

                biomeStartHeight = biomes[i].height;
            }

            var biome = biomes[biomeIndex];
            var sampleT = Mathf.InverseLerp(biomeStartHeight, biome.height, height);
            sampleT = (int) (sampleT * biome.numSteps) / (float) Mathf.Max(biome.numSteps, 1);

            // UV stores x: biomeIndex and y: val between 0 and 1 for how close to prev/next biome
            var uv = new Vector2(biomeIndex, sampleT);
            return uv;
        }

        private void CreateMeshComponents()
        {
            GameObject holder = null;

            if (meshFilter == null)
            {
                if (GameObject.Find(meshHolderName))
                {
                    holder = GameObject.Find(meshHolderName);
                }
                else
                {
                    holder = new GameObject(meshHolderName);
                    holder.AddComponent<MeshRenderer>();
                    holder.AddComponent<MeshFilter>();
                }

                meshFilter = holder.GetComponent<MeshFilter>();
                meshRenderer = holder.GetComponent<MeshRenderer>();
            }

            if (meshFilter.sharedMesh == null)
            {
                mesh = new Mesh();
                mesh.indexFormat = IndexFormat.UInt32;
                meshFilter.sharedMesh = mesh;
            }
            else
            {
                mesh = meshFilter.sharedMesh;
                mesh.Clear();
            }

            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        [Serializable]
        public class Biome
        {
            [Range(0, 1)] public float height;

            public Color startCol;
            public Color endCol;
            public int numSteps;
        }

        public class TerrainData
        {
            public int[,] fertility;
            public int size;

            public Vector3[,] tileCentres;

            //public Map<string, string> tileCentresMap;
            public Map<Vector3, Vector2Int> tileCentresMap;
            public string[,] tileType;
            public Vector2[,] uvs;
            public bool[,] walkable;

            public TerrainData(int size)
            {
                this.size = size;
                tileCentres = new Vector3[size, size];
                tileType = new string[size, size];
                walkable = new bool[size, size];
                fertility = new int[size, size];
                uvs = new Vector2[size, size];
                //tileCentresMap = new Map<string, string>();
                tileCentresMap = new Map<Vector3, Vector2Int>();
            }
        }
    }
}