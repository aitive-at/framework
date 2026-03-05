using Aitive.Framework.Plugins.Binding;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.Plugins;

public interface IPluginHost : IAsyncDisposable
{
    IReadOnlyList<PluginManifest> Plugins { get; }

    IPluginBindPoint Bind(IPluginBindPointBuilder builder);
}

public static class PluginHostExtensions
{
    extension(IPluginHost pluginHost)
    {
        public IPluginBindPoint BindPluginServices(IServiceCollection services)
        {
            return pluginHost.Bind(new ServicePluginBindPointBuilder(services));
        }
    }

    extension(IServiceCollection services)
    {
        public IPluginBindPoint BindPluginServices(IPluginHost pluginHost)
        {
            return pluginHost.BindPluginServices(services);
        }
    }
}
