using Aitive.Framework.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YesSql;
using YesSql.Indexes;
using YesSql.Provider.PostgreSql;

namespace Aitive.Framework.YesSql;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddYesSql(
            YesSqlDatabaseType yesSqlDatabaseType,
            string connectionStringKey,
            Action<IServiceProvider, global::YesSql.Configuration>? configure = null
        )
        {
            services.AddSingleton<IStore>(sp =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringKey);

                var connectionString =
                    sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()
                        .GetRequiredConnectionString(connectionStringKey);

                var configuration = new global::YesSql.Configuration();

                switch (yesSqlDatabaseType)
                {
                    case YesSqlDatabaseType.Postgres:
                        configuration.UsePostgreSql(connectionString);
                        break;
                }

                configure?.Invoke(sp, configuration);

                var store = StoreFactory.Create(configuration);
                var indices = sp.GetServices<IIndexProvider>();

                store.RegisterIndexes(indices);
                store.InitializeAsync().GetAwaiter().GetResult();

                return store;
            });
        }
    }
}
