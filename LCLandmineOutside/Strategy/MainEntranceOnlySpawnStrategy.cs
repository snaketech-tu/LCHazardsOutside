using LCHazardsOutside.Data;
using System.Collections.Generic;
using UnityEngine;

namespace LCHazardsOutside.Strategy
{
    internal class MainEntranceOnlySpawnStrategy : Abstract.SpawnStrategy
    {
        private static MainEntranceOnlySpawnStrategy instance;

        private MainEntranceOnlySpawnStrategy() { }

        public static MainEntranceOnlySpawnStrategy GetInstance()
        {
            instance ??= new MainEntranceOnlySpawnStrategy();
            return instance;
        }

        public override List<SpawnPositionData> CalculateCenterPositions(Vector3 shipLandPosition, Vector3 mainEntrancePosition, List<Vector3> pointsOfInterest, float spawnRadiusMultiplier)
        {
            SpawnPositionData data = CalculateCenterWithSpawnRadius(shipLandPosition, mainEntrancePosition, spawnRadiusMultiplier);

            return [data];
        }
    }
}
