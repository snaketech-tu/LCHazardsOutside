using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.SendMouseEvents;

namespace LCHazardsOutside
{
    internal class SpawnHelper
    {

        public static (Vector3, Quaternion) GetRandomGroundPosition(Vector3 centerPoint, float radius = 10f, System.Random randomSeed = null, int layerMask = -1, int maxAttempts = 10)
        {
            float y = centerPoint.y;
            float x, y2, z;

            // Geometry layer
            int roomLayerMask = 1 << LayerMask.NameToLayer("Room");

            Vector3 randomPosition;

            for (int i = 0; i < maxAttempts; i++)
            {
                x = RandomNumberInRadius(radius, randomSeed);
                y2 = RandomNumberInRadius(radius, randomSeed);
                z = RandomNumberInRadius(radius, randomSeed);
                randomPosition = centerPoint + new Vector3(x, y2, z);
                randomPosition.y = y;

                float maxDistance = Vector3.Distance(centerPoint, randomPosition) + 50f;

                if (NavMesh.SamplePosition(randomPosition, out NavMeshHit navMeshHit, maxDistance, layerMask))
                {
                    if (Physics.Raycast(randomPosition + Vector3.up, Vector3.down, out RaycastHit groundHit, 1000f, roomLayerMask))
                    {
                        float slopeAngle = Vector3.Angle(groundHit.point, Vector3.up);
                        LCHazardsOutside.GetLogger().LogDebug($"Nav hit at: {navMeshHit.position} and angle of {slopeAngle}");

                        return (groundHit.point, Quaternion.FromToRotation(Vector3.up, groundHit.normal));
                    } else
                    {
                        LCHazardsOutside.GetLogger().LogDebug($"Nav hit at: {navMeshHit.position} but ray cast failed.");
                    }
                }
            }

            return (centerPoint, Quaternion.identity);
        }

        private static float RandomNumberInRadius(float radius, System.Random randomSeed)
        {
            return ((float)randomSeed.NextDouble() - 0.5f) * radius;
        }

    }
}
