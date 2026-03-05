using Aitive.Framework.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.Orleans.Plugins;

public static class PluginHostExtensions
{
    extension(IPluginHost pluginHost)
    {
        public IPluginBindPoint BindPluginGrains(IServiceCollection services)
        {
            var builder = new OrleansPluginBindPointBuilder(services);
            return pluginHost.Bind(builder);
        }
    }

    extension(IServiceCollection services)
    {
        public IPluginBindPoint BindPluginGrains(IPluginHost pluginHost)
        {
            return pluginHost.BindPluginGrains(services);
        }
    }
}
