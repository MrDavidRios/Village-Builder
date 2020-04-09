using System.Collections.Generic;
using UnityEngine;

namespace TerrainGeneration
{
    [ExecuteInEditMode]
    public class TerrainGenerator : MonoBehaviour
    {
        const string meshHolderName = "Terrain Mesh";

        public bool autoUpdate = true;

        public bool centralize = true;
        public int worldSize = 20;
        public float waterDepth = .2f;
        public float edgeDepth = .2f;

        float[,] falloffMap;

        [Range(0, 10)] public float centerDensity;
        [Range(0, 10)] public float closenessToEdge;

        public NoiseSettings terrainNoise;
        public Material mat;

        public Biome water;
        public Biome sand;
        public Biome grass;

        [Header("Info")]
        public int numTiles;
        public int numLandTiles;
        public int numWaterTiles;
        public float waterPercent;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        Mesh mesh;

        bool needsUpdate;

        [Header("Debug")]
        public bool viewCoastTiles;
        public bool viewWaterTiles;
        public bool viewWalkableTiles;

        private List<Vector3> dotDrawLocations;

        public bool useFalloff;
        private TerrainData debugTerrainData;

        private void Awake()
        {
            falloffMap = FalloffGenerator.GenerateFalloffMap(worldSize, centerDensity, closenessToEdge);
        }

        void Update()
        {
            if (needsUpdate && autoUpdate)
            {
                needsUpdate = false;
                Generate();
            }
            else
            {
                if (!Application.isPlaying)
                {
                    UpdateColours();
                }
            }
        }

