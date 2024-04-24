using UnityEngine;

namespace LCHazardsOutside.Data
{
    public record struct SpawnPositionData
    {
        public SpawnPositionData(Vector3 centerPosition, float spawnRadius) : this()
        {
            CenterPosition = centerPosition;
            SpawnRadius = spawnRadius;
        }

        public Vector3 CenterPosition { get; set; }
        public float SpawnRadius{ get; set; }
    }
}
