using Aitive.Framework.Plugins.Resolution;

namespace Aitive.Framework.Plugins;

public interface IPluginHostBuilder
{
    IReadOnlyDictionary<PluginId, IReadOnlyList<PluginManifest>> AvailablePlugins { get; }

    IPluginHostBuilder WithProvider(IPluginProvider provider);

    IPluginHost Build(IReadOnlyList<PluginRequest> requests, PluginResolutionPolicy? policy = null);
}
