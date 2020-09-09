using System;
using UnityEngine;

namespace Terrain
{
    [Serializable]
    public class NoiseSettings
    {
        public int seed;

        [Range(1, 8)] public int numLayers = 4;

        [Range(0, 1)] public float persistance = 0.5f;

        public float lacunarity = 2;
        public float scale = 1;
        public Vector2 offset;
    }
}