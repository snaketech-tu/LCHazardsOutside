using System.Collections.Generic;
using UnityEngine;

namespace LCHazardsOutside.Abstract
{
    internal abstract class SpawnStrategy
    {
        public abstract void CalculateCenterPosition(Vector3 shipLandPosition, Vector3 mainEntrancePosition, List<Vector3> pointsOfInterest, float spawnRadiusMultiplier, out Vector3 centerPosition, out float spawnRadius);

        public abstract (Vector3, Quaternion) GetRandomGroundPositionAndRotation(Vector3 centerPoint, float radius = 10f, System.Random randomSeed = null, int layerMask = -1, int maxAttempts = 10);
    }
}
