

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LCHazardsOutside.ModCompatibility;
using LCHazardsOutside.Patches;
using System;
using System.Collections.Generic;

namespace LCHazardsOutside
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin {
        private const string modGUID = "snake.tech.LCHazardsOutside";
        private const string modName = "LCHazardsOutside";
        private const string modVersion = "1.1.3.0";

        private readonly Harmony harmony = new(modGUID);

        public static Plugin instance;
        public bool IsCoroutineRunning = false;
        public HashSet<Type> hazardBlockList = [];
        // Experimental
        public bool spawnCompatibilityMode = false;

        public ConfigEntry<bool> enableLandmine;
        public ConfigEntry<int> globalLandmineMinSpawnRate;
        public ConfigEntry<int> globalLandmineMaxSpawnRate;
        public Dictionary<string, MoonMinMax> landmineMoonMap;

        public ConfigEntry<bool> enableTurret;
        public ConfigEntry<int> globalTurretMinSpawnRate;
        public ConfigEntry<int> globalTurretMaxSpawnRate;
        public ConfigEntry<string> turretMoonString;
        public Dictionary<string, MoonMinMax> turretMoonMap;

        public ConfigEntry<bool> enableCustomHazard;
        public ConfigEntry<int> globalCustomHazardMinSpawnRate;
        public ConfigEntry<int> globalCustomHazardMaxSpawnRate;
        public ConfigEntry<string> customHazardMoonString;
        public Dictionary<string, MoonMinMax> customHazardMoonMap;

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
            new BrutalCompanyMinusHandler().Apply();

            // Patches
            harmony.PatchAll(typeof(RoundManagerPatch));

            GetLogger().LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        void LoadConfig() {
            noHazardSpawnChance = Config.Bind("General", "NoHazardSpawnChance", 0, "A global chance from 0 to 100 in % for NO hazards to spawn outside.\n Use a non-zero chance if you want to make hazards outside more of a surprise.");

            // Landmine
            enableLandmine = Config.Bind("Landmine","EnableLandmineOutside", true, "Whether or not to spawn landmines outside."); 
            globalLandmineMinSpawnRate = Config.Bind("Landmine", "LandmineMinSpawnRate", 5, "Minimum amount of landmines to spawn outside.");
            globalLandmineMaxSpawnRate = Config.Bind("Landmine", "LandmineMaxSpawnRate", 15, "Maximum amount of landmines to spawn outside.");
            ConfigEntry<string> landmineMoonString = Config.Bind("Landmine", "LandmineMoons", "", "The moon(s) where the landmines can spawn outside on in the form of a comma separated list of selectable level names with min/max values in moon:min:max format (e.g. \"experimentation:5:15,rend:0:10,dine:10:15\")\n" +
                "NOTE: These must be the internal data names of the levels (for vanilla moons use the names you see on the terminal i.e. vow, march and for modded moons you will have to find their name).\n");
              
            landmineMoonMap = ParseMoonString(landmineMoonString.Value);

            // Turret
            enableTurret = Config.Bind("Turret", "EnableTurretOutside", true, "Whether or not to spawn turrets outside.");
            globalTurretMinSpawnRate = Config.Bind("Turret", "TurretMinSpawnRate", 0, "Minimum amount of turrets to spawn outside.");
            globalTurretMaxSpawnRate = Config.Bind("Turret", "TurretMaxSpawnRate", 1, "Maximum amount of turrets to spawn outside.");
            ConfigEntry<string> turretMoonString = Config.Bind("Turret", "TurretMoons", "", "The moon(s) where the landmines can spawn outside on in the form of a comma separated list of selectable level names with min/max values in moon:min:max format (e.g. \"experimentation:5:15,rend:0:10,dine:10:15\")\n" +
               "NOTE: These must be the internal data names of the levels (for vanilla moons use the names you see on the terminal i.e. vow, march and for modded moons you will have to find their name).\n");

            turretMoonMap = ParseMoonString(turretMoonString.Value);

            // Custom
            enableCustomHazard = Config.Bind("Custom", "EnableCustomHazardOutside", true, "Whether or not to spawn modded hazards outside.");
            globalCustomHazardMinSpawnRate = Config.Bind("Custom", "CustomHazardMinSpawnRate", 1, "Minimum amount of custom hazards to spawn outside.");
            globalCustomHazardMaxSpawnRate = Config.Bind("Custom", "CustomHazardMaxSpawnRate", 1, "Maximum amount of custom hazards to spawn outside.");
            ConfigEntry<string> customHazardMoonString = Config.Bind("Custom", "CustomHazardMoons", "", "The moon(s) where the custom hazards can spawn outside on in the form of a comma separated list of selectable level names with min/max values in moon:min:max format (e.g. \"experimentation:5:15,rend:0:10,dine:10:15\")\n" +
               "NOTE: These must be the internal data names of the levels (for vanilla moons use the names you see on the terminal e.g. vow, march and for modded moons you will have to find their name).\n");

            customHazardMoonMap = ParseMoonString(customHazardMoonString.Value);
        }

        private Dictionary<string, MoonMinMax> ParseMoonString(string moonString)
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
                    GetLogger().LogError($"There was an error while parsing the moon string {moonMinMax}. Make sure it has the format moon:min:max.");
                }
            }

            return moonMap;
        }

        public static ManualLogSource GetLogger() {
            return Plugin.instance.Logger;
        }
    }
}
