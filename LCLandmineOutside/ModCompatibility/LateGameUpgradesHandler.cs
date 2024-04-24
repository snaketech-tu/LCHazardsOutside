using HarmonyLib;
using LCHazardsOutside.Abstract;
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
        }

        protected override string GetModGUID()
        {
            return "com.malco.lethalcompany.moreshipupgrades";
        }
    }
}
