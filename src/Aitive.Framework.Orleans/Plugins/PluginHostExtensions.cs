using Aitive.Framework.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.Orleans.Plugins;

public static class PluginHostExtensions
{
    extension(IPluginHost pluginHost)
    {
        public IPluginBindPoint BindAssemblyPluginGrains(IServiceCollection services)
        {
            var builder = new OrleansAssemblyPluginBindPointBuilder(services);
            return pluginHost.Bind(builder);
        }

        public IPluginBindPoint BindSiloBuilderPluginGrains(ISiloBuilder siloBuilder)
        {
            return pluginHost.BindSiloBuilderPluginGrains(
                siloBuilder,
                new Dictionary<Type, object>()
            );
        }

        public IPluginBindPoint BindSiloBuilderPluginGrains(
            ISiloBuilder siloBuilder,
            IReadOnlyDictionary<Type, object> initialServices
        )
        {
            return pluginHost.Bind(
                new OrleansSiloBuilderPluginBindPointBuilder(siloBuilder, initialServices)
            );
        }
    }
}
