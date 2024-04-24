using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using LCHazardsOutside.Abstract;
using System.Collections;
using System;
using LCHazardsOutside.Data;
using System.Runtime.CompilerServices;

namespace LCHazardsOutside.Patches
{

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
                    Plugin.GetLogger().LogInfo("No hazards spawned outside due to global chance.");
                    return;
                }
            }

            SpawnableMapObject[] hazardObjects = __instance.currentLevel.spawnableMapObjects;

            // This map has no hazards so skip outside hazard spawning.
            if (hazardObjects.Length == 0)
            {
                return;
            }

            __instance.StartCoroutine(SpawnHazardsAfterExitSpawn(__instance, hazardObjects, random));
        }

        private static void SpawnHazardsOutside(RoundManager __instance, SpawnableMapObject[] hazardObjects, EntranceContainer entranceContainer, System.Random random)
        {
            string sanitizedPlanetName = LCUtils.GetNumberlessMoonName(__instance.currentLevel);
            Plugin.GetLogger().LogDebug($"Planetname: {sanitizedPlanetName}");
            
            LCUtils.CUSTOM_LAYER_MASK.TryGetValue(sanitizedPlanetName, out string[] layers);
            layers ??= ["Room", "Default"];

            int layerMask = LayerMask.GetMask(layers);

            List<GameObject> spawnDenialPoints = [.. GameObject.FindGameObjectsWithTag("SpawnDenialPoint")];

            var hazardData = hazardObjects.Select((hazardObject, index) => new {
                HazardObject = hazardObject,
                IsBlacklisted = Plugin.instance.hazardBlockList.Any(type => hazardObject.prefabToSpawn.GetComponent(type) != null),
                IsIncreasedSpawnRate = __instance.increasedMapHazardSpawnRateIndex == index,
            }).ToArray();

            foreach (var data in hazardData)
            {
                Plugin.GetLogger().LogDebug("Current spawnable object: " + data.HazardObject.prefabToSpawn.name);

                if (data.IsBlacklisted)
                {
                    Plugin.GetLogger().LogInfo($"Hazard blocked from spawning due to blacklist: {data.HazardObject.prefabToSpawn.name}");
                    continue;
                }

                HazardType effectiveHazardType = HazardType.CustomHazard;
                if (LCUtils.HAZARD_MAP.TryGetValue(data.HazardObject.prefabToSpawn.name, out HazardType hazardType))
                {
                    effectiveHazardType = hazardType;
                }

                ProcessHazard(effectiveHazardType, __instance, data.IsIncreasedSpawnRate, data.HazardObject, spawnDenialPoints, sanitizedPlanetName, entranceContainer, random, layerMask);
            }

            Plugin.GetLogger().LogInfo("Outside hazard spawning done.");
        }

        private static void ProcessHazard(HazardType type, RoundManager __instance, bool isIncreasedSpawnRate, SpawnableMapObject hazardObj, List<GameObject> spawnDenialPoints, string moonName, EntranceContainer entranceContainer, System.Random random, int layerMask)
        {
            Plugin.instance.hazardConfigMap.TryGetValue(type, out HazardConfiguration hazardConfig);

            if (!hazardConfig.Enabled)
            {
                return;
            }

            Plugin.GetLogger().LogInfo($"Spawning {type}s outside...");
            hazardConfig.MoonMap.TryGetValue(moonName, out MoonMinMax moonMinMax);

            LCUtils.DetermineMinMaxSpawnRates(isIncreasedSpawnRate, hazardConfig.MinSpawnRate, hazardConfig.MaxSpawnRate, moonMinMax, out int minSpawnRate, out int maxSpawnRate);
            HazardCalculationContainer container = new(random, spawnDenialPoints, hazardObj, minSpawnRate, maxSpawnRate, layerMask);

            if (type == HazardType.Turret)
            {
                container.NeedsSafetyZone = true;
                container.SpawnRatioMultiplier = 1.25f;
            }

            if (type == HazardType.SpikeRoofTrap)
            {
                container.NeedsSafetyZone = true;
            }

            CalculateHazardSpawn(__instance, container, entranceContainer, hazardConfig.SpawnStrategy);
        }

        private static void CalculateHazardSpawn(RoundManager __instance, HazardCalculationContainer hazardCalculationContainer, EntranceContainer entranceContainer, SpawnStrategy spawnStrategy)
        {
            Vector3 mainEntrancePosition = entranceContainer.MainEntrancePosition;
            List<Vector3> fireExitPositions = entranceContainer.FireExitPositions;
            Transform[] shipSpawnPathPoints = __instance.shipSpawnPathPoints;

            int hazardCounter = 0;

            // This is where the ship actually lands.
            Vector3 shipLandPosition = shipSpawnPathPoints.Last().position;
            //Vector3 shipFront = GameObject.FindGameObjectWithTag("")

            int randomSpawnRate = hazardCalculationContainer.Random.Next(hazardCalculationContainer.MinSpawnRate, hazardCalculationContainer.MaxSpawnRate + 1);
            Plugin.GetLogger().LogDebug("Random spawn rate: " + randomSpawnRate);

            List<SpawnPositionData> positionDataList = spawnStrategy.CalculateCenterPositions(shipLandPosition, mainEntrancePosition, fireExitPositions, hazardCalculationContainer.SpawnRatioMultiplier);

            List<GameObject> gameObjects = [];
            int effectiveLayerMask = hazardCalculationContainer.LayerMask;

            int spawnRatePerPosition = randomSpawnRate / positionDataList.Count;
            Plugin.GetLogger().LogDebug("Actual spawn rate per position: " + spawnRatePerPosition);

            foreach (SpawnPositionData spawnPositionData in positionDataList)
            {
                for (int j = 0; j < spawnRatePerPosition; j++)
                {
                    (Vector3 randomPosition, Quaternion quaternion) = LCUtils.GetRandomGroundPositionAndRotation(spawnPositionData.CenterPosition, spawnPositionData.SpawnRadius, hazardCalculationContainer.Random, effectiveLayerMask);

                    if (randomPosition == Vector3.zero)
                    {
                        Plugin.GetLogger().LogDebug("No NavMesh hit!");
                        continue;
                    }


                    List<Vector3> denialPoints = [shipLandPosition];
                    denialPoints.AddRange(hazardCalculationContainer.SpawnDenialPoints.Select(x => x.transform.position).ToArray());
                    GameObject playerShipNavmesh = GameObject.Find("PlayerShipNavmesh");

                    if (playerShipNavmesh != null)
                    {
                        Vector3 shipPosition = playerShipNavmesh.transform.position;
                        denialPoints.Add(shipPosition);
                    }

                    bool invalidSpawnPointFound = IsInvalidSpawnPoint(denialPoints, randomPosition, hazardCalculationContainer.NeedsSafetyZone ? 18f : 8f);

                    // Do not spawn the hazard if it's too close to a spawn denial point.
                    if (invalidSpawnPointFound)
                    {
                        Plugin.GetLogger().LogDebug("Hazard was too close to denial or safety zone and was therefore deleted: " + randomPosition);
                        continue;
                    }

                    gameObjects.Add(InstantiateHazardObject(__instance, hazardCalculationContainer.SpawnableMapObject, randomPosition, quaternion));

                    hazardCounter++;
                }
            }

            __instance.StartCoroutine(SpawnHazardsInBulk(gameObjects));

            Plugin.GetLogger().LogDebug("Total hazard amount: " + hazardCounter);
        }

        private static GameObject InstantiateHazardObject(RoundManager __instance, SpawnableMapObject spawnableMapObject, Vector3 position, Quaternion quaternion)
        {
            Plugin.GetLogger().LogDebug("Spawn hazard outside at: " + position);
            GameObject gameObject = UnityEngine.Object.Instantiate(spawnableMapObject.prefabToSpawn, position, quaternion, __instance.mapPropsContainer.transform);

            if (spawnableMapObject.spawnFacingAwayFromWall)
            {
                gameObject.transform.eulerAngles = new Vector3(0f, __instance.YRotationThatFacesTheFarthestFromPosition(position + Vector3.up * 0.2f), 0f);
            }

            if (!Plugin.instance.v49CompatibilityEnabled)
            {
                if ((bool) LCUtils.GetReflectionField(spawnableMapObject, "spawnFacingWall"))
                {
                    gameObject.transform.eulerAngles = new Vector3(0f, __instance.YRotationThatFacesTheNearestFromPosition(position + Vector3.up * 0.2f), 0f);
                }

                if ((bool) LCUtils.GetReflectionField(spawnableMapObject, "spawnWithBackToWall") && Physics.Raycast(gameObject.transform.position, -gameObject.transform.forward, out var hitInfo, 300f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                {
                    if (Physics.Raycast(hitInfo.point + Vector3.up * 0.2f, Vector3.down, out RaycastHit groundHit, 50f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
                    {
                        gameObject.transform.position = groundHit.point;
                    } else
                    {
                        gameObject.transform.position = hitInfo.point;
                    }
                        
                    if ((bool) LCUtils.GetReflectionField(spawnableMapObject, "spawnWithBackFlushAgainstWall"))
                    {
                        gameObject.transform.forward = -hitInfo.normal;
                        gameObject.transform.eulerAngles = new Vector3(0f, gameObject.transform.eulerAngles.y, 0f);
                    }
                }
            }

            gameObject.SetActive(value: true);
            gameObject.layer = LayerMask.NameToLayer(HAZARD_LAYER_NAME);

            return gameObject;
        }

        private static bool IsInvalidSpawnPoint(List<Vector3> spawnDenialPoints, Vector3 randomPosition, float safetyDistance)
        {
            foreach (Vector3 spawnDenialPoint in spawnDenialPoints)
            {
                if (Vector3.Distance(randomPosition, spawnDenialPoint) < safetyDistance)
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
                        if (objectToSpawn != null)
                        {
                            NetworkObject networkObject = objectToSpawn.GetComponent<NetworkObject>();
                            if (networkObject != null)
                            {
                                networkObject.Spawn(destroyWithScene: true);
                            }
                            else
                            {
                                Plugin.GetLogger().LogError($"Hazard {objectToSpawn.name} had no network object and cannot be spawned.");
                            }
                        }
                        else
                        {
                            Plugin.GetLogger().LogError($"Hazard object was destroyed before it could spawn. Probably needs compatibility patch.");
                        }
                    }
                    catch (Exception e)
                    {
                        Plugin.GetLogger().LogError($"NetworkObject {objectToSpawn.name} could not be spawned: {e}");
                    }
                }

                yield return new WaitForSeconds(0.5f);
            }

            Plugin.instance.IsCoroutineRunning = false;
            Plugin.GetLogger().LogDebug($"SpawnHazardsInBulk Coroutine done.");
        }

        public static IEnumerator SpawnHazardsAfterExitSpawn(RoundManager __instance, SpawnableMapObject[] hazardObjects, System.Random random)
        {
            float startTime = Time.timeSinceLevelLoad;
            EntranceContainer entranceContainer = LCUtils.FindAllExitPositions();
            Plugin.GetLogger().LogDebug($"Time since level loaded: {startTime}");
            while (!entranceContainer.IsInitialized() && Time.timeSinceLevelLoad - startTime < 15f)
            {
                Plugin.GetLogger().LogDebug("Waiting for main entrance to load...");
                yield return new WaitForSeconds(1f);
                entranceContainer = LCUtils.FindAllExitPositions();
            }

            SpawnHazardsOutside(__instance, hazardObjects, entranceContainer, random);
        }
    }
}
