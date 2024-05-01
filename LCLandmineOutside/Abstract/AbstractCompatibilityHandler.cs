using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace LCHazardsOutside.Abstract
{
    internal abstract class AbstractCompatibilityHandler
    {
        protected abstract void DoApply();

        protected abstract string GetModGUID();

        private bool IsEnabled()
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

        protected void LogApply()
        {
            Plugin.GetLogger().LogInfo($"Applying compatibility fixes for {GetModGUID()}.");
        }

        protected Assembly GetTargetAssembly(string assemblyName)
        {
            return AccessTools.AllAssemblies().FirstOrDefault(
                assembly => assembly.GetName().Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
