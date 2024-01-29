using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using UnityEngine.UIElements;

namespace LCHazardsOutside.Patches {

    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch {

        private const string LAYER_NAME = "MapHazards";

        [HarmonyPatch(nameof(RoundManager.SpawnOutsideHazards))]
        [HarmonyPrefix]
        static void SpawnHazardsOutsidePatch(RoundManager __instance) {
            if (__instance.currentLevel.sceneName == "CompanyBuilding" || !__instance.IsServer || !__instance.IsHost) {
                return;
            }

            // Tie hazard spawn to seed.
            LCHazardsOutside.GetLogger().LogDebug("randomMapSeed: " + StartOfRound.Instance.randomMapSeed);
            System.Random random = new(StartOfRound.Instance.randomMapSeed + 587);
            List<GameObject> spawnDenialPoints = [.. GameObject.FindGameObjectsWithTag("SpawnDenialPoint")];
            List<Transform> shipPathPoints = [.. __instance.shipSpawnPathPoints];

            LCHazardsOutside.GetLogger().LogDebug("Getting spawnable objects..");
            List<SpawnableMapObject> hazardObjects = [.. __instance.currentLevel.spawnableMapObjects];

            if (hazardObjects.Any()) {
                for (int i = 0; i < hazardObjects.Count; i++)
                {
                    SpawnableMapObject hazardObject = hazardObjects[i];
                    bool isIncreasedMapHazardSpawn = __instance.increasedMapHazardSpawnRateIndex == i;

                    LCHazardsOutside.GetLogger().LogDebug("Current spawnable object: " + hazardObject.prefabToSpawn.ToString());

                    bool isTurret = hazardObject.prefabToSpawn.GetComponentInChildren<Turret>() != null;
                    bool isLandmine = hazardObject.prefabToSpawn.GetComponentInChildren<Landmine>() != null;

                    LCHazardsOutside.GetLogger().LogDebug("Is Turret: " + isTurret);
                    LCHazardsOutside.GetLogger().LogDebug("Is Landmine: " + isLandmine);

                    if (isLandmine && LCHazardsOutside.instance.configEnableLandmine.Value)
                    {
                        LCHazardsOutside.GetLogger().LogInfo("Spawning landmines outside...");
                        int configMinSpawnRate = LCHazardsOutside.instance.configLandmineMinSpawnRate.Value;
                        int configMaxSpawnRate = LCHazardsOutside.instance.configLandmineMaxSpawnRate.Value;

                        determineMinMaxSpawnRates(isIncreasedMapHazardSpawn, configMinSpawnRate, configMaxSpawnRate, out int minSpawnRate, out int maxSpawnRate);
                        HazardCalculationContainer landmineContainer = new(random, spawnDenialPoints, hazardObject, minSpawnRate, maxSpawnRate);

                        CalculateHazardSpawn(__instance, landmineContainer);
                    }  

                    if (isTurret && LCHazardsOutside.instance.configEnableTurret.Value)
                    {
                        LCHazardsOutside.GetLogger().LogInfo("Spawning turrets outside...");
                        int configMinSpawnRate = LCHazardsOutside.instance.configTurretMinSpawnRate.Value;
                        int configMaxSpawnRate = LCHazardsOutside.instance.configTurretMaxSpawnRate.Value;

                        determineMinMaxSpawnRates(false, configMinSpawnRate, configMaxSpawnRate, out int minSpawnRate, out int maxSpawnRate);
                        HazardCalculationContainer turretContainer = new(random, spawnDenialPoints, hazardObject, minSpawnRate, maxSpawnRate)
                        {
                            NeedsSafetyZone = true,
                            SpawnRatioMultiplier = 2f
                        };

                        CalculateHazardSpawn(__instance, turretContainer);   
                    }
                          
                    // Unknown Hazard
                    if (!isLandmine && !isTurret && LCHazardsOutside.instance.configEnableCustomHazard.Value)
                    {
                        LCHazardsOutside.GetLogger().LogInfo("Spawning custom hazards outside...");
                        int configMinSpawnRate = LCHazardsOutside.instance.configCustomHazardMinSpawnRate.Value;
                        int configMaxSpawnRate = LCHazardsOutside.instance.configCustomHazardMaxSpawnRate.Value;

                        determineMinMaxSpawnRates(isIncreasedMapHazardSpawn, configMinSpawnRate, configMaxSpawnRate, out int minSpawnRate, out int maxSpawnRate);
                        HazardCalculationContainer customContainer = new(random, spawnDenialPoints, hazardObject, 1, 1);

                        CalculateHazardSpawn(__instance, customContainer);
                    }
                }
            }

            LCHazardsOutside.GetLogger().LogInfo("Outside hazard spawning done.");
        }

        private static void determineMinMaxSpawnRates(bool increasedMapHazardSpawnRate, int configMinSpawnRate, int configMaxSpawnRate, out int minSpawnRate, out int maxSpawnRate)
        {
            // Range from 0 to max config.
            minSpawnRate = Mathf.Min(Mathf.Max(configMinSpawnRate, 0), configMaxSpawnRate);
            // Range from min to 50.
            maxSpawnRate = Mathf.Max(Mathf.Min(configMaxSpawnRate, 50), minSpawnRate);

            if (increasedMapHazardSpawnRate)
            {
                minSpawnRate = Mathf.Max(5, minSpawnRate);
                maxSpawnRate = Mathf.Min(maxSpawnRate * 2, 15);
            }
        }

        private static void CalculateHazardSpawn(RoundManager __instance, HazardCalculationContainer hazardCalculationContainer)
        {
            Transform[] shipSpawnPathPoints = __instance.shipSpawnPathPoints;

            int hazardCounter = 0;

            // This is where the ship actually lands.
            Transform shipLandingTransform = shipSpawnPathPoints.Last();

            int actualSpawnRate = hazardCalculationContainer.Random.Next(hazardCalculationContainer.MinSpawnRate, hazardCalculationContainer.MaxSpawnRate);
            LCHazardsOutside.GetLogger().LogDebug("Actual spawn rate: " + actualSpawnRate);

            List<Vector3> safetyPositions = new(2);

            Vector3 shipLandPosition = shipLandingTransform.position;
            Vector3 mainEntrancePosition = RoundManager.FindMainEntrancePosition(false, true);
            safetyPositions.Add(shipLandPosition);
            LCHazardsOutside.GetLogger().LogDebug("Ship spawn point: " + shipLandPosition);

            CalculateCenterPosition(shipLandPosition, mainEntrancePosition, hazardCalculationContainer.SpawnRatioMultiplier, out Vector3 middlePoint, out float spawnRadius);

            for (int j = 0; j < actualSpawnRate; j++)
            {
                System.Random random = hazardCalculationContainer.Random;
                (Vector3 randomPosition, Quaternion quaternion) = SpawnHelper.GetRandomGroundPosition(middlePoint, spawnRadius, hazardCalculationContainer.Random, -1);

                if (randomPosition == middlePoint)
                {
                    LCHazardsOutside.GetLogger().LogDebug("No NavMesh hit!");
                    continue;
                }

                bool invalidSpawnPointFound = IsInvalidSpawnPointHighSafety(hazardCalculationContainer.SpawnDenialPoints, shipSpawnPathPoints, randomPosition, safetyPositions, hazardCalculationContainer.NeedsSafetyZone);

                // Do not spawn the hazard if it's too close to a spawn denial point.
                if (invalidSpawnPointFound)
                {
                    LCHazardsOutside.GetLogger().LogDebug("Hazard was too close to denial or safety zone and was therefore deleted: " + randomPosition);
                    continue;
                }

                SpawnHazard(__instance, random, hazardCalculationContainer.SpawnableMapObject, randomPosition, quaternion);

                hazardCounter++;
            }

            LCHazardsOutside.GetLogger().LogDebug("Total hazard amount: " + hazardCounter);
        }

        private static void SpawnHazard(RoundManager __instance, System.Random random, SpawnableMapObject spawnableMapObject, Vector3 position, Quaternion quaternion)
        {
            LCHazardsOutside.GetLogger().LogDebug("Spawn hazard outside at: " + position);
            GameObject gameObject = UnityEngine.Object.Instantiate(spawnableMapObject.prefabToSpawn, position, quaternion, __instance.mapPropsContainer.transform);

            if (spawnableMapObject.spawnFacingAwayFromWall)
            {
                gameObject.transform.eulerAngles = new Vector3(0f, __instance.YRotationThatFacesTheFarthestFromPosition(position + Vector3.up * 0.2f), 0f);
            }
            else
            {
                gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, random.Next(0, 360), gameObject.transform.eulerAngles.z);
            }

            gameObject.SetActive(value: true);
            gameObject.layer = LayerMask.NameToLayer(LAYER_NAME);
            gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
        }

