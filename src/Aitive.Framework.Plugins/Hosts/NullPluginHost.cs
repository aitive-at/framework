using Aitive.Framework.Plugins.Binding;

namespace Aitive.Framework.Plugins.Hosts;

public sealed class NullPluginHost : IPluginHost
{
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public IReadOnlyList<PluginManifest> Plugins { get; } = [];

    public IPluginBindPoint Bind(IPluginBindPointBuilder builder)
    {
        return new EmptyPluginBindPoint();
    }
}
