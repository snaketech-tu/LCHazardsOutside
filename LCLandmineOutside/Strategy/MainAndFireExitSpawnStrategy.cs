using LCHazardsOutside.Data;
using System.Collections.Generic;
using UnityEngine;

namespace LCHazardsOutside.Strategy
{
    internal class MainAndFireExitSpawnStrategy : Abstract.SpawnStrategy
    {
        private static MainAndFireExitSpawnStrategy instance;

        private MainAndFireExitSpawnStrategy() { }

        public static MainAndFireExitSpawnStrategy GetInstance()
        {
            instance ??= new MainAndFireExitSpawnStrategy();
            return instance;
        }

        public override List<SpawnPositionData> CalculateCenterPositions(Vector3 shipLandPosition, Vector3 mainEntrancePosition, List<Vector3> pointsOfInterest, float spawnRadiusMultiplier)
        {
            List<SpawnPositionData> centerPositionData = [CalculateCenterWithSpawnRadius(shipLandPosition, mainEntrancePosition, spawnRadiusMultiplier)];

            foreach (Vector3 pointOfInterest in pointsOfInterest)
            {
                centerPositionData.Add(CalculateCenterWithSpawnRadius(shipLandPosition, pointOfInterest, spawnRadiusMultiplier));
            }

            return centerPositionData;
        }


    }
}
