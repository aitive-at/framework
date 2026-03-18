using Aitive.Framework.Patterns.Disposal;
using Microsoft.Extensions.Logging;

namespace Aitive.Framework.Plugins.Hosts;

public static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Binding plugins with: {Binder}")]
    public static partial void BindPlugins(this ILogger logger, Type binder);
}

internal sealed class DefaultPluginHost : IPluginHost
{
    private readonly IReadOnlyList<IPlugin> _plugins;
    private readonly ILogger _logger;

    internal DefaultPluginHost(IReadOnlyList<IPlugin> plugins, ILogger logger)
    {
        _logger = logger;
        _plugins = plugins.ToList();
    }

    public IReadOnlyList<PluginManifest> Plugins => _plugins.Select(p => p.Manifest).ToList();

    public IPluginBindPoint Bind(IPluginBindPointBuilder builder)
    {
        _logger.BindPlugins(builder.GetType());
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
