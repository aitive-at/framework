using Aitive.Framework.Patterns.Disposal;

namespace Aitive.Framework.Plugins.Hosts;

internal sealed class DefaultPluginHost : IPluginHost
{
    private readonly IReadOnlyList<IPlugin> _plugins;

    internal DefaultPluginHost(IReadOnlyList<IPlugin> plugins)
    {
        _plugins = plugins.ToList();
    }

    public IReadOnlyList<PluginManifest> Plugins => _plugins.Select(p => p.Manifest).ToList();

    public IPluginBindPoint Bind(IPluginBindPointBuilder builder)
    {
        return builder.Build(_plugins);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var plugin in _plugins)
        {
            await plugin.EnsureDisposal();
        }
    }
}
