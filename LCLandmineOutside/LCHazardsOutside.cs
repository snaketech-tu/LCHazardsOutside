

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LCHazardsOutside.Patches;

namespace LCHazardsOutside
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LCHazardsOutside : BaseUnityPlugin {
        private const string modGUID = "snake.tech.LCHazardsOutside";
        private const string modName = "LCHazardsOutside";
        private const string modVersion = "1.0.2.0";

        private readonly Harmony harmony = new(modGUID);

        public static LCHazardsOutside instance;

        public ConfigEntry<bool> configEnableLandmine;
        public ConfigEntry<int> configLandmineMinSpawnRate;
        public ConfigEntry<int> configLandmineMaxSpawnRate;

        public ConfigEntry<bool> configEnableTurret;
        public ConfigEntry<int> configTurretMinSpawnRate;
        public ConfigEntry<int> configTurretMaxSpawnRate;

        public ConfigEntry<bool> configEnableCustomHazard;
        public ConfigEntry<int> configCustomHazardMinSpawnRate;
        public ConfigEntry<int> configCustomHazardMaxSpawnRate;


        void Awake() {
            if (instance == null)
            {
                instance = this;
            }

            LoadConfig();
            GetLogger().LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            harmony.PatchAll(typeof(RoundManagerPatch));
        }

        void LoadConfig() {
            configEnableLandmine = Config.Bind("Landmine","EnableLandmineOutside", true, "Whether or not to spawn landmines outside."); 
            configLandmineMinSpawnRate = Config.Bind("Landmine", "LandmineMinSpawnRate", 5, "Minimum amount of landmines to spawn outside.");
            configLandmineMaxSpawnRate = Config.Bind("Landmine", "LandmineMaxSpawnRate", 15, "Maximum amount of landmines to spawn outside.");

            configEnableTurret = Config.Bind("Turret", "EnableTurretOutside", true, "Whether or not to spawn turrets outside.");
            configTurretMinSpawnRate = Config.Bind("Turret", "TurretMinSpawnRate", 0, "Minimum amount of turrets to spawn outside.");
            configTurretMaxSpawnRate = Config.Bind("Turret", "TurretMaxSpawnRate", 1, "Maximum amount of turrets to spawn outside.");

            configEnableCustomHazard = Config.Bind("Custom", "EnableCustomHazardOutside", true, "Whether or not to spawn modded hazards outside.");
            configCustomHazardMinSpawnRate = Config.Bind("Custom", "CustomHazardMinSpawnRate", 1, "Minimum amount of custom hazards to spawn outside.");
            configCustomHazardMaxSpawnRate = Config.Bind("Custom", "CustomHazardMaxSpawnRate", 2, "Maximum amount of custom hazards to spawn outside.");
        }

        public static ManualLogSource GetLogger() {
            return LCHazardsOutside.instance.Logger;
        }
    }
}
