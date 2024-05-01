using HarmonyLib;
using LCHazardsOutside.Abstract;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LCHazardsOutside.ModCompatibility
{
    internal class LateGameUpgradesHandler : AbstractCompatibilityHandler
    {
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        protected override void DoApply()
        {
            LogApply();
            Plugin.instance.hazardBlockList.Add(AccessTools.TypeByName("MoreShipUpgrades.UpgradeComponents.Items.Wheelbarrow.ScrapWheelbarrow"));
            Assembly targetAssembly = AccessTools.AllAssemblies().FirstOrDefault(assembly => assembly.GetName().Name.Equals("MoreShipUpgrades", System.StringComparison.OrdinalIgnoreCase));

            // Find all types that are subclasses of ContractObject
            var contractTypes = targetAssembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(AccessTools.TypeByName("ContractObject")));

            // Add found types to the hazardBlockList
            foreach (Type type in contractTypes)
            {
                Plugin.instance.hazardBlockList.Add(type);
                Plugin.GetLogger().LogDebug($"Added {type.Name} to hazardBlockList.");
            }
        }

        protected override string GetModGUID()
        {
            return "com.malco.lethalcompany.moreshipupgrades";
        }
    }
}
