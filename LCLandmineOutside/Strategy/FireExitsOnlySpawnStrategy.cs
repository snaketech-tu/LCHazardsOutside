using LCHazardsOutside.Data;
using System.Collections.Generic;
using UnityEngine;

namespace LCHazardsOutside.Strategy
{
    internal class FireExitsOnlySpawnStrategy : Abstract.SpawnStrategy
    {
        private static FireExitsOnlySpawnStrategy instance;

        private FireExitsOnlySpawnStrategy() { }

        public static FireExitsOnlySpawnStrategy GetInstance()
        {
            instance ??= new FireExitsOnlySpawnStrategy();
            return instance;
        }

        public override List<SpawnPositionData> CalculateCenterPositions(Vector3 shipLandPosition, Vector3 mainEntrancePosition, List<Vector3> pointsOfInterest, float spawnRadiusMultiplier)
        {
            List<SpawnPositionData> centerPositionData = [];

            foreach (Vector3 pointOfInterest in pointsOfInterest)
            {
                centerPositionData.Add(CalculateCenterWithSpawnRadius(shipLandPosition, pointOfInterest, spawnRadiusMultiplier));
            }

            return centerPositionData;
        }


    }
}
