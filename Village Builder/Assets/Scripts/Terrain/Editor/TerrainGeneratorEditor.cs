using UnityEditor;
using UnityEngine;

namespace Terrain
{
    [CustomEditor(typeof(TerrainGenerator))]
    public class TerrainGeneratorEditor : Editor
    {
        private TerrainGenerator terrainGen;

        private void OnEnable()
        {
            terrainGen = (TerrainGenerator) target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Refresh")) terrainGen.Generate();
        }
    }
}