namespace LCHazardsOutside.Abstract
{
    internal abstract class AbstractCompatibilityHandler
    {
        public abstract void Apply();

        public abstract string GetModGUID();

        public bool IsEnabled()
        {
            return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GetModGUID());
        }

        public void LogApply()
        {
            Plugin.GetLogger().LogInfo($"Applying compatibility fixes for {GetModGUID()}.");
        }
    }
}
