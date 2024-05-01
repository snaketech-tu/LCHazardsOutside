using HarmonyLib;
using LCHazardsOutside.Abstract;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LCHazardsOutside.ModCompatibility
{
    internal class V49Handler : AbstractCompatibilityHandler
    {
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        protected override void DoApply()
        {
            // Get base game assembly for faster access
            Assembly targetAssembly = GetTargetAssembly("Assembly-CSharp");
            if (targetAssembly == null)
            {
                Plugin.GetLogger().LogError("Target assembly 'Assembly-CSharp' not found.");
                return;
            }

            Plugin.instance.v49CompatibilityEnabled = !targetAssembly.GetTypes().Any(type => type.Name.Equals("SpikeRoofTrap", StringComparison.OrdinalIgnoreCase));

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
