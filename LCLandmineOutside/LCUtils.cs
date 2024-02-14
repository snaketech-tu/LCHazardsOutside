using System;
using System.Linq;

namespace LCHazardsOutside
{
    public class LCUtils
    {
        // Same as LethalLevelLoader so people get used to the same planet names.
        public static string GetNumberlessPlanetName(SelectableLevel selectableLevel)
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
            return Enum.TryParse(typeof(VanillaMoon), GetNumberlessPlanetName(selectableLevel), true, out _);
        }
    }
}
