namespace Aitive.Framework.Plugins;

public interface IPluginProvider
{
    IEnumerable<PluginManifest> GetDescriptions();
}
