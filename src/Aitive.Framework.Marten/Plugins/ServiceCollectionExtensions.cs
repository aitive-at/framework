using Aitive.Framework.Configuration;
using Aitive.Framework.Plugins;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aitive.Framework.Marten.Plugins;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public MartenServiceCollectionExtensions.MartenConfigurationExpression ConfigureMartenWithPlugins(
            IPluginHost pluginHost,
            string connectionStringKey
        )
        {
            return services.AddMarten(
                (serviceProvider) =>
                {
                    var storeOptions = new StoreOptions();
                    var configurations = serviceProvider.GetServices<
                        IConfigureOptions<StoreOptions>
                    >();
                    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                    var connectionString = configuration.GetRequiredConnectionString(
                        connectionStringKey
                    );

                    storeOptions.Connection(connectionString);

                    foreach (var configurationStep in configurations)
                    {
                        configurationStep.Configure(storeOptions);
                    }

                    _ = pluginHost.Bind(new MartenPluginBindPointBuilder(storeOptions));

                    return storeOptions;
                }
            );
        }
    }
}
