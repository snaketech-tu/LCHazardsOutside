using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using LCHazardsOutside.Abstract;
using Dissonance.Integrations.Unity_NFGO;
using System.Collections;
using System;

namespace LCHazardsOutside.Patches {

    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch {

        private const string HAZARD_LAYER_NAME = "MapHazards";

        [HarmonyPatch(nameof(RoundManager.SpawnOutsideHazards))]
        [HarmonyPrefix]
        static void SpawnHazardsOutsidePatch(RoundManager __instance) {
            if (!__instance.currentLevel.spawnEnemiesAndScrap || !__instance.IsServer || !__instance.IsHost) {
                return;
            }

            // Tie hazard spawn to seed.
            Plugin.GetLogger().LogDebug("randomMapSeed: " + StartOfRound.Instance.randomMapSeed);
            System.Random random = new(StartOfRound.Instance.randomMapSeed + 587);

            int noHazardSpawnChance = Plugin.instance.noHazardSpawnChance.Value;
            if (noHazardSpawnChance > 0)
            {
                double chance = noHazardSpawnChance / 100.0;

                if (random.NextDouble() < chance)
                {
                    Plugin.GetLogger().LogDebug("No hazards spawned outside due to global chance.");
                    return;
                }
            }

            string sanitizedPlanetName = LCUtils.GetNumberlessPlanetName(__instance.currentLevel);
            Plugin.GetLogger().LogDebug($"Planetname: {sanitizedPlanetName}");

            List<GameObject> spawnDenialPoints = [.. GameObject.FindGameObjectsWithTag("SpawnDenialPoint")];
            List<SpawnableMapObject> hazardObjects = [.. __instance.currentLevel.spawnableMapObjects];

            if (hazardObjects.Any()) {
                for (int i = 0; i < hazardObjects.Count; i++)
                {
                    SpawnableMapObject hazardObject = hazardObjects[i];
                    bool isIncreasedMapHazardSpawn = __instance.increasedMapHazardSpawnRateIndex == i;

                    Plugin.GetLogger().LogDebug("Current spawnable object: " + hazardObject.prefabToSpawn);

                    bool isTurret = hazardObject.prefabToSpawn.GetComponentInChildren<Turret>() != null;
                    bool isLandmine = hazardObject.prefabToSpawn.GetComponentInChildren<Landmine>() != null;

                    if (isLandmine && Plugin.instance.enableLandmine.Value)
                    {
                        Plugin.GetLogger().LogInfo("Spawning landmines outside...");
                        int configMinSpawnRate = Plugin.instance.globalLandmineMinSpawnRate.Value;
                        int configMaxSpawnRate = Plugin.instance.globalLandmineMaxSpawnRate.Value;
                        Plugin.instance.landmineMoonMap.TryGetValue(sanitizedPlanetName, out MoonMinMax moonMinMax);

                        DetermineMinMaxSpawnRates(isIncreasedMapHazardSpawn, configMinSpawnRate, configMaxSpawnRate, moonMinMax, out int minSpawnRate, out int maxSpawnRate);
                        HazardCalculationContainer landmineContainer = new(random, spawnDenialPoints, hazardObject, minSpawnRate, maxSpawnRate);

                        CalculateHazardSpawn(__instance, landmineContainer, new DefaultSpawnStrategy());
                    }  

                    if (isTurret && Plugin.instance.enableTurret.Value)
                    {
                        Plugin.GetLogger().LogInfo("Spawning turrets outside...");
                        int configMinSpawnRate = Plugin.instance.globalTurretMinSpawnRate.Value;
                        int configMaxSpawnRate = Plugin.instance.globalTurretMaxSpawnRate.Value;
                        Plugin.instance.turretMoonMap.TryGetValue(sanitizedPlanetName, out MoonMinMax moonMinMax);

                        DetermineMinMaxSpawnRates(false, configMinSpawnRate, configMaxSpawnRate, moonMinMax, out int minSpawnRate, out int maxSpawnRate);
                        HazardCalculationContainer turretContainer = new(random, spawnDenialPoints, hazardObject, minSpawnRate, maxSpawnRate)
                        {
                            NeedsSafetyZone = true,
                            SpawnRatioMultiplier = 2f
                        };

                        CalculateHazardSpawn(__instance, turretContainer, new DefaultSpawnStrategy());   
                    }
                          
                    // Unknown Hazard
                    if (!isLandmine && !isTurret && Plugin.instance.enableCustomHazard.Value)
                    {
                        Plugin.GetLogger().LogInfo("Spawning custom hazards outside...");
                        int configMinSpawnRate = Plugin.instance.globalCustomHazardMinSpawnRate.Value;
                        int configMaxSpawnRate = Plugin.instance.globalCustomHazardMaxSpawnRate.Value;
                        Plugin.instance.customHazardMoonMap.TryGetValue(sanitizedPlanetName, out MoonMinMax moonMinMax);

                        DetermineMinMaxSpawnRates(isIncreasedMapHazardSpawn, configMinSpawnRate, configMaxSpawnRate, moonMinMax, out int minSpawnRate, out int maxSpawnRate);
                        HazardCalculationContainer customContainer = new(random, spawnDenialPoints, hazardObject, 1, 1);

                        CalculateHazardSpawn(__instance, customContainer, new DefaultSpawnStrategy());
                    }
                }
            }

            Plugin.GetLogger().LogInfo("Outside hazard spawning done.");
        }

        private static void DetermineMinMaxSpawnRates(bool increasedMapHazardSpawnRate, int configMinSpawnRate, int configMaxSpawnRate, MoonMinMax moonMinMax, out int minSpawnRate, out int maxSpawnRate)
        {
            int effectiveUserMinValue = moonMinMax != null ? moonMinMax.min : configMinSpawnRate;
            int effectiveUserMaxValue = moonMinMax != null ? moonMinMax.max : configMaxSpawnRate;

            // Range from 0 to max config.
            minSpawnRate = Mathf.Min(Mathf.Max(effectiveUserMinValue, 0), effectiveUserMaxValue);
            // Range from min to 50.
            maxSpawnRate = Mathf.Max(Mathf.Min(effectiveUserMaxValue, 50), minSpawnRate);

            if (increasedMapHazardSpawnRate)
            {
                minSpawnRate = Mathf.Max(5, minSpawnRate);
                maxSpawnRate = Mathf.Min(maxSpawnRate * 2, 15);
            }
        }

        private static void CalculateHazardSpawn(RoundManager __instance, HazardCalculationContainer hazardCalculationContainer, SpawnStrategy spawnStrategy)
        {
            Transform[] shipSpawnPathPoints = __instance.shipSpawnPathPoints;

            int hazardCounter = 0;

            // This is where the ship actually lands.
            Transform shipLandingTransform = shipSpawnPathPoints.Last();

            int actualSpawnRate = hazardCalculationContainer.Random.Next(hazardCalculationContainer.MinSpawnRate, hazardCalculationContainer.MaxSpawnRate);
            Plugin.GetLogger().LogDebug("Actual spawn rate: " + actualSpawnRate);

            List<Vector3> safetyPositions = new(2);

            Vector3 shipLandPosition = shipLandingTransform.position;
            Vector3 mainEntrancePosition = RoundManager.FindMainEntrancePosition(false, true);

            safetyPositions.Add(shipLandPosition);
            Plugin.GetLogger().LogDebug("Ship spawn point: " + shipLandPosition);

            spawnStrategy.CalculateCenterPosition(shipLandPosition, mainEntrancePosition, [], hazardCalculationContainer.SpawnRatioMultiplier, out Vector3 middlePoint, out float spawnRadius);

            List<GameObject> gameObjects = [];

            int roomLayerMask = LayerMask.GetMask("Room");
            int moddedMoonLayerMask = LayerMask.GetMask("Room", "Default");
            int effectiveLayerMask = LCUtils.IsVanillaMoon(__instance.currentLevel) ? roomLayerMask : moddedMoonLayerMask;

            for (int j = 0; j < actualSpawnRate; j++)
            {
                System.Random random = hazardCalculationContainer.Random;
                (Vector3 randomPosition, Quaternion quaternion) = spawnStrategy.GetRandomGroundPositionAndRotation(middlePoint, spawnRadius, hazardCalculationContainer.Random, effectiveLayerMask);

                if (randomPosition == Vector3.zero)
                {
                    Plugin.GetLogger().LogDebug("No NavMesh hit!");
                    continue;
                }

                bool invalidSpawnPointFound = IsInvalidSpawnPointHighSafety(hazardCalculationContainer.SpawnDenialPoints, shipSpawnPathPoints, randomPosition, safetyPositions, hazardCalculationContainer.NeedsSafetyZone);

                // Do not spawn the hazard if it's too close to a spawn denial point.
                if (invalidSpawnPointFound)
                {
                    Plugin.GetLogger().LogDebug("Hazard was too close to denial or safety zone and was therefore deleted: " + randomPosition);
                    continue;
                }

                gameObjects.Add(InstantiateHazardObject(__instance, random, hazardCalculationContainer.SpawnableMapObject, randomPosition, quaternion));

                hazardCounter++;
            }

            __instance.StartCoroutine(SpawnHazardsInBulk(gameObjects));

            Plugin.GetLogger().LogDebug("Total hazard amount: " + hazardCounter);
        }

        private static GameObject InstantiateHazardObject(RoundManager __instance, System.Random random, SpawnableMapObject spawnableMapObject, Vector3 position, Quaternion quaternion)
        {
            Plugin.GetLogger().LogDebug("Spawn hazard outside at: " + position);
            GameObject gameObject = UnityEngine.Object.Instantiate(spawnableMapObject.prefabToSpawn, position, quaternion, __instance.mapPropsContainer.transform);

            if (spawnableMapObject.spawnFacingAwayFromWall)
            {
                gameObject.transform.eulerAngles = new Vector3(0f, __instance.YRotationThatFacesTheFarthestFromPosition(position + Vector3.up * 0.2f), 0f);
            }

            gameObject.SetActive(value: true);
            gameObject.layer = LayerMask.NameToLayer(HAZARD_LAYER_NAME);
            //gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);

            return gameObject;
        }

        private static bool IsInvalidSpawnPoint(List<GameObject> spawnDenialPoints, Transform[] shipPathPoints, Vector3 randomPosition)
        {
            foreach (GameObject spawnDenialObject in spawnDenialPoints)
            {
                if (Vector3.Distance(randomPosition, spawnDenialObject.transform.position) < 4f)
                {
                    return true;
                }
            }

            foreach (Transform shipPathTransform in shipPathPoints)
            {
                if (Vector3.Distance(shipPathTransform.position, randomPosition) < 6f)
                {
                    return true;
                }
            }

            return false;
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
                if (Vector3.Distance(position, safetyPosition) <= 16f)
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerator SpawnHazardsInBulk(List<GameObject> gameObjects)
        {
            yield return new WaitWhile(() => Plugin.instance.IsCoroutineRunning);

            Plugin.instance.IsCoroutineRunning = true;
            Plugin.GetLogger().LogDebug($"SpawnHazardsInBulk Coroutine running.");
            const int bulkSize = 10;
            for (int i = 0; i < gameObjects.Count; i += bulkSize)
            {
                int range = Mathf.Min(bulkSize, gameObjects.Count - i);
                for (int j = 0; j < range; j++)
                {
                    GameObject objectToSpawn = gameObjects[i + j];
                    try
                    {
                        objectToSpawn.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                    }
                    catch (Exception e)
                    {
                        Plugin.GetLogger().LogError($"NetworkObject could not be spawned: {e}");
                    }
                }

                yield return new WaitForSeconds(0.5f);
            }

            Plugin.instance.IsCoroutineRunning = false;
            Plugin.GetLogger().LogDebug($"SpawnHazardsInBulk Coroutine done.");
        }
    }
}
