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
            return pluginHost.BindPluginServices(services, new Dictionary<Type, object>());
        }

        public IPluginBindPoint BindPluginServices(
            IServiceCollection services,
            IReadOnlyDictionary<Type, object> initialServices
        )
        {
            return pluginHost.Bind(new ServicePluginBindPointBuilder(services, initialServices));
        }
    }

    extension(IServiceCollection services)
    {
        public IPluginBindPoint BindPluginServices(IPluginHost pluginHost)
        {
            return services.BindPluginServices(pluginHost, new Dictionary<Type, object>());
        }

        public IPluginBindPoint BindPluginServices(
            IPluginHost pluginHost,
            IReadOnlyDictionary<Type, object> initialServices
        )
        {
            return pluginHost.BindPluginServices(services, initialServices);
        }
    }
}
