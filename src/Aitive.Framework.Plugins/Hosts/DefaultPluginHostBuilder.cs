using Aitive.Framework.Collections;
using Aitive.Framework.Plugins.Resolution;
using Aitive.Framework.Versioning;

namespace Aitive.Framework.Plugins.Hosts;

public sealed class DefaultPluginHostBuilder : IPluginHostBuilder
{
    private readonly Dictionary<PluginId, IReadOnlyList<PluginManifest>> _availablePlugins;
    private readonly Dictionary<PluginVersionId, IPluginProvider> _pluginProviders;
    private readonly PluginResolver _resolver;

    public DefaultPluginHostBuilder()
        : this(new PluginResolver()) { }

    public DefaultPluginHostBuilder(PluginResolver resolver)
    {
        _availablePlugins = new Dictionary<PluginId, IReadOnlyList<PluginManifest>>();
        _pluginProviders = new Dictionary<PluginVersionId, IPluginProvider>();
        _resolver = resolver;
    }

    public IReadOnlyDictionary<PluginId, IReadOnlyList<PluginManifest>> AvailablePlugins =>
        _availablePlugins;

    public IPluginHostBuilder WithProvider(IPluginProvider provider)
    {
        foreach (var pluginManifest in provider.AvailablePlugins)
        {
            var list =
                (IList<PluginManifest>)
                    _availablePlugins.GetOrAdd(pluginManifest.Id, _ => new List<PluginManifest>());

            list.Add(pluginManifest);

            _availablePlugins[pluginManifest.Id] = (IReadOnlyList<PluginManifest>)list;
            _pluginProviders[pluginManifest.VersionId] = provider;
        }

        return this;
    }

    public IPluginHost Build(
        IReadOnlyList<PluginRequest> requests,
        PluginResolutionPolicy? policy = null
    )
    {
        var result = _resolver.Resolve(_availablePlugins, requests, policy);

        var loadedPlugins = new List<IPlugin>(result.OrderedManifests.Count);

        foreach (var manifest in result.OrderedManifests)
        {
            var provider = _pluginProviders[manifest.VersionId];
            loadedPlugins.Add(provider.Load(manifest.VersionId));
        }

        return new DefaultPluginHost(loadedPlugins);
    }
}
