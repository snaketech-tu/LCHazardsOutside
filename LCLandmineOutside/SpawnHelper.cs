using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace LCHazardsOutside
{
    internal class SpawnHelper
    {

        public static Vector3 GetRandomGroundPosition(Vector3 middlePoint, float radius = 10f, System.Random randomSeed = null, int layerMask = -1, int maxAttempts = 10)
        {
            float y = middlePoint.y;
            float x, y2, z;
            
            Vector3 randomPosition;

            for (int i = 0; i < maxAttempts; i++)
            {
                x = RandomNumberInRadius(radius, randomSeed);
                y2 = RandomNumberInRadius(radius, randomSeed);
                z = RandomNumberInRadius(radius, randomSeed);
                randomPosition = middlePoint + new Vector3(x, y2, z);
                randomPosition.y = y;

                if (NavMesh.SamplePosition(randomPosition, out NavMeshHit navHit, Vector3.Distance(middlePoint, randomPosition) + 50f, layerMask))
                {
                    return navHit.position;
                }
            }

            return middlePoint; // Return the original position if no valid position is found after maxAttempts
        }

        private static float RandomNumberInRadius(float radius, System.Random randomSeed)
        {
            return ((float)randomSeed.NextDouble() - 0.5f) * radius;
        }

    }
}
