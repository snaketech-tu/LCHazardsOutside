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
            Assembly targetAssembly = GetTargetAssembly("MoreShipUpgrades");
            if (targetAssembly == null)
            {
                Plugin.GetLogger().LogError("Target assembly 'MoreShipUpgrades' not found.");
                return;
            }

            AddTypeToHazardBlockList(targetAssembly, "MoreShipUpgrades.UpgradeComponents.Items.Wheelbarrow.ScrapWheelbarrow");
            AddContractItemsToBlocklist(targetAssembly);
        }

        private void AddTypeToHazardBlockList(Assembly assembly, string typeName)
        {
            Type type = assembly.GetTypes().FirstOrDefault(t => t.FullName == typeName);
            if (type != null)
            {
                Plugin.instance.hazardBlockList.Add(type);
                Plugin.GetLogger().LogDebug($"Added {typeName} to hazardBlockList.");
            }
            else
            {
                Plugin.GetLogger().LogWarning($"Type {typeName} not found in assembly.");
            }
        }

        private void AddContractItemsToBlocklist(Assembly assembly)
        {
            Type baseType = assembly.GetTypes().FirstOrDefault(t => t.Name == "ContractObject");
            if (baseType == null)
            {
                Plugin.GetLogger().LogError($"Base type ContractObject not found in assembly.");
                return;
            }

            var subclasses = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType));
            foreach (Type subclass in subclasses)
            {
                Plugin.instance.hazardBlockList.Add(subclass);
                Plugin.GetLogger().LogDebug($"Added {subclass.Name} to hazardBlockList.");
            }
        }

        protected override string GetModGUID()
        {
            return "com.malco.lethalcompany.moreshipupgrades";
        }
    }
}