        private static bool IsInvalidSpawnPoint(List<GameObject> spawnDenialPoints, Transform[] shipPathPoints, Vector3 randomPosition)
        {
            bool invalidSpawnPointFound = false;
            foreach (GameObject spawnDenialObject in spawnDenialPoints)
            {
                if (Vector3.Distance(randomPosition, spawnDenialObject.transform.position) < 4f)
                {
                    invalidSpawnPointFound = true;
                    break;
                }
            }

            foreach (Transform shipPathTransform in shipPathPoints)
            {
                if (Vector3.Distance(shipPathTransform.position, randomPosition) < 6f)
                {
                    invalidSpawnPointFound = true;
                    break;
                }
            }

            return invalidSpawnPointFound;
        }

        private static bool IsInvalidSpawnPointHighSafety(List<GameObject> spawnDenialPoints, Transform[] shipPathPoints, Vector3 position, List<Vector3> safetyZones, bool needsSafetyZone)
        {
            if (IsInvalidSpawnPoint(spawnDenialPoints, shipPathPoints, position))
            {
                return true;
            }

            if (!needsSafetyZone)
            {
                return false;
            }

            foreach (Vector3 safetyPosition in safetyZones)
            {
                if (Vector3.Distance(position, safetyPosition) <= 20f)
                {
                    return true;
                }
            }

            return false;
        }

        private static void CalculateCenterPosition(Vector3 shipLandPosition, Vector3 mainEntrancePosition, float spawnRadiusMultiplier, out Vector3 centerPosition, out float spawnRadius)
        {
            centerPosition = (shipLandPosition + mainEntrancePosition) / 2;
            spawnRadius = Vector3.Distance(mainEntrancePosition, centerPosition) * spawnRadiusMultiplier;
        }
    }
}
