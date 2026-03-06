using Aitive.Framework.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.Configuration.Plugins;

public static class PluginHostExtensions
{
    extension(IPluginHost pluginHost)
    {
        public IPluginBindPoint BindPluginConfigurationOptions(
            IServiceCollection services,
            IConfiguration configuration
        )
        {
            var builder = new ConfigurationPluginBindPointBuilder(services, configuration);

            return pluginHost.Bind(builder);
        }
    }

    extension(IServiceCollection services)
    {
        public IPluginBindPoint BindPluginConfigurationOptions(
            IPluginHost pluginHost,
            IConfiguration configuration
        )
        {
            return pluginHost.BindPluginConfigurationOptions(services, configuration);
        }
    }
}
