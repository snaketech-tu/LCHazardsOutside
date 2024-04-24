using HarmonyLib;
using LCHazardsOutside.Abstract;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LCHazardsOutside.ModCompatibility
{
    internal class V49Handler : AbstractCompatibilityHandler
    {
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        protected override void DoApply()
        {
            Plugin.instance.v49CompatibilityEnabled = !AccessTools.AllTypes().Any(type => type.Name == "SpikeRoofTrap");

            if (Plugin.instance.v49CompatibilityEnabled)
            {
                Plugin.GetLogger().LogInfo("Running in v49 compatibility mode.");
            }
        }

        protected override string GetModGUID()
        {
            return Plugin.modGUID;
        }
    }
}
