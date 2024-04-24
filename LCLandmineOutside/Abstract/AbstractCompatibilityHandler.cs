using System;

namespace LCHazardsOutside.Abstract
{
    internal abstract class AbstractCompatibilityHandler
    {
        protected abstract void DoApply();

        protected abstract string GetModGUID();

        public bool IsEnabled()
        {
            return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GetModGUID());
        }

        public void Apply()
        {
            if (IsEnabled())
            {
                try
                {
                    DoApply();
                } catch (Exception e)
                {
                    Plugin.GetLogger().LogError($"There was an error in patching {GetModGUID()}. Skipping... \n {e}\n");
                }
                
            }
        }

        public void LogApply()
        {
            Plugin.GetLogger().LogInfo($"Applying compatibility fixes for {GetModGUID()}.");
        }
    }
}
