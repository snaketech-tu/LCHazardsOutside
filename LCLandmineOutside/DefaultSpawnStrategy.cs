using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LCHazardsOutside
{
    internal class DefaultSpawnStrategy : Abstract.SpawnStrategy
    {

        private static float RandomNumberInRadius(float radius, System.Random randomSeed)
        {
            return ((float)randomSeed.NextDouble() - 0.5f) * radius;
        }
        public override (Vector3, Quaternion) GetRandomGroundPositionAndRotation(Vector3 centerPoint, float radius = 10f, System.Random randomSeed = null, int layerMask = -1, int maxAttempts = 10)
        {
            float y = centerPoint.y;
            float x, y2, z;

            Vector3 randomPosition;

            for (int i = 0; i < maxAttempts; i++)
            {
                x = RandomNumberInRadius(radius, randomSeed);
                y2 = RandomNumberInRadius(radius, randomSeed);
                z = RandomNumberInRadius(radius, randomSeed);
                randomPosition = centerPoint + new Vector3(x, y2, z);
                randomPosition.y = y;

                float maxDistance = Vector3.Distance(centerPoint, randomPosition) + 30f;

                if (NavMesh.SamplePosition(randomPosition, out NavMeshHit navMeshHit, maxDistance, -1))
                {
                    if (Physics.Raycast(navMeshHit.position + Vector3.up, Vector3.down, out RaycastHit groundHit, 50f, layerMask))
                    {
                        return (groundHit.point + Vector3.up * 0.1f, Quaternion.FromToRotation(Vector3.up, groundHit.normal));
                    } else
                    {
                        Plugin.GetLogger().LogDebug($"Nav hit at: {navMeshHit.position} but ray cast failed.");
                    }
                }
            }

            return (Vector3.zero, Quaternion.identity);
        }
        public override void CalculateCenterPosition(Vector3 shipLandPosition, Vector3 mainEntrancePosition, List<Vector3> pointsOfInterest, float spawnRadiusMultiplier, out Vector3 centerPosition, out float spawnRadius)
        {
            centerPosition = (shipLandPosition + mainEntrancePosition) / 2;
            spawnRadius = Vector3.Distance(mainEntrancePosition, centerPosition) * spawnRadiusMultiplier;
            centerPosition.y = Mathf.Max(shipLandPosition.y, mainEntrancePosition.y);
        }
    }
}
