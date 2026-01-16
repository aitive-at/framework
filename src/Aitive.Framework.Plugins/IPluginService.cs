namespace Aitive.Framework.Plugins;

public interface IPluginService
{
    IReadOnlyDictionary<PluginId, PluginDescription> AvailablePlugins { get; }

    IReadOnlyDictionary<PluginId, IPlugin> ResidentPlugins { get; }

    void Load(IEnumerable<PluginDescription> plugins);
}
