using System.Collections.Generic;
using UnityEngine;

namespace LCHazardsOutside.Data
{
    internal class HazardCalculationContainer(System.Random random, List<GameObject> spawnDenialPoints, SpawnableMapObject spawnableMapObject, int minSpawnRate, int maxSpawnRate, int layerMask)
    {
        public System.Random Random { get; set; } = random;
        public List<GameObject> SpawnDenialPoints { get; set; } = spawnDenialPoints;
        public SpawnableMapObject SpawnableMapObject { get; set; } = spawnableMapObject;
        public int MinSpawnRate { get; set; } = minSpawnRate;
        public int MaxSpawnRate { get; set; } = maxSpawnRate;
        public bool NeedsSafetyZone { get; set; } = false;
        public float SpawnRatioMultiplier { get; set; } = 1.5f;
        public int LayerMask { get; set; } = layerMask;

    }
}
