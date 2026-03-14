using System.Text.Json;
using Aitive.Framework.Configuration;
using Aitive.Framework.Plugins;
using Marten;
using Marten.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Weasel.Core;

namespace Aitive.Framework.Marten.Plugins;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public MartenServiceCollectionExtensions.MartenConfigurationExpression ConfigureMartenWithPlugins(
            IPluginHost pluginHost,
            string connectionStringKey,
            bool useGlobalJsonSerializer = true
        )
        {
            return services.AddMarten(
                (serviceProvider) =>
                {
                    var storeOptions = new StoreOptions();

                    if (useGlobalJsonSerializer)
                    {
                        storeOptions.Serializer(
                            new SystemTextJsonSerializer(
                                serviceProvider.GetRequiredService<JsonSerializerOptions>()
                            )
                            {
                                Casing = Casing.CamelCase,
                                EnumStorage = EnumStorage.AsString,
                            }
                        );
                    }

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
