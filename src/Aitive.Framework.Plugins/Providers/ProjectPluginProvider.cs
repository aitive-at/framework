using System.Reflection;

namespace Aitive.Framework.Plugins.Providers;

public class ProjectPluginProvider : IPluginProvider
{
    private readonly Dictionary<PluginVersionId, Assembly> _pluginAssemblies;
    private readonly Dictionary<PluginVersionId, PluginManifest> _pluginsManifests;

    public ProjectPluginProvider(IEnumerable<Assembly> plugins)
    {
        _pluginAssemblies = new Dictionary<PluginVersionId, Assembly>();
        _pluginsManifests = new Dictionary<PluginVersionId, PluginManifest>();

        foreach (var plugin in plugins)
        {
            var manifest = AssemblyPluginScanner.Scan(plugin);

            if (manifest)
            {
                _pluginAssemblies.Add(manifest.Value.VersionId, plugin);
                _pluginsManifests.Add(manifest.Value.VersionId, manifest.Value);
            }
        }
    }

    public IEnumerable<PluginManifest> AvailablePlugins => _pluginsManifests.Values;

    public IPlugin Load(PluginVersionId pluginVersionId)
    {
        if (
            _pluginsManifests.TryGetValue(pluginVersionId, out var pluginManifest)
            && _pluginAssemblies.TryGetValue(pluginVersionId, out var plugin)
        )
        {
            return new ProjectPlugin(pluginManifest, plugin);
        }

        throw new PluginException($"Unknown plugin version {pluginVersionId}");
    }
}
