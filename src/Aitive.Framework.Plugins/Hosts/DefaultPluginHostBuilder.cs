using Aitive.Framework.Collections;
using Aitive.Framework.Plugins.Resolution;
using Aitive.Framework.Versioning;
using Microsoft.Extensions.Logging;

namespace Aitive.Framework.Plugins.Hosts;

public static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Loading plugin: {PluginId}")]
    public static partial void LoadingPlugin(this ILogger logger, PluginVersionId pluginId);
}

public sealed class DefaultPluginHostBuilder : IPluginHostBuilder
{
    private readonly Dictionary<PluginId, IReadOnlyList<PluginManifest>> _availablePlugins;
    private readonly Dictionary<PluginVersionId, IPluginProvider> _pluginProviders;
    private readonly PluginResolver _resolver;
    private readonly ILogger _logger;

    public DefaultPluginHostBuilder(ILogger logger)
        : this(new PluginResolver(), logger) { }

    public DefaultPluginHostBuilder(PluginResolver resolver, ILogger logger)
    {
        _availablePlugins = new Dictionary<PluginId, IReadOnlyList<PluginManifest>>();
        _pluginProviders = new Dictionary<PluginVersionId, IPluginProvider>();
        _resolver = resolver;
        _logger = logger;
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
            _logger.LoadingPlugin(manifest.VersionId);

            var plugin = provider.Load(manifest.VersionId);
            loadedPlugins.Add(plugin);
        }

        return new DefaultPluginHost(loadedPlugins, _logger);
    }
}
