using LCHazardsOutside.Abstract;
using System.Collections.Generic;

namespace LCHazardsOutside.Data
{
    public class HazardConfiguration(bool enabled, int minSpawnRate, int maxSpawnRate, Dictionary<string, MoonMinMax> moonMap, SpawnStrategy spawnStrategy)
    {
        public bool Enabled { get; set; } = enabled;
        public int MinSpawnRate { get; set; } = minSpawnRate;
        public int MaxSpawnRate { get; set; } = maxSpawnRate;
        public Dictionary<string, MoonMinMax> MoonMap { get; set; } = moonMap;
        public SpawnStrategy SpawnStrategy { get; set; } = spawnStrategy;
    }
}
