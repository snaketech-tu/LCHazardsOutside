using System;
using System.Linq;
using UnityEngine.AI;
using UnityEngine;
using System.Collections.Generic;
using LCHazardsOutside.Data;
using LCHazardsOutside.Abstract;
using LCHazardsOutside.Strategy;
using System.Reflection;
using HarmonyLib;

namespace LCHazardsOutside
{
    public class LCUtils
    {

        public static readonly Dictionary<string, string[]> CUSTOM_LAYER_MASK = new()
        {
            { VanillaMoon.march.ToString(), ["Room"] }, // March's water is on the Default layer instead of the Water layer..
        };

        public static readonly Dictionary<string, HazardType> HAZARD_MAP = new(3)
        {
            { "Landmine", HazardType.Landmine },
            { "TurretContainer", HazardType.Turret },
            { "SpikeRoofTrapHazard", HazardType.SpikeRoofTrap }
        };

        private static readonly Dictionary<SpawnStrategyType, SpawnStrategy> STRATEGY_MAP = new(3)
        {
            { SpawnStrategyType.MainAndFireExit, MainAndFireExitSpawnStrategy.GetInstance() },
            { SpawnStrategyType.MainEntranceOnly, MainEntranceOnlySpawnStrategy.GetInstance() },
            { SpawnStrategyType.FireExitsOnly, FireExitsOnlySpawnStrategy.GetInstance() }
        };

        public static SpawnStrategy GetSpawnStrategy(string typeString)
        {
            try
            {
                SpawnStrategyType strategyType = (SpawnStrategyType)Enum.Parse(typeof(SpawnStrategyType), typeString);

                if (STRATEGY_MAP.TryGetValue(strategyType, out SpawnStrategy result))
                {
                    return result;
                }
            } catch (Exception)
            {
                Plugin.GetLogger().LogError($"Type {typeString} could not be parsed into a SpawnStrategyType. Reverting to default...");
            }

            return MainAndFireExitSpawnStrategy.GetInstance();
        }

        // Same as LethalLevelLoader so people get used to the same planet names.
        public static string GetNumberlessMoonName(SelectableLevel selectableLevel)
        {
            if (selectableLevel != null)
            {
                return new string(selectableLevel.PlanetName.SkipWhile(c => !char.IsLetter(c)).ToArray()).ToLower();
            }
            else
            {
                return string.Empty;
            }
        }

        public static bool IsVanillaMoon(string moonName)
        {
            return Enum.TryParse(typeof(VanillaMoon), moonName, true, out _);
        }

        public static bool IsVanillaMoon(SelectableLevel selectableLevel)
        {
            return Enum.TryParse(typeof(VanillaMoon), GetNumberlessMoonName(selectableLevel), true, out _);
        }

        private static float RandomNumberInRadius(float radius, System.Random randomSeed)
        {
            return ((float)randomSeed.NextDouble() - 0.5f) * radius;
        }

        public static (Vector3, Quaternion) GetRandomGroundPositionAndRotation(Vector3 centerPoint, float radius = 10f, System.Random randomSeed = null, int layerMask = -1, int maxAttempts = 10)
        {
            float y = centerPoint.y;
            float x, y2, z;

            Vector3 randomPosition;

            for (int i = 0; i < maxAttempts; i++)
            {
                try
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
                        }
                        else
                        {
                            Plugin.GetLogger().LogDebug($"Nav hit at: {navMeshHit.position} but ray cast failed.");
                        }
                    }
                }
                catch (Exception)
                {

                }
            }

            return (Vector3.zero, Quaternion.identity);
        }

        public static void DetermineMinMaxSpawnRates(bool increasedMapHazardSpawnRate, int configMinSpawnRate, int configMaxSpawnRate, MoonMinMax moonMinMax, out int minSpawnRate, out int maxSpawnRate)
        {
            int effectiveUserMinValue = moonMinMax != null ? moonMinMax.Min : configMinSpawnRate;
            int effectiveUserMaxValue = moonMinMax != null ? moonMinMax.Max : configMaxSpawnRate;

            // Range from 0 to max config.
            minSpawnRate = Mathf.Min(Mathf.Max(effectiveUserMinValue, 0), effectiveUserMaxValue);
            // Range from min to 100.
            maxSpawnRate = Mathf.Max(Mathf.Min(effectiveUserMaxValue, 100), minSpawnRate);

            if (increasedMapHazardSpawnRate)
            {
                minSpawnRate = Mathf.Max(5, minSpawnRate);
                maxSpawnRate = Mathf.Min(maxSpawnRate * 2, 15);
            }
        }

        public static EntranceContainer FindAllExitPositions()
        {
            EntranceTeleport[] entranceTeleports = UnityEngine.Object.FindObjectsOfType<EntranceTeleport>(includeInactive: false);
            List<Vector3> fireExists = [];
            Vector3 mainEntrance = Vector3.zero;
            for (int i = 0; i < entranceTeleports.Length; i++)
            {
                EntranceTeleport entranceTeleport = entranceTeleports[i];   

                if (entranceTeleport.isEntranceToBuilding)
                {
                    // main entrance always has entranceId of 0.
                    if (entranceTeleport.entranceId == 0)
                    {
                        mainEntrance = entranceTeleport.transform.position;
                    } else
                    {
                        fireExists.Add(entranceTeleports[i].transform.position);
                    }
                }
            }

            return new EntranceContainer(mainEntrance, fireExists);
        }

        public static Dictionary<string, MoonMinMax> ParseMoonString(string moonString)
        {
            if (string.IsNullOrEmpty(moonString))
            {
                return [];
            }

            Dictionary<string, MoonMinMax> moonMap = [];

            string[] moonMinMaxList = moonString.Trim().ToLower().Split(',');

            foreach (string moonMinMax in moonMinMaxList)
            {
                try
                {
                    string[] parts = moonMinMax.Trim().Split(':');

                    moonMap.TryAdd(parts[0], new MoonMinMax(int.Parse(parts[1]), int.Parse(parts[2])));
                }
                catch (Exception)
                {
                    Plugin.GetLogger().LogError($"There was an error while parsing the moon string {moonMinMax}. Make sure it has the format moon:min:max.");
                }
            }

            return moonMap;
        }

        // Reflection for compatibility with old versions
        public static object GetReflectionField(object obj, string fieldName)
        {
            return AccessTools.Field(obj.GetType(), fieldName).GetValue(obj);
        }

    }
}
