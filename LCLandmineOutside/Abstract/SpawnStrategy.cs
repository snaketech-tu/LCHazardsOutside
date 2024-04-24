using LCHazardsOutside.Data;
using System.Collections.Generic;
using UnityEngine;

namespace LCHazardsOutside.Abstract
{
    public abstract class SpawnStrategy
    {
        public abstract List<SpawnPositionData> CalculateCenterPositions(Vector3 shipLandPosition, Vector3 mainEntrancePosition, List<Vector3> pointsOfInterest, float spawnRadiusMultiplier);

        protected SpawnPositionData CalculateCenterWithSpawnRadius(Vector3 shipLandPosition, Vector3 targetPosition, float spawnRadiusMultiplier)
        {
            float spawnRadius;
            Vector3 centerPosition = (shipLandPosition + targetPosition) / 2;
            spawnRadius = Vector3.Distance(targetPosition, centerPosition) * spawnRadiusMultiplier;
            centerPosition.y = Mathf.Max(shipLandPosition.y, targetPosition.y);

            return new SpawnPositionData(centerPosition, spawnRadius);
        }
    }
}
