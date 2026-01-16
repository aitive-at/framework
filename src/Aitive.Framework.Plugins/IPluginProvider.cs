namespace Aitive.Framework.Plugins;

public interface IPluginProvider
{
    IEnumerable<PluginDescription> GetDescriptions();
}
