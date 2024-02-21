using LCHazardsOutside.Abstract;

namespace LCHazardsOutside.ModCompatibility
{
    internal class BrutalCompanyMinusHandler : AbstractCompatibilityHandler
    {
        public override void Apply()
        {
            if (IsEnabled())
            {
                LogApply();
                Plugin.instance.spawnCompatibilityMode = true;
            }
        }

        public override string GetModGUID()
        {
            return "Drinkable.BrutalCompanyMinus";
        }
    }
}
