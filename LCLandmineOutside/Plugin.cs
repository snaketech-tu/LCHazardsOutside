

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LCHazardsOutside.Abstract;
using LCHazardsOutside.Data;
using LCHazardsOutside.ModCompatibility;
using LCHazardsOutside.Patches;
using System;
using System.Collections.Generic;

namespace LCHazardsOutside
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("com.malco.lethalcompany.moreshipupgrades", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin {
        public const string modGUID = "snake.tech.LCHazardsOutside";
        private const string modName = "LCHazardsOutside";
        private const string modVersion = "1.2.0";

        private readonly Harmony harmony = new(modGUID);
        private readonly AcceptableValueRange<int> acceptableSpawnRange = new(0, 100);
        private readonly AcceptableValueList<string> acceptableSpawnStrategies = new(Enum.GetNames(typeof(SpawnStrategyType)));

        // General Globals
        public static Plugin instance;
        public bool IsCoroutineRunning = false;
        public HashSet<Type> hazardBlockList = [];
        public bool v49CompatibilityEnabled = false;

        // Config
        public Dictionary<HazardType, HazardConfiguration> hazardConfigMap = [];
        public ConfigEntry<int> noHazardSpawnChance;


        void Awake() {
            if (instance == null)
            {
                instance = this;
            }

            // Config
            LoadConfig();

            // Compatibility
            new LateGameUpgradesHandler().Apply();
            new V49Handler().Apply();

            // Patches
            harmony.PatchAll(typeof(RoundManagerPatch));

            GetLogger().LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        void LoadConfig() {
            ConfigDescription spawnStrategyDescription = new(
                """
                This setting dictates how spawn positions are allocated. It has 3 possible options: "MainAndFireExit", "MainEntranceOnly" and "FireExitsOnly".
                When set to "MainAndFireExit", spawn positions are determined based on both the main entrance, the fire exits and the ship.
                When set to "MainEntranceOnly", spawn positions are limited strictly to the area between the ship and the main entrance of the facility, making spawn points more concentrated and fire exits safe.
                When set to "FireExitsOnly", spawn positions are limited strictly to the area between the ship and the fire exits of the facility, making fire exits more punishing while leaving the main entrance hassle-free.
                """, acceptableSpawnStrategies);

            ConfigDescription minDescription = new("Minimum amount to spawn outside.", acceptableSpawnRange);
            ConfigDescription maxDescription = new("Maximum amount to spawn outside.", acceptableSpawnRange);
            ConfigDescription moonDescription = new("""
                The moon(s) where this hazard can spawn outside in the form of a comma separated list of selectable level names with min/max values in moon:min:max format (e.g. "experimentation:5:15,rend:0:10,dine:10:15")
                "NOTE: These must be the internal data names of the levels (for vanilla moons use the names you see on the terminal i.e. vow, march and for modded moons check their description or ask the author).
                """);

            noHazardSpawnChance = Config.Bind("0. General", "NoHazardSpawnChance", 0, "A global chance from 0 to 100 in % for NO hazards to spawn outside.\n Use a non-zero chance if you want to make hazards outside more of a surprise.");

            // Landmine
            ConfigEntry<bool> enableLandmine = Config.Bind("1. Landmine","EnableLandmineOutside", true, "Whether or not to spawn landmines outside.");
            ConfigEntry<int> globalLandmineMinSpawnRate = Config.Bind("1. Landmine", "LandmineMinSpawnRate", 15, minDescription);
            ConfigEntry<int> globalLandmineMaxSpawnRate = Config.Bind("1. Landmine", "LandmineMaxSpawnRate", 30, maxDescription);
            ConfigEntry<string> landmineMoonString = Config.Bind("1. Landmine", "LandmineMoons", "", moonDescription);
            ConfigEntry<string> landmineSpawnStrategyString = Config.Bind("1. Landmine", "LandmineSpawnStrategy", SpawnStrategyType.MainAndFireExit.ToString(), spawnStrategyDescription);

            Dictionary<string, MoonMinMax> landmineMoonMap = LCUtils.ParseMoonString(landmineMoonString.Value);
            SpawnStrategy landmineSpawnStrategy = LCUtils.GetSpawnStrategy(landmineSpawnStrategyString.Value);

            hazardConfigMap.Add(HazardType.Landmine, new HazardConfiguration(enableLandmine.Value, globalLandmineMinSpawnRate.Value, globalLandmineMaxSpawnRate.Value, landmineMoonMap, landmineSpawnStrategy));

            // Turret
            ConfigEntry<bool> enableTurret = Config.Bind("2. Turret", "EnableTurretOutside", false, "Whether or not to spawn turrets outside.");
            ConfigEntry<int> globalTurretMinSpawnRate = Config.Bind("2. Turret", "TurretMinSpawnRate", 0, minDescription);
            ConfigEntry<int> globalTurretMaxSpawnRate = Config.Bind("2. Turret", "TurretMaxSpawnRate", 1, maxDescription);
            ConfigEntry<string> turretMoonString = Config.Bind("2. Turret", "TurretMoons", "", moonDescription);
            ConfigEntry<string> turretSpawnStrategyString = Config.Bind("2. Turret", "TurretSpawnStrategy", SpawnStrategyType.MainAndFireExit.ToString(), spawnStrategyDescription);

            Dictionary<string, MoonMinMax> turretMoonMap = LCUtils.ParseMoonString(turretMoonString.Value);
            SpawnStrategy turretSpawnStrategy = LCUtils.GetSpawnStrategy(turretSpawnStrategyString.Value);

            hazardConfigMap.Add(HazardType.Turret, new HazardConfiguration(enableTurret.Value, globalTurretMinSpawnRate.Value, globalTurretMaxSpawnRate.Value, turretMoonMap, turretSpawnStrategy));

            // SpikeRoofTrap
            ConfigEntry<bool> enableSpikeRoofTrap = Config.Bind("3. SpikeRoofTrap", "EnableSpikeRoofTrapOutside", true, "Whether or not to spawn spike roof traps outside.");
            ConfigEntry<int> globalSpikeRoofTrapMinSpawnRate = Config.Bind("3. SpikeRoofTrap", "SpikeRoofTrapMinSpawnRate", 0, minDescription);
            ConfigEntry<int> globalSpikeRoofTrapMaxSpawnRate = Config.Bind("3. SpikeRoofTrap", "SpikeRoofTrapMaxSpawnRate", 2, maxDescription);
            ConfigEntry<string> spikeRoofTrapMoonString = Config.Bind("3. SpikeRoofTrap", "SpikeRoofTrapMoons", "", moonDescription);
            ConfigEntry<string> spikeRoofTrapSpawnStrategyString = Config.Bind("3. SpikeRoofTrap", "SpikeRoofTrapSpawnStrategy", SpawnStrategyType.MainAndFireExit.ToString(), spawnStrategyDescription);

            Dictionary<string, MoonMinMax> spikeRoofTrapMoonMap = LCUtils.ParseMoonString(spikeRoofTrapMoonString.Value);
            SpawnStrategy spikeRoofTrapSpawnStrategy = LCUtils.GetSpawnStrategy(spikeRoofTrapSpawnStrategyString.Value);

            hazardConfigMap.Add(HazardType.SpikeRoofTrap, new HazardConfiguration(enableSpikeRoofTrap.Value, globalSpikeRoofTrapMinSpawnRate.Value, globalSpikeRoofTrapMaxSpawnRate.Value, spikeRoofTrapMoonMap, spikeRoofTrapSpawnStrategy));

            // CustomHazard
            ConfigEntry<bool> enableCustomHazard = Config.Bind("99. Custom", "EnableCustomHazardOutside", false, "Whether or not to spawn modded hazards outside.");
            ConfigEntry<int> globalCustomHazardMinSpawnRate = Config.Bind("99. Custom", "CustomHazardMinSpawnRate", 0, minDescription);
            ConfigEntry<int> globalCustomHazardMaxSpawnRate = Config.Bind("99. Custom", "CustomHazardMaxSpawnRate", 3, maxDescription);
            ConfigEntry<string> customHazardMoonString = Config.Bind("99. Custom", "CustomHazardMoons", "", moonDescription);
            ConfigEntry<string> customHazardSpawnStrategyString = Config.Bind("99. Custom", "CustomHazardSpawnStrategy", SpawnStrategyType.MainAndFireExit.ToString(), spawnStrategyDescription);

            Dictionary<string, MoonMinMax> customHazardMoonMap = LCUtils.ParseMoonString(customHazardMoonString.Value);
            SpawnStrategy customHazardSpawnStrategy = LCUtils.GetSpawnStrategy(customHazardSpawnStrategyString.Value);

            hazardConfigMap.Add(HazardType.CustomHazard, new HazardConfiguration(enableCustomHazard.Value, globalCustomHazardMinSpawnRate.Value, globalCustomHazardMaxSpawnRate.Value, customHazardMoonMap, customHazardSpawnStrategy));
        }

        public static ManualLogSource GetLogger() {
            return Plugin.instance.Logger;
        }
    }
}
