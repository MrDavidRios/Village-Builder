﻿using UnityEngine;

namespace DavidRios.Terrain
{
    public static class FalloffGenerator
    {
        public static float[,] GenerateFalloffMap(int size, float a = 3.0f, float b = 2.2f)
        {
            var map = new float[size, size];

            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
            {
                var x = i / (float) size * 2 - 1;
                var y = j / (float) size * 2 - 1;

                var value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value, a, b);
            }

            return map;
        }

        private static float Evaluate(float value, float a = 3.0f, float b = 2.2f)
        {
            return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
        }
    }
}