using HarmonyLib;
using LCHazardsOutside.Abstract;

namespace LCHazardsOutside.ModCompatibility
{
    internal class LateGameUpgradesHandler : AbstractCompatibilityHandler
    {

        public override void Apply()
        {
            if (IsEnabled())
            {
                LogApply();
                Plugin.instance.hazardBlockList.Add(AccessTools.TypeByName("MoreShipUpgrades.UpgradeComponents.Items.Wheelbarrow.ScrapWheelbarrow"));
            }
        }

        public override string GetModGUID()
        {
            return "com.malco.lethalcompany.moreshipupgrades";
        }
    }
}