        public TerrainData Generate()
        {
            CreateMeshComponents();

            int numTilesPerLine = Mathf.CeilToInt(worldSize);
            float min = (centralize) ? -numTilesPerLine / 2f : 0;
            float[,] map = HeightmapGenerator.GenerateHeightmap(terrainNoise, numTilesPerLine);

            string[,] hexCodes = new string[worldSize, worldSize];

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            // Some convenience stuff:
            var biomes = new Biome[] { water, sand, grass };
            Vector3[] upVectorX4 = { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            Coord[] nswe = { Coord.up, Coord.down, Coord.left, Coord.right };
            int[][] sideVertIndexByDir = { new int[] { 0, 1 }, new int[] { 3, 2 }, new int[] { 2, 0 }, new int[] { 1, 3 } };
            Vector3[] sideNormalsByDir = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

            // Terrain data:
            var terrainData = new TerrainData(numTilesPerLine);
            numLandTiles = 0;
            numWaterTiles = 0;

            var colors = new List<Color>();
            Color[] startCols = { water.startCol, sand.startCol, grass.startCol };
            Color[] endCols = { water.endCol, sand.endCol, grass.endCol };

            var waterColorSteps = new List<string>();

            for (int y = 0; y < numTilesPerLine; y++)
            {
                for (int x = 0; x < numTilesPerLine; x++)
                {
                    if (useFalloff)
                        map[x, y] = Mathf.Clamp01(map[x, y] - falloffMap[x, y]);

                    Vector2 uv = GetBiomeInfo(map[x, y], biomes);

                    bool isWaterTile = uv.x == 0f;

                    var color = Color.Lerp(startCols[(int)uv.x], endCols[(int)uv.x], uv.y);

                    string tileHexCode = ColorUtility.ToHtmlStringRGBA(color);

                    if (isWaterTile && !waterColorSteps.Contains(tileHexCode))
                        waterColorSteps.Add(tileHexCode);

                    hexCodes[x, y] = ColorUtility.ToHtmlStringRGBA(color);
                }
            }

            int neighboursOutOfBounds = 0;

            for (int y = 0; y < numTilesPerLine; y++)
            {
                for (int x = 0; x < numTilesPerLine; x++)
                {
                    Vector2 uv = GetBiomeInfo(map[x, y], biomes);

                    var color = Color.Lerp(startCols[(int)uv.x], endCols[(int)uv.x], uv.y);
                    colors.AddRange(new[] { color, color, color, color });

                    bool isWaterTile = uv.x == 0f;
                    bool isLandTile = !isWaterTile;

                    if (isWaterTile)
                        numWaterTiles++;
                    else
                        numLandTiles++;

                    // Vertices
                    int vertIndex = vertices.Count;
                    float height = isWaterTile ? -waterDepth : 0;
                    Vector3 nw = new Vector3(min + x, height, min + y + 1);
                    Vector3 ne = nw + Vector3.right;
                    Vector3 sw = nw - Vector3.forward;
                    Vector3 se = sw + Vector3.right;
                    Vector3[] tileVertices = { nw, ne, sw, se };
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
                    bool isEdgeTile = x == 0 || x == numTilesPerLine - 1 || y == 0 || y == numTilesPerLine - 1;

                    if (isLandTile || isEdgeTile)
                    {
                        for (int i = 0; i < nswe.Length; i++)
                        {
                            int neighbourX = x + nswe[i].x;
                            int neighbourY = y + nswe[i].y;
                            bool neighbourIsOutOfBounds = neighbourX < 0 || neighbourX >= numTilesPerLine || neighbourY < 0 || neighbourY >= numTilesPerLine;
                            bool neighbourIsWater = false;

                            if (neighbourIsOutOfBounds)
                                neighboursOutOfBounds++;

                            if (!neighbourIsOutOfBounds)
                            {
                                string neighbourHex = hexCodes[neighbourX, neighbourY];

                                neighbourIsWater = waterColorSteps.Contains(neighbourHex);

                                if (isEdgeTile && isLandTile && neighbourIsWater)
                                    neighbourIsWater = true;
                                else if (isEdgeTile)
                                    neighbourIsWater = false;

                                if (neighbourIsWater)
                                {
                                    terrainData.tileType[neighbourX, neighbourY] = "Shore";
                                }
                            }

                            if (neighbourIsOutOfBounds || (isLandTile && neighbourIsWater))
                            {
                                float depth = waterDepth;

                                if (neighbourIsOutOfBounds)
                                {
                                    depth = (isWaterTile) ? edgeDepth : edgeDepth + waterDepth;
                                }

                                vertIndex = vertices.Count;
                                int edgeVertIndexA = sideVertIndexByDir[i][0];
                                int edgeVertIndexB = sideVertIndexByDir[i][1];
                                vertices.Add(tileVertices[edgeVertIndexA]);
                                vertices.Add(tileVertices[edgeVertIndexA] + Vector3.down * depth);
                                vertices.Add(tileVertices[edgeVertIndexB]);
                                vertices.Add(tileVertices[edgeVertIndexB] + Vector3.down * depth);

                                colors.AddRange(new[] { color, color, color, color });

                                int[] sideTriIndices = { vertIndex, vertIndex + 1, vertIndex + 2, vertIndex + 1, vertIndex + 3, vertIndex + 2 };
                                triangles.AddRange(sideTriIndices);
                                normals.AddRange(new Vector3[] { sideNormalsByDir[i], sideNormalsByDir[i], sideNormalsByDir[i], sideNormalsByDir[i] });
                            }
                        }
                    }

                    // Terrain data:
                    terrainData.tileCentres[x, y] = nw + new Vector3(0.5f, 0, -0.5f);

                    Vector3 tileCentreForMap = new Vector3(terrainData.tileCentres[x, y].x, 0f, terrainData.tileCentres[x, y].z);
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
                            float colorDepth = uv.y;

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
            }

            // Update mesh:
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0, true);
            mesh.SetColors(colors);
            mesh.SetNormals(normals);

            meshRenderer.sharedMaterial = mat;
            UpdateColours();

            numTiles = numLandTiles + numWaterTiles;
            waterPercent = numWaterTiles / (float)numTiles;

            return terrainData;
        }

        void OnDrawGizmos()
        {
            if (viewCoastTiles || viewWalkableTiles || viewWaterTiles)
            {
                if (debugTerrainData == null)
                    debugTerrainData = Generate();

                for (int y = 0; y < debugTerrainData.size; y++)
                {
                    for (int x = 0; x < debugTerrainData.size; x++)
                    {
                        if (viewCoastTiles)
                        {
                            if (debugTerrainData.tileType[x, y] == "Shore")
                            {
                                Gizmos.color = Color.blue;
                                Gizmos.DrawCube(debugTerrainData.tileCentres[x, y], new Vector3(0.3f, 0.3f, 0.3f));
                            }
                        }

                        if (viewWalkableTiles)
                        {
                            if (Environment.walkable[x, y])
                            {
                                Gizmos.color = Color.green;
                                Gizmos.DrawSphere(debugTerrainData.tileCentres[x, y], 0.3f);
                            }
                        }

                        if (viewWaterTiles)
                        {
                            if (debugTerrainData.tileType[x, y] == "Water")
                            {
                                Gizmos.color = Color.yellow;
                                Gizmos.DrawCube(debugTerrainData.tileCentres[x, y], new Vector3(0.3f, 0.3f, 0.3f));
                            }
                        }
                    }
                }
            }
        }

        void UpdateColours()
        {
            if (mat != null)
            {
                Color[] startCols = { water.startCol, sand.startCol, grass.startCol };
                Color[] endCols = { water.endCol, sand.endCol, grass.endCol };
                mat.SetColorArray("_StartCols", startCols);
                mat.SetColorArray("_EndCols", endCols);
            }
        }

        Vector2 GetBiomeInfo(float height, Biome[] biomes)
        {
            // Find current biome
            int biomeIndex = 0;
            float biomeStartHeight = 0;

            for (int i = 0; i < biomes.Length; i++)
            {
                if (height <= biomes[i].height)
                {
                    biomeIndex = i;
                    break;
                }

                biomeStartHeight = biomes[i].height;
            }

            Biome biome = biomes[biomeIndex];
            float sampleT = Mathf.InverseLerp(biomeStartHeight, biome.height, height);
            sampleT = (int)(sampleT * biome.numSteps) / (float)Mathf.Max(biome.numSteps, 1);

            // UV stores x: biomeIndex and y: val between 0 and 1 for how close to prev/next biome
            Vector2 uv = new Vector2(biomeIndex, sampleT);
            return uv;
        }

        void CreateMeshComponents()
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
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                meshFilter.sharedMesh = mesh;
            }
            else
            {
                mesh = meshFilter.sharedMesh;
                mesh.Clear();
            }

            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        void OnValidate()
        {
            needsUpdate = true;

            falloffMap = FalloffGenerator.GenerateFalloffMap(worldSize, centerDensity, closenessToEdge);
        }

        [System.Serializable]
        public class Biome
        {
            [Range(0, 1)]
            public float height;
            public Color startCol;
            public Color endCol;
            public int numSteps;
        }

        public class TerrainData
        {
            public int size;
            public Vector3[,] tileCentres;
            public string[,] tileType;
            public bool[,] walkable;
            public int[,] fertility;
            public Vector2[,] uvs;
            //public Map<string, string> tileCentresMap;
            public Map<Vector3, Vector2Int> tileCentresMap;

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